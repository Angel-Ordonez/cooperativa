using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cooperativa.App.CRUD
{
    public class ReporteDashboardCrud
    {


        public class ReporteDashboardGananciasRetirosGlobal
        {
            public class ReporteVm
            {
                public decimal GananciaGlobal { get; set; }
                public decimal RetirosGlobal { get; set; }
                public decimal GananciaGlobalNeta { get; set; }
                public List<GananciaRetiroAnioVm> GananciaAndRetirosAnios { get; set; }
                public List<GananciaAnioModuloVm> GananciasAnioModulo { get; set; }
            }
            public class GananciaRetiroAnioVm
            {
                public int Anio { get; set; }
                public decimal TotalGanancia { get; set; }
                public decimal TotalRetirado { get; set; }
                public decimal TotalGananciaNeta { get; set; }
                public List<GananciaRetiroMesVm> Meses { get; set; }
            }
            public class GananciaRetiroMesVm
            {
                public DateTime Fecha { get; set; }
                public string MesDescripcion { get; set; }
                public decimal GananciaMes { get; set; }
                public decimal RetiradoMes { get; set; }
                public decimal GananciaTotalNetaMes { get; set; }
            }
            public class GananciaAnioModuloVm
            {
                public string Modulo { get; set; }
                public int Anio { get; set; }
                public decimal TotalGanancia { get; set; }
                public List<GananciaRetiroMesModuloVm> Meses { get; set; }
            }

            public class GananciaRetiroMesModuloVm
            {
                public DateTime Fecha { get; set; }
                public string MesDescripcion { get; set; }
                public decimal GananciaMes { get; set; }
            }


            public class QueryPrestamosIndex : IRequest<ReporteVm>
            {

            }
            public class QueryPrestamosIndexHandler : IRequestHandler<QueryPrestamosIndex, ReporteVm>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosIndexHandler(CooperativaDbContext context)
                {
                    _context = context;
                }
                public async Task<ReporteVm> Handle(QueryPrestamosIndex query, CancellationToken cancellationToken)
                {
                    var prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.Estado == EstadoPrestamo.Pagado || x.Estado == EstadoPrestamo.Vigente)
                        .AsNoTracking()
                        .ToListAsync();

                    var prestamosIds = prestamos.Select(x => x.Id).ToList();
                    var prestamosDetalles = await _context.PrestamoDetalle.Where(x => !x.IsSoftDeleted && prestamosIds.Contains(x.PrestamoId))
                        .AsNoTracking()
                        .ToListAsync();

                    var anios = prestamos.Select(x => x.CreatedDate.Year).Distinct().ToList();
                    var estados = prestamos.Select(x => x.Estado).Distinct().ToList();


                    var retiros = await _context.Retiro.Where(x => x.Estado == EstadoRetiro.Aprobado && !x.IsSoftDeleted && x.Enabled).ToListAsync();


                    var reporteRes = new ReporteVm
                    {
                        GananciaAndRetirosAnios = new List<GananciaRetiroAnioVm>(),
                        GananciasAnioModulo = new List<GananciaAnioModuloVm>()
                    };
                    var gananciasRetirosAnio = new List<GananciaRetiroAnioVm>();
                    var gananciasAnioModulos = new List<GananciaAnioModuloVm>();

                    for (int i = 0; i < anios.Count(); i++)
                    {
                        var anio = anios.ElementAt(i);

                        var mesesAnio = prestamos.Where(x => x.CreatedDate.Year == anio).Select(x => x.CreatedDate.Month).Distinct().ToList();
                        var mesesRetiros = retiros.Where(x => x.CreatedDate.Year == anio).Select(x => x.CreatedDate.Month).Distinct().ToList();

                        mesesAnio.AddRange(mesesRetiros);
                        mesesAnio = mesesAnio.Distinct().ToList();

                        var gananciaRetiroAnio = new GananciaRetiroAnioVm
                        {
                            Anio = anio,    
                            Meses = new List<GananciaRetiroMesVm>()
                        };

                        var gananciaRetiroAnioModulo = new GananciaAnioModuloVm
                        {
                            Modulo = "Prestamo PIM",
                            Anio = anio,
                            Meses = new List<GananciaRetiroMesModuloVm>()
                        };

                        for (int m = 0; m < mesesAnio.Count(); m++)
                        {
                            var mes = mesesAnio.ElementAt(m);

                            var prestamosMesAnio = prestamos.Where(x => x.CreatedDate.Year == anio && x.CreatedDate.Month == mes).ToList();
                            var prestamosMesAnioIds = prestamosMesAnio.Select(x => x.Id).ToList();
                            var prestamosDetallesMesAnio = prestamosDetalles.Where(x => x.CreatedDate.Year == anio && x.CreatedDate.Month == mes).ToList();
                            
                            var gananciasMes = prestamosDetallesMesAnio.Sum(x => x.MontoInteres);

                            var retirosMesAnio = retiros.Where(x => x.CreatedDate.Year == anio && x.CreatedDate.Month == mes).ToList();
                            var totalRetiroMes = retirosMesAnio.Sum(x => x.Monto);


                            #region Mostrar hasta que dia del mes se saco el reporte
                            var hoy = DateTime.Today;
                            DateTime fecha;

                            if (anio == hoy.Year && mes == hoy.Month)
                            {
                                // Mes actual → usar el día de hoy
                                fecha = new DateTime(anio, mes, hoy.Day);
                            }
                            else
                            {
                                // Mes pasado → usar último día del mes
                                int ultimoDia = DateTime.DaysInMonth(anio, mes);
                                fecha = new DateTime(anio, mes, ultimoDia);
                            }

                            #endregion

                            var gananciaReritoMes = new GananciaRetiroMesVm
                            {
                                Fecha = fecha,
                                MesDescripcion = MesDescripcion.GetMesTexto(mes),
                                GananciaMes = gananciasMes,
                                RetiradoMes = totalRetiroMes,
                                GananciaTotalNetaMes = gananciasMes - totalRetiroMes
                            };
                            gananciaRetiroAnio.Meses.Add(gananciaReritoMes);
                            gananciaRetiroAnioModulo.Meses.Add(gananciaReritoMes.Adapt<GananciaRetiroMesModuloVm>());
                        }

                        gananciaRetiroAnio.TotalGanancia = gananciaRetiroAnio.Meses.Sum(x => x.GananciaMes);
                        gananciaRetiroAnio.TotalRetirado = gananciaRetiroAnio.Meses.Sum(x => x.RetiradoMes);
                        gananciaRetiroAnio.TotalGananciaNeta = gananciaRetiroAnio.TotalGanancia - gananciaRetiroAnio.TotalRetirado;
                        //Modulos
                        gananciaRetiroAnioModulo.TotalGanancia = gananciaRetiroAnioModulo.Meses.Sum(x => x.GananciaMes);



                        gananciasRetirosAnio.Add(gananciaRetiroAnio);
                        gananciasAnioModulos.Add(gananciaRetiroAnioModulo);
                    }


                    //Si hay otro modulo, solo lo agrego (Aqui analizo si es neecsario crear objetos de  GananciaRetiroMesVm (Aqui mas bien seria ir a buscar por Año y Mes y sumarlo, y creo el objeto GananciaRetiroAnioModuloVm



                    var respuesta = new ReporteVm
                    {
                        GananciaAndRetirosAnios = gananciasRetirosAnio,
                        GananciasAnioModulo = gananciasAnioModulos,
                    };
                    respuesta.GananciaGlobal = respuesta.GananciaAndRetirosAnios.Sum(x => x.TotalGanancia);
                    respuesta.RetirosGlobal = respuesta.GananciaAndRetirosAnios.Sum(x => x.TotalRetirado);
                    respuesta.GananciaGlobalNeta = respuesta.GananciaAndRetirosAnios.Sum(x => x.TotalGananciaNeta);


                    return respuesta;
                }
            }
        }










































    }
}

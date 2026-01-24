using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Domain.Model.Prestamos;

namespace Cooperativa.App.CRUD
{
    public class GananciaDetalleSocioCrud
    {


        public class CrearByPrestamoId
        {
            public class CommandCGA : IRequest<AppResult>
            {
                public Guid PrestamoId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandCGA, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCGA cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var pagosPrestamo = await _context.PrestamoDetalle.Where(x => x.PrestamoId == cmd.PrestamoId && !x.IsSoftDeleted).ToListAsync();
                        var sumaInteres = pagosPrestamo.Sum(x => x.MontoInteres);

                        var detallesSociosInversiones = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == cmd.PrestamoId && !x.IsSoftDeleted)
                            .Include(x => x.SocioInversion)
                            .Include(x => x.SocioInversion).ThenInclude(x => x.Socio)
                            .ToListAsync();

                        List<GananciaDetalleSocio> nuevasGananciasDetalle = new List<GananciaDetalleSocio>();

                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        if (detallesSociosInversiones.Any())
                        {
                            var detallesSociosInversionIds = detallesSociosInversiones.Select(x => x.Id).ToList();
                            var gananciasDetalleActuales = await _context.GananciaDetalleSocio.Where(x => detallesSociosInversionIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted && x.Enabled).ToListAsync();

                            foreach (var detalle in detallesSociosInversiones)
                            {
                                var socio = detalle.SocioInversion.Socio;
                                if (socio == null)
                                {
                                    socio = await _context.Socio.Where(x => x.Id == detalle.SocioInversion.SocioId).FirstOrDefaultAsync();
                                }

                                //var newCalculoGanancia = sumaInteres * (detalle.PorcentajeEnPrestamo / 100);  //Este funciona si el socio ganara 100% del interes, pero cuando se crea un Socio se le pone cuando es el % de ganancia
                                var gananciaPrevia = sumaInteres * (detalle.PorcentajeEnPrestamo / 100);
                                var newCalculoGanancia = gananciaPrevia * (socio.PorcentajeGanancia / 100);

                                var newGananciaDetalle = GananciaDetalleSocio.New(detalle.Id, sumaInteres, newCalculoGanancia, createdBy);

                                var gananciaActual = gananciasDetalleActuales.Where(x => x.DetalleSocioInversionId == detalle.Id).OrderByDescending(x => x.CreatedDate).FirstOrDefault();


                                if (gananciaActual != null)
                                {
                                    newGananciaDetalle.Retirado = gananciaActual.Retirado;
                                    newGananciaDetalle.CantidadRetirada = gananciaActual.CantidadRetirada;
                                    newGananciaDetalle.CantidadDisponibleARetirar = newGananciaDetalle.Ganancia - gananciaActual.CantidadRetirada;

                                    //Al anterior la deshabulito porque la sustituye la nueva
                                    newGananciaDetalle.GananciaDetalleSocioAnteriorId = gananciaActual.Id;
                                    gananciaActual.Enabled = false;
                                    gananciaActual.ModifiedBy = createdBy;
                                    gananciaActual.ModifiedDate = DateTime.Now;
                                    gananciaActual.Observacion = "Se sustituyo por: " + newGananciaDetalle.Id.ToString();
                                }

                                await _context.GananciaDetalleSocio.AddAsync(newGananciaDetalle);

                                detalle.GananciaDetalleSocioId = newGananciaDetalle.Id;

                                nuevasGananciasDetalle.Add(newGananciaDetalle);
                            }
                        }


                        if (nuevasGananciasDetalle.Any())
                        {
                            //await _context.GananciaDetalleSocio.AddRangeAsync(nuevasGananciasDetalle);
                            await _context.SaveChangesAsync();
                            return AppResult.New(true, "Mantenimiento exitoso!");
                        }
                        else
                        {
                            return AppResult.New(true, "No hay calculos de ganacias que hacer");
                        }






                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }

























    }
}

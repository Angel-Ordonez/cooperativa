using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cooperativa.App.Engine;
using static Cooperativa.App.CRUD.EngineCrud;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Mapster;

namespace Cooperativa.App.CRUD
{
    public class HistorialCambioMonedaCrud
    {

        public class Create
        {
            public class CommandHistorialMonedaCreate : IRequest<AppResult>
            {
   
            }

            public class CommandHandler : IRequestHandler<CommandHistorialMonedaCreate, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public CommandHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<AppResult> Handle(CommandHistorialMonedaCreate command, CancellationToken cancellationToken)
                {
                    var resu = await _exchangeratesService.GetExchangeRate();
                    var paises = MonedaValorVm.ObtenerMonedas();

                    if (resu.Success)
                    {
                        var historialActivos = await _context.HistorialCambioMoneda.Where(x => !x.IsSoftDeleted && x.Enabled).ToListAsync();

                        foreach(var item  in historialActivos)
                        {
                            item.Enabled = false;
                            item.ModifiedDate = DateTime.Now;
                        }


                        foreach (var kvp in resu.Rates)
                        {
                            var moneda = paises.Where(x => x.Moneda == kvp.Key).FirstOrDefault();
                            if (moneda != null)
                            {
                                moneda.Valor = kvp.Value;
                            }
                        }

                        var espana = paises.Where(x => x.Pais.ToLower() == "españa").FirstOrDefault();
                        var usa = paises.Where(x => x.Pais.ToLower() == "usa").FirstOrDefault();
                        var mexico = paises.Where(x => x.Pais.ToLower() == "mexico").FirstOrDefault();
                        var nicaragua = paises.Where(x => x.Pais.ToLower() == "nicaragua").FirstOrDefault();
                        var costaRica = paises.Where(x => x.Pais.ToLower() == "costa rica").FirstOrDefault();
                        var honduras = paises.Where(x => x.Pais.ToLower() == "honduras").FirstOrDefault();
                        var guatemala = paises.Where(x => x.Pais.ToLower() == "guatemala").FirstOrDefault();

                        var newHistorialMoneda = HistorialCambioMoneda.New(
                            resu.Base,
                            espana != null ? espana.Valor : 0,
                            usa != null ? usa.Valor : 0,
                            honduras != null ? honduras.Valor : 0,
                            guatemala != null ? guatemala.Valor : 0,
                            mexico != null ? mexico.Valor : 0,
                            nicaragua != null ? nicaragua.Valor : 0,
                            costaRica != null ? costaRica.Valor : 0
                            );

                        await _context.HistorialCambioMoneda.AddAsync(newHistorialMoneda);
                        await _context.SaveChangesAsync();
                    }

                    return AppResult.New(true, paises);
                }
            }
        }


        public class Delete
        {
            public class CommandHistorialMonedaDelete : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Observacion { get; set; }
                public Guid ModifiedBy { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandHistorialMonedaDelete, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public CommandHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<AppResult> Handle(CommandHistorialMonedaDelete command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var historialAeliminar = await _context.HistorialCambioMoneda.Where(x => x.Id == command.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        if(historialAeliminar != null)
                        {
                            var fechaAnioAnterior = DateTime.Now;
                            fechaAnioAnterior = fechaAnioAnterior.AddYears(-1);

                            var penultimo = await _context.HistorialCambioMoneda.Where(x => x.CreatedDate.Year == DateTime.Now.Year && !x.IsSoftDeleted && !x.Enabled)
                                .OrderByDescending(s => s.CreatedDate)
                                .FirstOrDefaultAsync();

                            if(penultimo == null)
                            {
                                penultimo = await _context.HistorialCambioMoneda.Where(x => x.CreatedDate.Year == fechaAnioAnterior.Year && !x.IsSoftDeleted && !x.Enabled)
                                    .OrderByDescending(s => s.CreatedDate)
                                    .FirstOrDefaultAsync();
                            }

                            if(penultimo == null)
                            {
                                throw new Exception($"No es posible eliminar, porque no se encontro un penultimo historial entre {DateTime.Now.Year} y {fechaAnioAnterior.Year}");
                            }


                            historialAeliminar.Observacion = command.Observacion + " | Lo sustituyo: " + penultimo.Id.ToString();
                            historialAeliminar.IsSoftDeleted = true;
                            historialAeliminar.Enabled = false;
                            historialAeliminar.ModifiedDate = DateTime.Now;
                            historialAeliminar.ModifiedBy = command.ModifiedBy;
                            
                            penultimo.Enabled = true;

                            await _context.SaveChangesAsync();

                            return AppResult.New(true, "Transaccion existosa. Lo sustituyo: " + penultimo.Id.ToString());
                        }
                        else
                        {
                            return AppResult.New(false, $"No se encontro HistorialCambioMoneda");

                        }

                    }
                    catch(Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                    
                }
            }
        }



        public class Index
        {
            public class Query : IRequest<List<HistorialCambioMonedaMonedaVm>>
            {

            }

            public class QueryHandler : IRequestHandler<Query, List<HistorialCambioMonedaMonedaVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<List<HistorialCambioMonedaMonedaVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var index = await _context.HistorialCambioMoneda.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<HistorialCambioMonedaMonedaVm>()
                        .ToListAsync(cancellationToken);

                    return index.OrderByDescending(x => x.CreatedDate).ToList();
                }
            }
        }



        public class GetByFecha
        {
            public class Query : IRequest<List<HistorialCambioMonedaMonedaVm>>
            {
                public DateTime Fecha { get; set; }
            }

            public class QueryHandler : IRequestHandler<Query, List<HistorialCambioMonedaMonedaVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<List<HistorialCambioMonedaMonedaVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var index = await _context.HistorialCambioMoneda.Where(x => !x.IsSoftDeleted && x.CreatedDate.Date == query.Fecha.Date)
                        .AsNoTracking()
                        .ProjectToType<HistorialCambioMonedaMonedaVm>()
                        .ToListAsync(cancellationToken);

                    return index.OrderByDescending(x => x.CreatedDate).ToList();
                }
            }
        }



        public class GetByRangoFechas
        {
            public class Query : IRequest<List<HistorialCambioMonedaMonedaVm>>
            {
                public DateTime FechaInicio { get; set; }
                public DateTime FechaFin { get; set; }
            }

            public class QueryHandler : IRequestHandler<Query, List<HistorialCambioMonedaMonedaVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<List<HistorialCambioMonedaMonedaVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var index = await _context.HistorialCambioMoneda.Where(x => !x.IsSoftDeleted && (x.CreatedDate.Date >= query.FechaInicio.Date && x.CreatedDate.Date <= query.FechaFin.Date))
                        .AsNoTracking()
                        .ProjectToType<HistorialCambioMonedaMonedaVm>()
                        .ToListAsync(cancellationToken);

                    return index.OrderByDescending(x => x.CreatedDate).ToList();
                }
            }
        }




        //Faltaria un metodo donde manden a convertir una cantidad... donde se reciba las 2 monedas y la fecha de ese Historial 







        public class IndexActualApi
        {
            public class CommandHistorialMonedaIndexApi : IRequest<AppResult>
            {

            }

            public class CommandHandler : IRequestHandler<CommandHistorialMonedaIndexApi, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public CommandHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<AppResult> Handle(CommandHistorialMonedaIndexApi command, CancellationToken cancellationToken)
                {
                    var resu = await _exchangeratesService.GetExchangeRate();

                    return AppResult.New(true, resu);
                }
            }
        }

















































        public class MonedaValorVm
        {
            public string Pais { set; get; }
            public string Moneda { set; get; }
            public decimal Valor { set; get; }


            // Método estático para devolver la lista de monedas
            public static List<MonedaValorVm> ObtenerMonedas()
            {
                return new List<MonedaValorVm>
                {
                    new MonedaValorVm { Pais = "España", Moneda = "EUR", Valor = 0 },
                    new MonedaValorVm { Pais = "USA", Moneda = "USD", Valor = 0 },
                    new MonedaValorVm { Pais = "Mexico", Moneda = "MXN", Valor = 0 },
                    new MonedaValorVm { Pais = "Nicaragua", Moneda = "NIO", Valor = 0 },
                    new MonedaValorVm { Pais = "Costa Rica", Moneda = "CRC", Valor = 0 },
                    new MonedaValorVm { Pais = "Honduras", Moneda = "HNL", Valor = 0 },
                    new MonedaValorVm { Pais = "Guatemala", Moneda = "GTQ", Valor = 0 },
                };
            }

        }



        public class HistorialCambioMonedaMonedaVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            //public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            //public DateTime ModifiedDate { get; set; }
            public bool IsSoftDeleted { get; set; }
            public bool Enabled { get; set; }
            public string MonedaBase { get; set; }      //Es por api que consumo, toma de arranque EUR
            public decimal Euro { get; set; }
            public decimal Dolar { get; set; }
            public decimal Lempira { get; set; }
            public decimal Quetzal { get; set; }
            public decimal MXN { get; set; }
            public decimal CordobaNicaragua { get; set; }    //Nicaragua
            public decimal ColonCostaRica { get; set; }     //Costa Rica
        }









    }
}

using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Engine;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.HistorialCambioMonedaCrud;
using static Cooperativa.App.Domain.Model.EntidadesUtiles.TasasCambioDivisa;

namespace Cooperativa.App.CRUD
{
    public class TasasCambioDivisaCrud
    {



        public class ActualizarDivisasConCurrencyfreaks
        {
            public class Query : IRequest<AppResult>
            {

            }

            public class QueryHandler : IRequestHandler<Query, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<AppResult> Handle(Query query, CancellationToken cancellationToken)
                {
                    var actualizar = await _exchangeratesService.ActualizarDivisasConCurrencyfreaks();
                    return actualizar;
                }
            }
        }




        public class GetByDivisas
        {
            public class Query : IRequest<List<TasasCambioDivisaVm>>
            {
                public List<string> Divisas { get; set; }
            }

            public class QueryHandler : IRequestHandler<Query, List<TasasCambioDivisaVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<List<TasasCambioDivisaVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var index = await _context.TasasCambioDivisa.Where(x => query.Divisas.Contains(x.MonedaDestino) && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<TasasCambioDivisaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }





        public class GetTasasCambioDivisaActivas
        {
            public class Query : IRequest<List<TasasCambioDivisaVm>>
            {

            }

            public class QueryHandler : IRequestHandler<Query, List<TasasCambioDivisaVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandler(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<List<TasasCambioDivisaVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var index = await _context.TasasCambioDivisa.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .ProjectToType<TasasCambioDivisaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }














    }
}

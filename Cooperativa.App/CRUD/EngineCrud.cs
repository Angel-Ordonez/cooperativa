using Cooperativa.App.Domain.Data;
using Cooperativa.App.Engine;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.CajaCrud;
using static Cooperativa.App.CRUD.EngineCrud;

namespace Cooperativa.App.CRUD
{
    public class EngineCrud
    {
        public class ConversionMoneda
        {
            public class QueryConversionMoneda : IRequest<List<ConversionMonedaVm>>
            {
                public string MonedaAConvertir { set; get; }
            }

            public class QueryHandlerConversionMoneda : IRequestHandler<QueryConversionMoneda, List<ConversionMonedaVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IExchangeratesService _exchangeratesService;

                public QueryHandlerConversionMoneda(CooperativaDbContext context, IExchangeratesService exchangeratesService)
                {
                    _context = context;
                    _exchangeratesService = exchangeratesService;
                }

                public async Task<List<ConversionMonedaVm>> Handle(QueryConversionMoneda query, CancellationToken cancellationToken)
                {

                    if(query.MonedaAConvertir.Length > 3 || query.MonedaAConvertir.Length < 1)
                    {
                        throw new ArgumentException("La moneda a convertir debe tener 3 caracteres. Por ejemplo: HNL, MXN, etc");
                    }

                    var resu = await _exchangeratesService.GetExchangeRate();

                    List<MonedaValorVm> monedasValores = new List<MonedaValorVm>();
                    foreach (var kvp in resu.Rates)
                    {
                        if(  kvp.Key == "EUR" || kvp.Key == "USD" || kvp.Key == query.MonedaAConvertir.ToUpper())
                        {
                            var monedaValor = new MonedaValorVm
                            {
                                Moneda = kvp.Key,
                                Valor = Math.Round(kvp.Value, 2)
                            };
                            monedasValores.Add(monedaValor);
                        }

                    }


                    var euro = monedasValores.Where(x => x.Moneda == "EUR").FirstOrDefault();
                    var dolar = monedasValores.Where(x => x.Moneda == "USD").FirstOrDefault();
                    var monedaAConvertir = monedasValores.Where(x => x.Moneda == query.MonedaAConvertir.ToUpper()).FirstOrDefault();

                    List<ConversionMonedaVm> conversiones = new List<ConversionMonedaVm>();

                    if(monedaAConvertir != null)
                    {
                        if (dolar != null && monedaAConvertir != null)
                        {

                            var newResEuro = new ConversionMonedaVm
                            {
                                MonedaExtrajera = euro.Moneda,
                                MonedaAConvertir = monedaAConvertir.Moneda,
                                Conversion = monedaAConvertir.Valor
                            };
                            conversiones.Add(newResEuro);

                        }


                        if (dolar != null && euro != null && monedaAConvertir != null)
                        {
                            //1. Convertir 1 dolar a Euro
                            var dolarAEuro = Math.Round(1.0m / dolar.Valor, 4);
                            //2. COnvertir 1 dolar a moneda del cliente
                            var dolarAMonedaAConvertir = dolarAEuro * monedaAConvertir.Valor;  

                            var newResDolar = new ConversionMonedaVm
                            {
                                MonedaExtrajera = dolar.Moneda,
                                MonedaAConvertir = monedaAConvertir.Moneda,
                                Conversion = (decimal)Math.Round(dolarAMonedaAConvertir, 2)
                            };

                            conversiones.Add(newResDolar);

                        }
                    }
                    else
                    {
                        throw new ArgumentException($"No se encontro Moneda: {query.MonedaAConvertir}");
                    }



                    return conversiones;

                }


            }


        }



        public class ConversionMonedaVm
        {
            public string MonedaExtrajera { set; get; }
            public string MonedaAConvertir { set; get; }
            public decimal Conversion { set; get; }
        }

        public class MonedaValorVm
        {
            public string Moneda { set; get; }
            public decimal Valor { set; get; }
        }




    }
}

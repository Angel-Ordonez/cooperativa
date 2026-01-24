using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.EntidadesUtiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cooperativa.App.Engine
{
    public interface IExchangeratesService
    {
        Task<ExchangeRatesData> GetExchangeRate();                      //Para ir a traer cuanto vale las monedas, tomando como referencia el EUR
        Task<CurrencyFreaksData> GetDivisasCurrencyfreaks();            //tomando como referencia el USD
        Task<AppResult> ActualizarDivisasConCurrencyfreaks();
    }

    public class ExchangeratesService : IExchangeratesService
    {

        private readonly CooperativaDbContext _context;

        public ExchangeratesService(CooperativaDbContext bd)
        {
            _context = bd;
        }
        public async Task<ExchangeRatesData> GetExchangeRate()
        {
            try
            {

                //Esta api me trae en base a 1 EURO las conversiones
                HttpClient client = new HttpClient();
                string apiKey = "429ba309a94555a8fa4614a31d0e6883";
                string apiUrl = $"http://api.exchangeratesapi.io/v1/latest?access_key={apiKey}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var exchangeRatesData = JsonSerializer.Deserialize<ExchangeRatesData>(responseContent, options);
                    var resjson = exchangeRatesData.ToJson();
                    return exchangeRatesData;
                }
                else
                {
                    throw new HttpRequestException($"Error en la solicitud HTTP: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return new ExchangeRatesData
                {
                    Success = false,
                    Error = ex.Message
                };
                //throw new Exception($"Error al obtener la tasa de cambio: {ex.Message}");
            }
        }


        public async Task<CurrencyFreaksData> GetDivisasCurrencyfreaks()
        {
            try
            {
                HttpClient client = new HttpClient();
                string apiKey = "586755c7c8144a8f8a997b734a7e019f";     //Se reinicia cada 1 de cada mes, y se tiene 1000 llamadas gratis. USD es la moneda base
                string apiUrl = $"https://api.currencyfreaks.com/v2.0/rates/latest?apikey={apiKey}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var exchangeRatesData = JsonSerializer.Deserialize<CurrencyFreaksData>(responseContent, options);

                    return exchangeRatesData;
                }
                else
                {
                    throw new HttpRequestException($"Error en la solicitud HTTP: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return new CurrencyFreaksData
                {
                    Error = ex.Message
                };
            }
        }






        public async Task<AppResult> ActualizarDivisasConCurrencyfreaks()
        {
            try
            {
                var monedas = TasasCambioDivisa.Divisas;

                var divisasExistentes = await _context.TasasCambioDivisa.Where(x => monedas.Contains(x.MonedaDestino) && !x.IsSoftDeleted && x.Enabled).ToListAsync();
                foreach(var item  in divisasExistentes)
                {
                    item.Enabled = false;
                    item.ModifiedDate = DateTime.Now;
                    item.Observacion += "***Sustiuido por nueva actualización";
                }


                List<TasasCambioDivisa> tasasCambioDivisas = new List<TasasCambioDivisa>();
                var currencyfreaks = GetDivisasCurrencyfreaks();
                var currencyfreaksResultado = currencyfreaks.Result;

                foreach(var mon in monedas)
                {

                    if (currencyfreaksResultado.Rates.ContainsKey(mon))
                    {
                        var tasa = currencyfreaksResultado.Rates[mon];
                        var dolarMoneda = currencyfreaksResultado.Rates.Where(x => x.Key == mon).FirstOrDefault();

                        var newTasaDivisa = TasasCambioDivisa.New(DateTime.Parse(currencyfreaks.Result.Date), currencyfreaks.Result.Base, mon, Convert.ToDecimal(tasa), "Currencyfreaks", "");

                        var tasaActual = divisasExistentes.Where(x => x.MonedaDestino == mon).FirstOrDefault();
                        if(tasaActual != null)
                        {
                            var diferencia = newTasaDivisa.Tasa - tasaActual.Tasa;
                            newTasaDivisa.TasaAnterior = tasaActual.Tasa;
                            newTasaDivisa.Diferencia = diferencia;
                        }


                        tasasCambioDivisas.Add(newTasaDivisa);
                    }
                }
                await _context.TasasCambioDivisa.AddRangeAsync(tasasCambioDivisas);
                await _context.SaveChangesAsync();
                return AppResult.New(true, "Transacción exitosa!");

            }
            catch (Exception ex)
            {
                return AppResult.New(false, ex.Message);
            }
        }
































    }




























    public class ExchangeRatesData
    {
        public bool Success { get; set; }
        public long Timestamp { get; set; }
        public string Base { get; set; }
        public string Date { get; set; }
        public string Error { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }

        internal object ToJson()
        {
            throw new NotImplementedException();
        }
    }


    //Segunda api
    public class CurrencyFreaksData
    {
        public string Date { get; set; }
        public string Base { get; set; }
        public Dictionary<string, string> Rates { get; set; }
        public string Error { get; set; }
    }



}
using Cooperativa.App.CRUD;
using Cooperativa.App.Engine;
using Cooperativa.App.Utilidades;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

[Route("api/[action]")]
public class ExchangeRatesController : Controller
{
    private readonly IMediator _mediator;
    private readonly IExchangeratesService _exchangeratesService;
    private readonly IUtilidadesBase _iUtilidadesBase;

    public ExchangeRatesController(IMediator mediator, IExchangeratesService exchangeratesService, IUtilidadesBase utilidadesBase)
    {
        _mediator = mediator;
        _exchangeratesService = exchangeratesService;
        _iUtilidadesBase = utilidadesBase;
    }


    [HttpGet]
    public async Task<IActionResult> ConversionMoneda([FromQuery] EngineCrud.ConversionMoneda.QueryConversionMoneda query)
    {
        var res = await _mediator.Send(query); ;
        return Ok(res);
    }



    [HttpGet]           //Consumir directamente el Engine
    public async Task<IActionResult> GetExchangeRates()
    {
        try
        {
            var exchangeRatesData = await _exchangeratesService.GetExchangeRate();
            return Ok(exchangeRatesData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener las tasas de cambio: {ex.Message}");
        }
    }

    [HttpGet]           //Consumir directamente el Engine
    public async Task<IActionResult> GetDivisasCurrencyfreaks()
    {
        try
        {
            var exchangeRatesData = await _exchangeratesService.GetDivisasCurrencyfreaks();
            return Ok(exchangeRatesData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al obtener las tasas de cambio: {ex.Message}");
        }
    }


    [HttpPost]           //Consumir directamente el Engine
    public async Task<IActionResult> ActualizarDivisasConCurrencyfreaks()
    {
        var exchangeRatesData = await _exchangeratesService.ActualizarDivisasConCurrencyfreaks();
        return Ok(exchangeRatesData);
    }




    [HttpPost]
    public async Task<IActionResult> PruebaCorreo([FromBody] NotificacionCrud.EnviarCorreo.CommandCorreoGamil query)
    {
        var res = await _mediator.Send(query); ;
        return Ok(res);
    }



    [HttpPost]
    public async Task<IActionResult> PruebaSecuencial()
    {
        var res = await _iUtilidadesBase.GenerarSecuencial("Cliente", "CTE-EN-25-");
        return Ok(res);
    }




    public class ExchangeRatesData2
    {
        public bool Success { get; set; }
        public long Timestamp { get; set; }
        public string Base { get; set; }
        public string Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }



}
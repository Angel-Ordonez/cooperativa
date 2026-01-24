using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/TasasCambioDivisa/[action]")]
    public class TasasCambioDivisaController : Controller
    {
        private readonly IMediator _mediator;
        public TasasCambioDivisaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarDivisasConCurrencyfreaks([FromBody] TasasCambioDivisaCrud.ActualizarDivisasConCurrencyfreaks.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByDivisas([FromBody] TasasCambioDivisaCrud.GetByDivisas.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetTasasCambioDivisaActivas([FromQuery] TasasCambioDivisaCrud.GetTasasCambioDivisaActivas.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }









    }
}

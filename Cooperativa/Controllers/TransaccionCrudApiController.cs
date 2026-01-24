using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/Transaccion/[action]")]
    public class TransaccionCrudApiController : Controller
    {

        private readonly IMediator _mediator;

        public TransaccionCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] TransaccionCrud.Index.QueryIndexTransaccion query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetTransaccionesByAnio([FromQuery] TransaccionCrud.GetTransaccionesByAnio.QueryIndexTransaccion query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetTransacciones([FromBody] TransaccionCrud.GetTransacciones.QueryTransacciones query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetTransaccionesV2([FromBody] TransaccionCrud.GetTransaccionesV2.QueryTransacciones query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> MantenimientoSaldoAnteriorCaja([FromBody] TransaccionCrud.MantenimientoSaldoAnteriorCaja.QueryIndexTransaccion query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }



    }
}

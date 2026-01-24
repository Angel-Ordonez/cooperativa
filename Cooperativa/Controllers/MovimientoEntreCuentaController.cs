using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/MovimientoEntreCuenta/[action]")]
    public class MovimientoEntreCuentaController : Controller
    {

        private readonly IMediator _mediator;
        public MovimientoEntreCuentaController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        public async Task<IActionResult> GetCajasByEmpresaId([FromQuery] MovimientoEntreCuentaCrud.GetByCaja.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetCajasConTransaccionesByEmpresaId([FromQuery] MovimientoEntreCuentaCrud.GetByCuentaBancaria.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] MovimientoEntreCuentaCrud.GetMovimientoEntreCuentaByFiltros.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }










    }
}

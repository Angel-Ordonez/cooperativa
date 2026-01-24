using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/CuentaBancaria/[action]")]
    public class CuentaBancariaController : Controller
    {
        private readonly IMediator _mediator;
        public CuentaBancariaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CuentaBancariaCrud.Create.CommandCUC command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearParaCaja([FromBody] CuentaBancariaCrud.CrearParaCaja.CommandCUC command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCuentaEfectivoPersonasSinCuenta([FromBody] CuentaBancariaCrud.CrearCuentaEfectivoPersonasSinCuenta.CommandCrearCuentaEfectivoPersonasSinCuenta command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCuentaEfectivoCajasSinCuenta([FromBody] CuentaBancariaCrud.CrearCuentaEfectivoCajasSinCuenta.CommandCrearCuentaEfectivoCajasSinCuenta command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] CuentaBancariaCrud.Update.CommandCUU command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] CuentaBancariaCrud.Eliminar.CommandCBE command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Habilitar([FromBody] CuentaBancariaCrud.Habilitar.CommandCBH command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] CuentaBancariaCrud.Index.QueryCI query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetByPersonaId([FromQuery] CuentaBancariaCrud.GetByPersonaId.QueryCU1 query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetByInstitucionBancariaId([FromQuery] CuentaBancariaCrud.GetByInstitucionBancariaId.QueryCU2 query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByPersonasds([FromBody] CuentaBancariaCrud.GetByPersonasds.CommandCB1 command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByInstitucionesBancariaIds([FromBody] CuentaBancariaCrud.GetByInstitucionesBancariaIds.CommandCB2 command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }







    }
}

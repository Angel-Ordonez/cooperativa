using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/Caja/[action]")]
    public class CajaCrudApiController : Controller
    {


        private readonly IMediator _mediator;
        public CajaCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CajaCrud.Crear.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetCajasByEmpresaId([FromQuery] CajaCrud.GetCajasByEmpresaId.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetCajasConTransaccionesByEmpresaId([FromQuery] CajaCrud.GetCajasConTransaccionesByEmpresaId.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetCajaModuloPrestamoByEmpresaId([FromQuery] CajaCrud.GetCajaModuloPrestamoByEmpresaId.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSaldoActivo([FromQuery] CajaCrud.GetSaldoActivo.QueryGetSaldoActivo query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] CajaCrud.Index.QueryGetCajas query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetInformacionInicio([FromQuery] CajaCrud.GetInformacionInicio.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> MoverDineroDeUnaCuentaAOtraEnCaja([FromBody] CajaCrud.MoverDineroDeUnaCuentaAOtraEnCaja.CommandAR command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }





    }
}

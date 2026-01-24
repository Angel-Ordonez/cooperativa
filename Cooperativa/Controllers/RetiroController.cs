using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/Retiro/[action]")]
    public class RetiroController : Controller
    {

        private readonly IMediator _mediator;
        public RetiroController(IMediator mediator)
        {
            _mediator = mediator;
        }



        [HttpPost]
        public async Task<IActionResult> CrearParaCaja([FromBody] RetiroCrud.CrearParaCaja.CommandRec command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearParaSocioInversion([FromBody] RetiroCrud.CrearParaSocioInversion.CommandRsi command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> AtenderRetiro([FromBody] RetiroCrud.AtenderRetiro.CommandAR command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByFiltros([FromBody] RetiroCrud.GetByFiltros.Query command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPendientesAtender([FromQuery] RetiroCrud.GetPendientesAtender.CommandGPA command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetRetiros([FromQuery] RetiroCrud.GetRetiros.CommandGR command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetRetirosBySocioId([FromQuery] RetiroCrud.GetRetirosBySocioId.CommandGRBS command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetReporteRapido([FromQuery] RetiroCrud.GetReporteRapido.CommandRR command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposRetiros([FromQuery] RetiroCrud.GetTiposRetiros.QueryGTR command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetEstadosRetiro([FromQuery] RetiroCrud.GetEstadosRetiro.QueryGTR command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }



    }
}

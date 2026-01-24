using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/Nota/[action]")]
    public class NotaController : Controller
    {
        private readonly IMediator _mediator;
        public NotaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] NotaCrud.Crear.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearAvanzado([FromBody] NotaCrud.CrearAvanzado.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> AtenderNotaPendiente([FromBody] NotaCrud.AtenderNotaPendiente.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] NotaCrud.Eliminar.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByPersonasId([FromBody] NotaCrud.GetByPersonasId.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetNotasByFiltros([FromBody] NotaCrud.GetNotasByFiltros.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByPrestamosId([FromBody] NotaCrud.GetByPrestamosId.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposNota([FromQuery] NotaCrud.GetTiposNota.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetRazonRapidaNota([FromQuery] NotaCrud.GetRazonRapidaNota.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }



    }
}

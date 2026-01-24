using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/RetornoCliente/[action]")]
    public class RetornoClienteController : Controller
    {
        private readonly IMediator _mediator;
        public RetornoClienteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RetornoClienteCrud.Crear.Command command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> Atender([FromBody] RetornoClienteCrud.Atender.Command command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] RetornoClienteCrud.Eliminar.Command command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> GetByFiltros([FromBody] RetornoClienteCrud.GetByFiltros.QueryByFiltros command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }
        [HttpPost]
        public async Task<IActionResult> GetByClientes([FromBody] RetornoClienteCrud.GetByClientes.Command command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }
        [HttpGet]
        public async Task<IActionResult> GetPendientes([FromQuery] RetornoClienteCrud.GetPendientes.Command command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }








    }
}

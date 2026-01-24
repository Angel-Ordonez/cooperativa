using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.ClienteCrud;

namespace Cooperativa.Controllers
{

    [Route("api/Cliente/[action]")]
    public class ClienteCrudApiController : Controller
    {

        private readonly IMediator _mediator;
        public ClienteCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClienteCrud.Create.Command command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ClienteCrud.Update.CommandUpdateUser command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Habilitar([FromBody] ClienteCrud.HabilitarCliente.Commandhc command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] ClienteCrud.EliminarCliente.Commandec command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpGet]
        public async Task<ActionResult<List<ClienteCrud.ClienteVm>>> Index()
        {
            return Ok(await _mediator.Send(new ClienteCrud.Index.Query()));
        }

        [HttpGet]
        public async Task<ActionResult<List<IndexClientesYPrestamosActivos.InfoPrestamoCliente>>> IndexClientesYPrestamosActivos()
        {
            return Ok(await _mediator.Send(new ClienteCrud.IndexClientesYPrestamosActivos.Query()));
        }

        [HttpGet]
        public async Task<ActionResult<List<ClienteCrud.ClienteVm>>> IndexHabilitados()
        {
            return Ok(await _mediator.Send(new ClienteCrud.IndexHabilitados.Query()));
        }


        [HttpGet]
        public async Task<IActionResult> GetClienteById([FromQuery] ClienteCrud.GetClienteById.Query query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }


    }
}

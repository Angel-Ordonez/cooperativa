using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/InstitucionBancaria/[action]")]
    public class InstitucionBancariaController : Controller
    {
        private readonly IMediator _mediator;
        public InstitucionBancariaController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InstitucionBancariaCrud.Create.CommandIBC command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearGenericas([FromBody] InstitucionBancariaCrud.CrearGenericas.CommandIBCG command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] InstitucionBancariaCrud.Update.CommandIBU command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] InstitucionBancariaCrud.Eliminar.CommandIBE command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Habilitar([FromBody] InstitucionBancariaCrud.Habilitar.CommandIBH command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] InstitucionBancariaCrud.Index.QueryI query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetByPais([FromQuery] InstitucionBancariaCrud.GetByPais.QueryGP query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposInstitucionBancaria([FromQuery] InstitucionBancariaCrud.GetTiposInstitucionBancaria.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }


    }
}

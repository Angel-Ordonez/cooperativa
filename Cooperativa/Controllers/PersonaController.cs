using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/Persona/[action]")]
    public class PersonaController : Controller
    {
        private readonly IMediator _mediator;
        public PersonaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> RecalcularEdades([FromBody] PersonaCrud.RecalcularEdades.CommandRecalcularEdades command )
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetCumpleanos([FromQuery] PersonaCrud.GetCumpleanos.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }










    }
}

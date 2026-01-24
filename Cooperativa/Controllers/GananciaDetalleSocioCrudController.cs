using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [ApiController]
    [Route("api/GananciaDetalleSocio/[action]")]
    public class GananciaDetalleSocioCrudController : Controller
    {
        private readonly IMediator _mediator;
        public GananciaDetalleSocioCrudController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CrearByPrestamoId([FromBody] GananciaDetalleSocioCrud.CrearByPrestamoId.CommandCGA command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }





    }
}

using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/ConfiguracionPrestamo/[action]")]
    public class ConfiguracionPrestamoCrudController : Controller
    {
        private readonly IMediator _mediator;
        public ConfiguracionPrestamoCrudController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConfiguracionPrestamoCrud.Create.CommandConfiguracionPrestamo command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ConfiguracionPrestamoCrud.Update.CommandConfiguracionPrestamoU command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ConfiguracionPrestamoCrud.IndexConfiguracionPrestamo.QueryIndexConfiguracionPrestamo query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }




    }
}

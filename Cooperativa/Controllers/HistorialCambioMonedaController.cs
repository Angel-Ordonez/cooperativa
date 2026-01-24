using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/HistorialCambioMoneda/[action]")]
    public class HistorialCambioMonedaController : Controller
    {
        private readonly IMediator _mediator;
        public HistorialCambioMonedaController(IMediator mediator)
        {
            _mediator = mediator;
        }





        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HistorialCambioMonedaCrud.Create.CommandHistorialMonedaCreate command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] HistorialCambioMonedaCrud.Delete.CommandHistorialMonedaDelete command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }


        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] HistorialCambioMonedaCrud.Index.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetByFecha([FromQuery] HistorialCambioMonedaCrud.GetByFecha.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetByRangoFechas([FromQuery] HistorialCambioMonedaCrud.GetByRangoFechas.Query query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }


        [HttpGet]
        public async Task<IActionResult> IndexActualApi([FromQuery] HistorialCambioMonedaCrud.IndexActualApi.CommandHistorialMonedaIndexApi command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }












    }
}

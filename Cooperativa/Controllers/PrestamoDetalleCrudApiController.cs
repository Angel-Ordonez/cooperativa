using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [ApiController]
    [Route("api/PrestamoDetalle/[action]")]
    public class PrestamoDetalleCrudApiController : Controller
    {

        private readonly IMediator _mediator;
        public PrestamoDetalleCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrestamoDetalleCrud.Create.CommandPrestamoDetalleCreate command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] PrestamoDetalleCrud.Delete.CommandDetallePrestamoDelete command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> GetInteresAndFechaByCantidad([FromBody] PrestamoDetalleCrud.GetInteresAndFechaByCantidad.CommandPrestamoDetalleIn command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetInteresAndFechaByCantidadAndFechaCotizacion([FromBody] PrestamoDetalleCrud.GetInteresAndFechaByCantidadAndFechaCotizacion.CommandGetInteresAndFechaByCantidadAndFechaCotizacion command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrestamoDetalleByClienteId([FromQuery] PrestamoDetalleCrud.GetPrestamoDetalleByClienteId.QueryPrestamoId command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }






    }
}

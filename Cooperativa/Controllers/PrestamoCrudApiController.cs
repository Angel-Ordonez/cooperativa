using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [ApiController]
    [Route("api/Prestamo/[action]")]
    public class PrestamoCrudApiController : Controller
    {
        private readonly IMediator _mediator;
        public PrestamoCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PrestamoCrud.Create.CommandPrestamoCreate command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CrearPrestamoPIM([FromBody] PrestamoCrud.CrearPrestamoPIM.CommandPrestamoCreate command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> AtenderPrestamoPendiente([FromForm] PrestamoCrud.AtenderPrestamoPendiente.CommandPrestamo command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] PrestamoCrud.Delete.CommandPrestamoDelete command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> RecalcularCantidadCuotas([FromBody] PrestamoCrud.RecalcularCantidadCuotas.CommandPrestamoRe command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PrestamoCrud.Index.QueryPrestamosIndex query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> IndexSoloPrestamos([FromQuery] PrestamoCrud.IndexSoloPrestamos.QueryPrestamosIndex query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> DashBoardPIM([FromQuery] PrestamoCrud.DashBoardPIM.QueryPrestamosIndex query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByFiltros([FromBody] PrestamoCrud.GetByFiltros.QueryPrestamosByFiltros command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetByFiltrosSinDetalles([FromBody] PrestamoCrud.GetByFiltrosSinDetalles.QueryPrestamosByFiltros command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }



        [HttpGet]
        public async Task<IActionResult> GetPrestamoByClienteId([FromQuery] PrestamoCrud.GetPrestamoByClienteId.QueryPrestamoByClienteId query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrestamoById([FromQuery] PrestamoCrud.GetPrestamoById.QueryPrestamoById query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetPrestamoDetalleByClienteId([FromQuery] PrestamoDetalleCrud.GetPrestamoDetalleByClienteId.QueryPrestamoId command)
        //{
        //    var res = await _mediator.Send(command); ;
        //    return Ok(res);
        //}

        [HttpGet]
        public async Task<IActionResult> GetPrestamosActivos([FromQuery] PrestamoCrud.GetPrestamosActivos.QueryPrestamosActivos query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrestamosPagados([FromQuery] PrestamoCrud.GetPrestamosPagados.QueryPrestamosPagados query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetPrestamosCancelados([FromQuery] PrestamoCrud.GetPrestamosCancelados.QueryPrestamosCancelados query)
        {
            var res = await _mediator.Send(query);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetEstadosPrestamo([FromQuery] PrestamoCrud.GetEstadosPrestamo.QueryGTR command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetEstadosParaAtenderPrestamo([FromQuery] PrestamoCrud.GetEstadosParaAtenderPrestamo.QueryGTR command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetCuentasBancariasParaPrestamosPendientesAprobacion([FromQuery] PrestamoCrud.GetCuentasBancariasParaPrestamosPendientesAprobacion.Command command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }







    }
}

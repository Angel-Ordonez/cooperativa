using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [ApiController]
    [Route("api/SocioInversion/[action]")]
    public class SocioInvercionCrudApiController : Controller
    {
        private readonly IMediator _mediator;
        public SocioInvercionCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SocioInversionCrud.Crear.CommandCreateSocioInversion command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] SocioInversionCrud.Delete.CommandDeleteSocioInversion command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CalcularGananciaSocioInversiones([FromBody] SocioInversionCrud.CalcularGananciaSocioInversiones.CommandCGSI command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }


        [HttpGet]
        public async Task<ActionResult<List<SocioInversionCrud.SocioInversionVm>>> Index([FromQuery] SocioInversionCrud.IndexSocioInversion.CommandIndexSocioInversion query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }


        [HttpGet]
        public async Task<IActionResult> GetSocioInversionBySocioId([FromQuery] SocioInversionCrud.GetSocioInversionBySocioId.CommandIndexSocioInversionBySocio query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> GetSocioInversionBySocioIdAndAnio([FromBody] SocioInversionCrud.GetSocioInversionBySocioIdAndAnio.CommandIndexSocioInversionBySocioAndAnio query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }




        [HttpPost]
        public async Task<IActionResult> LlenarNuevosCampos([FromBody] SocioInversionCrud.LlenarNuevosCampos.CommandLlenarNuevosCampos query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }




    }
}

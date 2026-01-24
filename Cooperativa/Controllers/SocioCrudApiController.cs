using Cooperativa.App.CRUD;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [ApiController]
    [Route("api/Socio/[action]")]
    public class SocioCrudApiController : Controller
    {
        private readonly IMediator _mediator;
        public SocioCrudApiController(IMediator mediator)
        {
            _mediator = mediator;
        }



        [HttpPost]
        public async Task<IActionResult> CreateConInversion([FromBody] SocioCrud.CreateSocio.CommandCreateSocio command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SocioCrud.Crear.CommandCreateSocio command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SocioCrud.UpdateSocio.CommandUpdateSocio command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Habilitar([FromBody] SocioCrud.HabilitarSocio.Commandhs command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar([FromBody] SocioCrud.EliminarSocio.Commandes command)
        {
            var res = await _mediator.Send(command);
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> Index([FromBody] SocioCrud.IndexSocio.CommandIndexSocio command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<ActionResult<List<SocioCrud.SocioVm>>> Index()
        {
            return Ok(await _mediator.Send(new SocioCrud.IndexSocio.CommandIndexSocio()));
        }

        [HttpGet]
        public async Task<IActionResult> IndexV2([FromQuery] SocioCrud.IndexV2.CommandIndexSocio command)
        {
            var res = await _mediator.Send(command); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSocioById([FromQuery] SocioCrud.GetSocioById.QueryGetSocio query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetSocioAndCuentasBancarias([FromQuery] SocioCrud.GetSocioAndCuentasBancarias.CommandGSCB query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }



    }



}

using Cooperativa.App.CRUD;
using Cooperativa.App.Engine;
using Cooperativa.App.Utilidades;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/Notificacion/[action]")]
    public class NotificacionController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IExchangeratesService _exchangeratesService;
        private readonly IUtilidadesBase _iUtilidadesBase;

        public NotificacionController(IMediator mediator, IExchangeratesService exchangeratesService, IUtilidadesBase utilidadesBase)
        {
            _mediator = mediator;
            _exchangeratesService = exchangeratesService;
            _iUtilidadesBase = utilidadesBase;
        }



        [HttpPost]
        public async Task<IActionResult> PruebaCorreo([FromBody] NotificacionCrud.EnviarCorreo.CommandCorreoGamil query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarCorreo([FromBody] NotificacionCrud.EnviarCorreo.CommandCorreoGamil query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> EnviarCorreoBasico([FromBody] NotificacionCrud.EnviarCorreoBasico.CommandCorreoGamil query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }


        [HttpPost]
        public async Task<IActionResult> EnviarCorreoPagoPIM([FromBody] NotificacionCrud.EnviarCorreoPagoPIM.CommandCorreoGamil query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }











    }
}

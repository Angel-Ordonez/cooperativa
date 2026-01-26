using Cooperativa.App.CRUD;
using Cooperativa.App.Engine;
using Cooperativa.App.Utilidades;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cooperativa.Controllers
{
    [Route("api/ReporteDashboard/[action]")]
    public class ReporteDashboardController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IExchangeratesService _exchangeratesService;
        private readonly IUtilidadesBase _iUtilidadesBase;

        public ReporteDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }



        [HttpGet]
        public async Task<IActionResult> ReporteDashboardGananciasRetirosGlobal([FromQuery] ReporteDashboardCrud.ReporteDashboardGananciasRetirosGlobal.QueryPrestamosIndex query)
        {
            var res = await _mediator.Send(query); ;
            return Ok(res);
        }









    }
}

using Cooperativa.App.CRUD;
using Cooperativa.App.Soluciones.Pdf;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Cooperativa.Controllers.Soluciones
{
    [Route("api/pdf/[action]")]
    public class PdfController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IPdfPimService _pdfPimService;
        public PdfController(IMediator mediator, IPdfPimService pdfPimService)
        {
            _mediator = mediator;
            _pdfPimService = pdfPimService; 
        }


        [HttpGet]
        public async Task<IActionResult> GenerarContratoByPrestamoId([FromQuery] Guid id)
        {
            var appResult = await _pdfPimService.GenerarContratoByPrestamoId(id);

            // Exponer el encabezado: Asi logro captura el nombre del pdf desde el frontend
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }


        //[HttpGet]
        //public async Task<IActionResult> GenerarEstadoPrestamoById([FromQuery] Guid id)
        //{
        //    var appResult = await _pdfPimService.GenerarEstadoPrestamoById(id);
        //    return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        //}

        [HttpGet]
        public async Task<IActionResult> GenerarEstadoPrestamoById([FromQuery] Guid id)
        {
            var appResult = await _pdfPimService.GenerarEstadoPrestamoById(id);

            // Exponer el encabezado: Asi logro captura el nombre del pdf desde el frontend
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarEstadoPrestamoByIdVertical([FromQuery] Guid id)
        {
            var appResult = await _pdfPimService.GenerarEstadoPrestamoByIdVertical(id);

            // Exponer el encabezado: Asi logro captura el nombre del pdf desde el frontend
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }



        [HttpGet]
        public async Task<IActionResult> GenerarContrato()
        {
            var pdfBytes = await _mediator.Send(new PdfContrato.CrearPdfContrato.Query());

            return File(pdfBytes, "application/pdf", "ContratoDemo.pdf");
        }



        [HttpGet]
        public async Task<IActionResult> GenerarReporteActivosPIM([FromQuery] Guid? id)
        {
            var appResult = await _pdfPimService.GenerarReporteActivosPIM(id, id);

            // Exponer el encabezado: Asi logro captura el nombre del pdf desde el frontend
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }








        [HttpGet]
        public async Task<IActionResult> GenerarPdfEjemplo()
        {
            var appResult = await _pdfPimService.GenerarPdfEjemplo();
            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }





        //=============================
        //Pdf desde no services
        //=============================
        [HttpPost]
        public async Task<IActionResult> GenerarReporteActivosPIM([FromBody] PdfServices.ReporteListaPrestamosPimByFiltros.Command command)
        {
            var appResult = await _mediator.Send(command); ;
            // Exponer el encabezado: Asi logro captura el nombre del pdf desde el frontend
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }


        [HttpPost]
        public async Task<IActionResult> ReportePimClienteByFiltros([FromBody] PdfServices.ReportePimClienteByFiltros.Command command)
        {
            var appResult = await _mediator.Send(command); ;
            // Exponer el encabezado: Asi logro captura el nombre del pdf desde el frontend
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            return File(appResult.Archivo, "application/pdf", $"{appResult.NombreArchivo}.pdf");
        }





    }
}

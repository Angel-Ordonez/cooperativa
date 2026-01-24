using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Soluciones.Pdf;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.PrestamoCrud;

namespace Cooperativa.App.CRUD
{
    public class PdfServices
    {

        public class ReporteListaPrestamosPimByFiltros
        {
            public class Command : IRequest<AppResult>
            {
                public Guid? EmpresaId { get; set; }
                public Guid? ResponsableId { get; set; }
                public EstadoPrestamo? Estado { get; set; }
                public bool TodosLosEstados { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IPdfPimService _pdfPimService;
                public CommandHandler(CooperativaDbContext context, IPdfPimService pdfPimService)
                {
                    _context = context;
                    _pdfPimService = pdfPimService;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {

                    return await _pdfPimService.GenerarReportePIM(cmd.EmpresaId, cmd.ResponsableId, cmd.Estado, cmd.TodosLosEstados);
                }
            }
        }



        public class ReportePimClienteByFiltros
        {
            public class Command : IRequest<AppResult>
            {
                public Guid ClienteId { get; set; }
                public EstadoPrestamo? Estado { get; set; }
                public bool TodosLosEstados { get; set; }
                public bool MostrarDetalles { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IPdfPimService _pdfPimService;
                public CommandHandler(CooperativaDbContext context, IPdfPimService pdfPimService)
                {
                    _context = context;
                    _pdfPimService = pdfPimService;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {

                    return await _pdfPimService.GenerarReportePIMCliente(cmd.ClienteId, cmd.Estado, cmd.TodosLosEstados, cmd.MostrarDetalles);
                }
            }
        }
















    }
}

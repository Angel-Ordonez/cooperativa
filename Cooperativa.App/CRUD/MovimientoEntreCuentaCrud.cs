using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Entidad;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.Domain.Model.Caja.MovimientoEntreCuenta;
using static Cooperativa.App.Domain.Model.Entidad.Nota;

namespace Cooperativa.App.CRUD
{
    public class MovimientoEntreCuentaCrud
    {

        public class GetByCaja
        {
            public class Command : IRequest<List<MovimientoEntreCuentaVm>>
            {
                public Guid CajaId { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<MovimientoEntreCuentaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<MovimientoEntreCuentaVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var movimientoEntreCuentaRes = await _context.MovimientoEntreCuenta.Where(x => x.CajaId == cmd.CajaId && !x.IsSoftDeleted)
                        .ProjectToType<MovimientoEntreCuentaVm>()
                        .ToListAsync();

                    return movimientoEntreCuentaRes;
                }
            }
        }



        public class GetByCuentaBancaria
        {
            public class Command : IRequest<List<MovimientoEntreCuentaVm>>
            {
                public Guid CuentaBancariaId { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<MovimientoEntreCuentaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<MovimientoEntreCuentaVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var movimientoEntreCuentaRes = await _context.MovimientoEntreCuenta.Where(x => (x.CuentaBancariaOrigenId == cmd.CuentaBancariaId || x.CuentaBancariaDestinoId == cmd.CuentaBancariaId) && !x.IsSoftDeleted)
                        .ProjectToType<MovimientoEntreCuentaVm>()
                        .ToListAsync();

                    return movimientoEntreCuentaRes;
                }
            }
        }




        public class GetMovimientoEntreCuentaByFiltros
        {
            public class RangoFechasVm
            {
                public DateTime FechaInicio { get; set; }
                public DateTime FechaFin { get; set; }
            }
            public class AnioMesVm
            {
                public int Anio { get; set; }
                public int Mes { get; set; }
            }
            public class NotaRes : NotaVm
            {
                public string Razon { get; set; }
                public string Referencia { get; set; }
            }

            public class Command : IRequest<List<MovimientoEntreCuentaVm>>
            {
                public Guid CajaId { get; set; }
                public Guid? CuentaBancariaId { get; set; }
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<MovimientoEntreCuentaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<MovimientoEntreCuentaVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {

                    var movimientoEntreCuentasQuery = _context.MovimientoEntreCuenta.Where(x => !x.IsSoftDeleted && x.CajaId == cmd.CajaId);

                    if (cmd.CuentaBancariaId != null && cmd.CuentaBancariaId != Guid.Empty)
                    {
                        movimientoEntreCuentasQuery = movimientoEntreCuentasQuery.Where(x => x.CuentaBancariaOrigenId == cmd.CuentaBancariaId || x.CuentaBancariaDestinoId == cmd.CuentaBancariaId);
                    }

                    if (cmd.Index)
                    {

                    }
                    else if (cmd.Anio != 0)
                    {
                        movimientoEntreCuentasQuery = movimientoEntreCuentasQuery.Where(x => x.CreatedDate.Year == cmd.Anio);
                    }
                    else if (cmd.AnioMes != null)
                    {
                        movimientoEntreCuentasQuery = movimientoEntreCuentasQuery.Where(x => x.CreatedDate.Date.Year == cmd.AnioMes.Anio && x.CreatedDate.Date.Month == cmd.AnioMes.Mes);
                    }
                    else if (cmd.RangoFechas != null)
                    {
                        var inicio = cmd.RangoFechas.FechaInicio.Date;
                        var fin = cmd.RangoFechas.FechaFin.Date;

                        movimientoEntreCuentasQuery = movimientoEntreCuentasQuery.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin);
                    }


                    var movimientoEntreCuentaRes = await movimientoEntreCuentasQuery.ProjectToType<MovimientoEntreCuentaVm>().ToListAsync();

                    return movimientoEntreCuentaRes;
                }
            }
        }

























    }
}

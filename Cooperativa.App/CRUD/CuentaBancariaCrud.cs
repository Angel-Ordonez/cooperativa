using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Mapster;
using static Cooperativa.App.Domain.Model.People.InstitucionBancaria;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;
using Cooperativa.App.Utilidades;
using System.Security.Cryptography.Xml;
using static Cooperativa.App.CRUD.NotaCrud;

namespace Cooperativa.App.CRUD
{
    public class CuentaBancariaCrud
    {

        public class Create
        {
            public class CommandCUC : IRequest<AppResult>
            {
                public string NumeroCuenta { get; set; }
                public string NumeroTarjeta { get; set; }           //Por ejmplo la tarjeta de debito
                public TipoCuentaBancaria TipoCuenta { get; set; }
                public Guid PersonaId { get; set; }
                public Guid? InstitucionBancariaId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandCUC, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCUC cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var persona = await _context.Persona.Where(x => x.Id == cmd.PersonaId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if (persona == null) { return AppResult.New(false, "Persona no existe"); }

                        var ib = await _context.InstitucionBancaria.Where(x => x.Id == cmd.InstitucionBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (ib == null) { return AppResult.New(false, "Institucion Bancaria no existe"); }

                        var existe = await _context.CuentaBancaria.Where(x => !x.IsSoftDeleted
                             //&& (x.NumeroCuenta == cmd.NumeroCuenta || x.NumeroTarjeta == cmd.NumeroCuenta || x.NumeroCuenta == cmd.NumeroTarjeta || x.NumeroTarjeta == cmd.NumeroTarjeta))
                             && (x.NumeroCuenta == cmd.NumeroCuenta || x.NumeroCuenta == cmd.NumeroTarjeta ) && x.PersonaId == persona.Id)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(cancellationToken);

                        if(existe != null)
                        {
                            if(existe.NumeroCuenta == cmd.NumeroCuenta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Cuenta!");
                            }
                            if (existe.NumeroCuenta == cmd.NumeroTarjeta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Tarjeta, esta como Numero de Cuenta!");
                            }
                            if (existe.NumeroTarjeta == cmd.NumeroTarjeta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Tarjeta!");
                            }
                            if (existe.NumeroTarjeta == cmd.NumeroCuenta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Cuenta, esta como Numero de Tarjeta!");
                            }
                        }


                        var newCuentaBancaria = CuentaBancaria.New(persona.Id, ib.Id, cmd.NumeroCuenta, cmd.NumeroTarjeta, cmd.TipoCuenta, createdBy);

                        await _context.CuentaBancaria.AddAsync(newCuentaBancaria);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Cuenta Bancaria creada exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }



        public class CrearParaCaja
        {
            public class CommandCUC : IRequest<AppResult>
            {
                public Guid CajaId { get; set; }
                public string NumeroCuenta { get; set; }
                public string NumeroTarjeta { get; set; }           //Por ejmplo la tarjeta de debito
                public TipoCuentaBancaria TipoCuenta { get; set; }
                public Guid? InstitucionBancariaId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandCUC, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCUC cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var caja = await _context.Caja.Where(x => x.Id == cmd.CajaId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        caja.ThrowIfNull("Caja no existe");

                        var ib = await _context.InstitucionBancaria.Where(x => x.Id == cmd.InstitucionBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (ib == null) { return AppResult.New(false, "Institucion Bancaria no existe"); }

                        var existe = await _context.CuentaBancaria.Where(x => !x.IsSoftDeleted
                             //&& (x.NumeroCuenta == cmd.NumeroCuenta || x.NumeroTarjeta == cmd.NumeroCuenta || x.NumeroCuenta == cmd.NumeroTarjeta || x.NumeroTarjeta == cmd.NumeroTarjeta))
                             && (x.NumeroCuenta == cmd.NumeroCuenta || x.NumeroCuenta == cmd.NumeroTarjeta) && x.CajaId == caja.Id)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(cancellationToken);

                        if (existe != null)
                        {
                            if (existe.NumeroCuenta == cmd.NumeroCuenta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Cuenta!");
                            }
                            if (existe.NumeroCuenta == cmd.NumeroTarjeta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Tarjeta, esta como Numero de Cuenta!");
                            }
                            if (existe.NumeroTarjeta == cmd.NumeroTarjeta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Tarjeta!");
                            }
                            if (existe.NumeroTarjeta == cmd.NumeroCuenta)
                            {
                                throw new Exception($"Ya existe Cuenta bancaria con este Numero de Cuenta, esta como Numero de Tarjeta!");
                            }
                        }


                        var newCuentaBancaria = CuentaBancaria.NewParaCaja(caja.Id, ib.Id, cmd.NumeroCuenta, cmd.NumeroTarjeta, cmd.TipoCuenta, createdBy);

                        await _context.CuentaBancaria.AddAsync(newCuentaBancaria);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Cuenta Bancaria creada exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }




        //MetaData
        public class CrearCuentaEfectivoPersonasSinCuenta
        {
            public class CommandCrearCuentaEfectivoPersonasSinCuenta : IRequest<AppResult>
            {

            }

            public class CommandHandler : IRequestHandler<CommandCrearCuentaEfectivoPersonasSinCuenta, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCrearCuentaEfectivoPersonasSinCuenta cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var personas = await _context.Persona.Where(x => !x.CuentaBancarias.Any()).ToListAsync();

                        List<CuentaBancaria> nuevasCuentasBancarias = new List<CuentaBancaria>();

                        foreach(var persona in personas)
                        {
                            var newCuentaBancaria = CuentaBancaria.New(persona.Id, null, "Efectivo", "Efectivo", TipoCuentaBancaria.Otro, createdBy);
                            nuevasCuentasBancarias.Add(newCuentaBancaria);
                        }

                        await _context.CuentaBancaria.AddRangeAsync(nuevasCuentasBancarias);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, $"{nuevasCuentasBancarias.Count()} Cuentas Bancarias creadas exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }




        //MetaData
        public class CrearCuentaEfectivoCajasSinCuenta
        {
            public class CommandCrearCuentaEfectivoCajasSinCuenta : IRequest<AppResult>
            {

            }

            public class CommandHandler : IRequestHandler<CommandCrearCuentaEfectivoCajasSinCuenta, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCrearCuentaEfectivoCajasSinCuenta cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        //var cajas = await _context.Caja.Where(x => x.Enabled && !x.IsSoftDeleted && !x.CuentaBancarias.Any()).ToListAsync();

                        var cajas = await _context.Caja.Where(x => x.Enabled && !x.IsSoftDeleted && !x.CuentaBancarias.Any(s => s.NumeroCuenta.ToLower() == "efectivo")).ToListAsync();

                        List<CuentaBancaria> nuevasCuentasBancarias = new List<CuentaBancaria>();

                        foreach (var caja in cajas)
                        {
                            var newCuentaBancaria = CuentaBancaria.NewParaCaja(caja.Id, null, "Efectivo", "Efectivo", TipoCuentaBancaria.Otro, createdBy);
                            nuevasCuentasBancarias.Add(newCuentaBancaria);
                        }

                        await _context.CuentaBancaria.AddRangeAsync(nuevasCuentasBancarias);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, $"{nuevasCuentasBancarias.Count()} Cuentas Bancarias creadas exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }










        public class Update
        {
            public class CommandCUU : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string NumeroCuenta { get; set; }
                public string NumeroTarjeta { get; set; }           //Por ejmplo la tarjeta de debito
                public TipoCuentaBancaria TipoCuenta { get; set; }
                public Guid? PersonaId { get; set; }
                public Guid? InstitucionBancariaId { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandCUU, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCUU cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var usuarioId = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var cb = await _context.CuentaBancaria.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if (cb == null) { return AppResult.New(false, "Cuenta Bancaria no existe"); }

                        var existe = await _context.CuentaBancaria.Where(x => !x.IsSoftDeleted
                            && (x.NumeroCuenta == cmd.NumeroCuenta || x.NumeroTarjeta == cmd.NumeroCuenta || x.NumeroCuenta == cmd.NumeroTarjeta || x.NumeroTarjeta == cmd.NumeroTarjeta))
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

                        var queSeActualizo = "";

                        if (cmd.NumeroCuenta != null && cmd.NumeroCuenta.Length > 1 && cmd.NumeroCuenta != cb.NumeroCuenta)
                        {
                            if(existe != null && existe.Id != cb.Id)
                            {
                                if (existe.NumeroCuenta == cmd.NumeroCuenta)
                                {
                                    throw new Exception($"Ya existe Cuenta bancaria con este Numero de Cuenta!");
                                }
                                if (existe.NumeroTarjeta == cmd.NumeroCuenta)
                                {
                                    throw new Exception($"Ya existe Cuenta bancaria con este Numero de Cuenta, esta como Numero de Tarjeta!");
                                }
                            }

                            cb.NumeroCuenta = cmd.NumeroCuenta;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", NumeroCuenta"; } else { queSeActualizo += "NumeroCuenta"; }
                        }
                        if (cmd.NumeroTarjeta != null && cmd.NumeroTarjeta.Length > 1 && cmd.NumeroTarjeta != cb.NumeroTarjeta)
                        {
                            if (existe != null && existe.Id != cb.Id)
                            {
                                if (existe.NumeroCuenta == cmd.NumeroTarjeta)
                                {
                                    throw new Exception($"Ya existe Cuenta bancaria con este Numero de Tarjeta, esta como Numero de Cuenta!");
                                }
                                if (existe.NumeroTarjeta == cmd.NumeroTarjeta)
                                {
                                    throw new Exception($"Ya existe Cuenta bancaria con este Numero de Tarjeta!");
                                }
                            }

                            cb.NumeroTarjeta = cmd.NumeroTarjeta;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", NumeroTarjeta"; } else { queSeActualizo += "NumeroTarjeta"; }
                        }
                        if (cmd.PersonaId != null && cmd.PersonaId != Guid.Empty && cmd.PersonaId != cb.PersonaId)
                        {
                            var persona = await _context.Persona.Where(x => x.Id == cmd.PersonaId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            if (persona == null) { throw new Exception("Persona ha actualizar no existe"); }

                           cb.PersonaId = persona.Id;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Dueño"; } else { queSeActualizo += "Dueño"; }
                        }
                        if (cmd.InstitucionBancariaId != null && cmd.InstitucionBancariaId != Guid.Empty && cmd.InstitucionBancariaId != cb.InstitucionBancariaId)
                        {
                            var ib = await _context.InstitucionBancaria.Where(x => x.Id == cmd.InstitucionBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                            if (ib == null) { throw new Exception("Institucion Bancaria no existe"); }

                            cb.InstitucionBancariaId = ib.Id;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", InstitucionBancariaId"; } else { queSeActualizo += "InstitucionBancariaId"; }
                        }
                        if (cmd.TipoCuenta != cb.TipoCuenta)
                        {
                            cb.TipoCuenta = cmd.TipoCuenta;
                            cb.TipoCuenta_Descripcion = TipoInstitucionBancariaDescripcion.GetEstadoTexto((int)cmd.TipoCuenta);

                            if (queSeActualizo.Length > 1) { queSeActualizo += ", TipoCuenta"; } else { queSeActualizo += "TipoCuenta"; }
                        }

                        if (queSeActualizo.Length > 1)
                        {
                            if (cmd.ModifiedBy == null || cmd.ModifiedBy == Guid.Empty)
                            {
                                cmd.ModifiedBy = usuarioId;
                            }

                            cb.ModifiedBy = (Guid)cmd.ModifiedBy;
                            cb.ModifiedDate = DateTime.Now;

                            await _context.SaveChangesAsync();
                            return AppResult.New(true, $"Se actualizo: {queSeActualizo}");
                        }
                        else
                        {
                            return AppResult.New(true, $"Nada que actualizar");
                        }
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }



        public class Eliminar
        {
            public class CommandCBE : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandCBE, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCBE cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var cb = await _context.CuentaBancaria.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if (cb == null) { return AppResult.New(false, "Cuenta Bancaria no existe"); }

                        cb.IsSoftDeleted = true;
                        cb.ModifiedDate = DateTime.Now;
                        if (cmd.ModifiedBy != null && cmd.ModifiedBy != Guid.Empty)
                        {
                            cb.ModifiedBy = (Guid)cmd.ModifiedBy;
                        }

                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Cuenta Bancaria eliminada con exito!");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }



        public class Habilitar
        {
            public class CommandCBH : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public bool Enabled { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandCBH, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCBH cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var cb = await _context.CuentaBancaria.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if (cb == null) { return AppResult.New(false, "Cuenta Bancaria no existe"); }

                        if (cb.Enabled == cmd.Enabled)
                        {
                            if (cmd.Enabled)
                            {
                                { return AppResult.New(false, "Cuenta Bancaria ya esta Habilitada"); }
                            }
                            else
                            {
                                { return AppResult.New(false, "Cuenta Bancaria ya esta Deshabilitida"); }
                            }
                        }

                        cb.Enabled = cmd.Enabled;
                        cb.ModifiedDate = DateTime.Now;
                        if (cmd.ModifiedBy != null && cmd.ModifiedBy != Guid.Empty)
                        {
                            cb.ModifiedBy = (Guid)cmd.ModifiedBy;
                        }

                        await _context.SaveChangesAsync();

                        var msj = cmd.Enabled ? "Habiliada" : "Deshabilitada";

                        return AppResult.New(true, $"Cuenta Bancaria {msj} con exito!");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }


        public class Index
        {
            public class QueryCI : IRequest<List<CuentaBancariaVm>>
            {

            }
            public class QueryHandler : IRequestHandler<QueryCI, List<CuentaBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CuentaBancariaVm>> Handle(QueryCI query, CancellationToken cancellationToken)
                {
                    var index = await _context.CuentaBancaria.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<CuentaBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }



        public class GetByPersonaId
        {
            public class QueryCU1 : IRequest<List<CuentaBancariaVm>>
            {
                public Guid PersonaId { get; set; }
            }
            public class QueryHandler : IRequestHandler<QueryCU1, List<CuentaBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CuentaBancariaVm>> Handle(QueryCU1 query, CancellationToken cancellationToken)
                {
                    var index = await _context.CuentaBancaria.Where(x => x.PersonaId == query.PersonaId && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<CuentaBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }



        public class GetByInstitucionBancariaId
        {
            public class QueryCU2 : IRequest<List<CuentaBancariaVm>>
            {
                public Guid InstitucionBancariaId { get; set; }
            }
            public class QueryHandler : IRequestHandler<QueryCU2, List<CuentaBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CuentaBancariaVm>> Handle(QueryCU2 query, CancellationToken cancellationToken)
                {
                    var index = await _context.CuentaBancaria.Where(x => x.InstitucionBancariaId == query.InstitucionBancariaId && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<CuentaBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }



        public class GetByPersonasds
        {
            public class CommandCB1 : IRequest<List<CuentaBancariaVm>>
            {
                public List<Guid> PersonaIds { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandCB1, List<CuentaBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CuentaBancariaVm>> Handle(CommandCB1 cmd, CancellationToken cancellationToken)
                {
                    var index = await _context.CuentaBancaria.Where(x => cmd.PersonaIds.Contains((Guid)x.PersonaId) && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<CuentaBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }



        public class GetByInstitucionesBancariaIds
        {
            public class CommandCB2 : IRequest<List<CuentaBancariaVm>>
            {
                public List<Guid> InstitucionesBancariaIds { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandCB2, List<CuentaBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CuentaBancariaVm>> Handle(CommandCB2 cmd, CancellationToken cancellationToken)
                {
                    var index = await _context.CuentaBancaria.Where(x => cmd.InstitucionesBancariaIds.Contains((Guid)x.InstitucionBancariaId) && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<CuentaBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }












    }
}

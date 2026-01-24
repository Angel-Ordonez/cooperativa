using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Engine;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using Microsoft.EntityFrameworkCore;
using static Cooperativa.App.Domain.Model.People.InstitucionBancaria;
using Mapster;

namespace Cooperativa.App.CRUD
{
    public class InstitucionBancariaCrud
    {

        public class Create
        {
            public class CommandIBC : IRequest<AppResult>
            {
                public string Nombre { get; set; }
                public string Pais { get; set; }
                public string Direccion { get; set; }
                public string Direccion2 { get; set; }
                public string Telefono { get; set; }
                public string Telefono2 { get; set; }
                public string SitioWeb { get; set; }
                public TipoInstitucionBancaria TipoInstitucion { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandIBC, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandIBC cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        var newInstitucion = InstitucionBancaria.New(cmd.Nombre, cmd.Pais, cmd.Direccion, cmd.Direccion2, cmd.Telefono, cmd.Telefono2, cmd.SitioWeb, cmd.TipoInstitucion, createdBy);

                        await _context.InstitucionBancaria.AddAsync(newInstitucion);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "InstitucionBancaria creada exitosamente");
                    }
                    catch(Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }






        public class CrearGenericas
        {
            public class CommandIBCG : IRequest<AppResult>
            {

            }

            public class CommandHandler : IRequestHandler<CommandIBCG, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandIBCG cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var instucionesBancarias = await _context.InstitucionBancaria.Where(x => !x.IsSoftDeleted).ToListAsync();

                        List<InstitucionBancaria> nuevasInstituciones = new List<InstitucionBancaria>();
                        List<InstitucionBancaria> nuevasInstitucionesPrevias = new List<InstitucionBancaria>();

                        var newInstitucion1 = InstitucionBancaria.New("Banco Atlántida", "Honduras", "Tegucigalpa, Francisco Morazán", "", "+504 2280 1010", "", "https://www.bancatlan.hn", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion2 = InstitucionBancaria.New("Banco de Occidente", "Honduras", "Santa Rosa de Copán", "", "+504 2553 8000", "", "https://www.bancocci.hn", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion3 = InstitucionBancaria.New("Banco Ficohsa", "Honduras", "Tegucigalpa, Francisco Morazán", "", "+504 2280 2000", "", "https://www.ficohsa.com", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion4 = InstitucionBancaria.New("Banco Davivienda Honduras", "Honduras", "Tegucigalpa, Francisco Morazán", "", "+504 2275 2200", "", "https://www.davivienda.hn", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion5 = InstitucionBancaria.New("Banco Lafise Honduras", "Honduras", "Tegucigalpa, Francisco Morazán", "", "+504 2239 0800", "", "https://www.lafise.com", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion6 = InstitucionBancaria.New("Banco Promérica", "Honduras", "Tegucigalpa, Francisco Morazán", "", "+504 2270 0200", "", "https://www.promerica.hn", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion7 = InstitucionBancaria.New("Banco de Honduras", "Honduras", "Tegucigalpa, Francisco Morazán", "", "", "", "", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion8 = InstitucionBancaria.New("BANPAIS", "Honduras", "San Pedro Sula, Cortés", "", "+504 2516 9000", "", "https://www.banpais.hn", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion9 = InstitucionBancaria.New("Banco Azteca de Honduras", "Honduras", "Tegucigalpa, Francisco Morazán", "", "+504 2234 1100", "", "https://www.bancoazteca.com.hn", TipoInstitucionBancaria.Banco, createdBy);
                        var newInstitucion10 = InstitucionBancaria.New("Banco Hondureño del Café (BANHCAFE)", "Honduras", "Tegucigalpa, Francisco Morazán", "", "", "", "", TipoInstitucionBancaria.Banco, createdBy);

                        var coop1 = InstitucionBancaria.New("Cooperativa Elga", "Honduras", "Tegucigalpa", "", "", "", "", TipoInstitucionBancaria.Cooperativa, createdBy);
                        var coop2 = InstitucionBancaria.New("Cooperativa Ocotepeque", "Honduras", "Ocotepeque", "", "", "", "", TipoInstitucionBancaria.Cooperativa, createdBy);
                        var coop3 = InstitucionBancaria.New("Cooperativa Educadores de Honduras (Coacehl)", "Honduras", "Tegucigalpa", "", "", "", "", TipoInstitucionBancaria.Cooperativa, createdBy);
                        var coop4 = InstitucionBancaria.New("Cooperativa Sagrada Familia", "Honduras", "Comayagüela", "", "", "", "", TipoInstitucionBancaria.Cooperativa, createdBy);
                        var coop5 = InstitucionBancaria.New("Cooperativa Chorotega", "Honduras", "Tegucigalpa", "", "", "", "", TipoInstitucionBancaria.Cooperativa, createdBy);

                        var efectivo = InstitucionBancaria.New("Efectivo", "", "", "", "", "", "", TipoInstitucionBancaria.Otro, createdBy);

                        nuevasInstitucionesPrevias.Add(newInstitucion1);
                        nuevasInstitucionesPrevias.Add(newInstitucion2);
                        nuevasInstitucionesPrevias.Add(newInstitucion3);
                        nuevasInstitucionesPrevias.Add(newInstitucion4);
                        nuevasInstitucionesPrevias.Add(newInstitucion5);
                        nuevasInstitucionesPrevias.Add(newInstitucion6);
                        nuevasInstitucionesPrevias.Add(newInstitucion7);
                        nuevasInstitucionesPrevias.Add(newInstitucion8);
                        nuevasInstitucionesPrevias.Add(newInstitucion9);
                        nuevasInstitucionesPrevias.Add(newInstitucion10);

                        nuevasInstitucionesPrevias.Add(coop1);
                        nuevasInstitucionesPrevias.Add(coop2);
                        nuevasInstitucionesPrevias.Add(coop3);
                        nuevasInstitucionesPrevias.Add(coop4);
                        nuevasInstitucionesPrevias.Add(coop5);

                        nuevasInstitucionesPrevias.Add(efectivo);

                        foreach (var newInstitucion in nuevasInstitucionesPrevias)
                        {
                            var existe = instucionesBancarias.Where(x => x.Nombre == newInstitucion.Nombre).Any();

                            if (!existe)
                            {
                                nuevasInstituciones.Add(newInstitucion);
                            }
                        }


                        if (nuevasInstituciones.Any())
                        {
                            await _context.InstitucionBancaria.AddRangeAsync(nuevasInstituciones);
                            await _context.SaveChangesAsync();

                            return AppResult.New(true, $"{nuevasInstituciones.Count()} InstitucionBancarias creadas exitosamente");
                        }
                        else
                        {
                            return AppResult.New(false, "No se crearon InstitucionBancarias porque ya fueron creadas todas las genericas anteriormente!");
                        }


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
            public class CommandIBU : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Nombre { get; set; }
                public string Pais { get; set; }
                public string Direccion { get; set; }
                public string Direccion2 { get; set; }
                public string Telefono { get; set; }
                public string Telefono2 { get; set; }
                public string SitioWeb { get; set; }
                public TipoInstitucionBancaria TipoInstitucion { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandIBU, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandIBU cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var ib = await _context.InstitucionBancaria.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if(ib == null) { return AppResult.New(false, "Institucion Bancaria no existe"); }


                        var queSeActualizo = "";
                        if (cmd.Nombre != null && cmd.Nombre.Length > 1 && cmd.Nombre != ib.Nombre)
                        {
                            ib.Nombre = cmd.Nombre;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Nombre"; } else { queSeActualizo += "Nombre"; }
                        }
                        if (cmd.Pais != null && cmd.Pais.Length > 1 && cmd.Pais != ib.Pais)
                        {
                            ib.Pais = cmd.Pais;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Pais"; } else { queSeActualizo += "Pais"; }
                        }
                        if (cmd.Direccion != null && cmd.Direccion.Length > 1 && cmd.Direccion != ib.Direccion)
                        {
                            ib.Direccion = cmd.Direccion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Direccion"; } else { queSeActualizo += "Direccion"; }
                        }
                        if (cmd.Direccion2 != null && cmd.Direccion2.Length > 1 && cmd.Direccion2 != ib.Direccion2)
                        {
                            ib.Direccion2 = cmd.Direccion2;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Direccion2"; } else { queSeActualizo += "Direccion2"; }
                        }
                        if (cmd.Telefono != null && cmd.Telefono.Length > 1 && cmd.Telefono != ib.Telefono)
                        {
                            ib.Telefono = cmd.Telefono;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Telefono"; } else { queSeActualizo += "Telefono"; }
                        }
                        if (cmd.Telefono2 != null && cmd.Telefono2.Length > 1 && cmd.Telefono2 != ib.Telefono2)
                        {
                            ib.Telefono2 = cmd.Telefono2;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Telefono2"; } else { queSeActualizo += "Telefono2"; }
                        }
                        if (cmd.SitioWeb != null && cmd.SitioWeb.Length > 1 && cmd.SitioWeb != ib.SitioWeb)
                        {
                            ib.SitioWeb = cmd.SitioWeb;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", SitioWeb"; } else { queSeActualizo += "SitioWeb"; }
                        }
                        if (cmd.TipoInstitucion != ib.TipoInstitucion)
                        {
                            ib.TipoInstitucion = cmd.TipoInstitucion;
                            ib.TipoInstitucion_Descripcion = TipoInstitucionBancariaDescripcion.GetEstadoTexto((int)cmd.TipoInstitucion);

                            if (queSeActualizo.Length > 1) { queSeActualizo += ", TipoInstitucion"; } else { queSeActualizo += "TipoInstitucion"; }
                        }


                        if (queSeActualizo.Length > 1)
                        {
                            var modificadoPor = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                            if (cmd.ModifiedBy == null || cmd.ModifiedBy == Guid.Empty)
                            {
                                cmd.ModifiedBy = modificadoPor;
                            }

                            ib.ModifiedBy = (Guid)cmd.ModifiedBy;
                            ib.ModifiedDate = DateTime.Now;

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
            public class CommandIBE : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandIBE, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandIBE cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var ib = await _context.InstitucionBancaria.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if (ib == null) { return AppResult.New(false, "Institucion Bancaria no existe"); }

                        var cuentasBancariasConInstitucion = await _context.CuentaBancaria.Where(x => x.InstitucionBancariaId == ib.Id && !x.IsSoftDeleted).AnyAsync();
                        if (cuentasBancariasConInstitucion)
                        {
                            throw new Exception("No es posible eliminar, hay Cuentas Bancarias con esta Institucion Bancaria");
                        }


                        ib.IsSoftDeleted = true;
                        ib.ModifiedDate = DateTime.Now;
                        if (cmd.ModifiedBy != null && cmd.ModifiedBy != Guid.Empty)
                        {
                            ib.ModifiedBy = (Guid)cmd.ModifiedBy;
                        }

                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Institucion Bancaria eliminada con exito!");
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
            public class CommandIBH : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public bool Enabled { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandIBH, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandIBH cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var ib = await _context.InstitucionBancaria.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        if (ib == null) { return AppResult.New(false, "Institucion Bancaria no existe"); }

                        if(ib.Enabled == cmd.Enabled)
                        {
                            if(cmd.Enabled)
                            {
                                { return AppResult.New(false, "Institucion Bancaria ya esta Habilitada"); }
                            }
                            else
                            {
                                { return AppResult.New(false, "Institucion Bancaria ya esta Deshabilitida"); }
                            }
                        }

                        var cuentasBancariasConInstitucion = await _context.CuentaBancaria.Where(x => x.InstitucionBancariaId == ib.Id && !x.IsSoftDeleted).AnyAsync();
                        if (cmd.Enabled == false && cuentasBancariasConInstitucion)
                        {
                            throw new Exception("No es posible DesHabilitar, hay Cuentas Bancarias con esta Institucion Bancaria");
                        }

                        ib.Enabled = cmd.Enabled;
                        ib.ModifiedDate = DateTime.Now;
                        if (cmd.ModifiedBy != null && cmd.ModifiedBy != Guid.Empty)
                        {
                            ib.ModifiedBy = (Guid)cmd.ModifiedBy;
                        }

                        await _context.SaveChangesAsync();

                        var msj = cmd.Enabled ? "Habiliada" : "Deshabilitada";

                        return AppResult.New(true, $"Institucion Bancaria {msj} con exito!");
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
            public class QueryI : IRequest<List<InstitucionBancariaVm>>
            {

            }
            public class QueryHandler : IRequestHandler<QueryI, List<InstitucionBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<InstitucionBancariaVm>> Handle(QueryI query, CancellationToken cancellationToken)
                {
                    var index = await _context.InstitucionBancaria.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<InstitucionBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }



        public class GetByPais
        {
            public class QueryGP : IRequest<List<InstitucionBancariaVm>>
            {
                public string Pais { get; set; }
            }
            public class QueryHandler : IRequestHandler<QueryGP, List<InstitucionBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<InstitucionBancariaVm>> Handle(QueryGP query, CancellationToken cancellationToken)
                {
                    var index = await _context.InstitucionBancaria.Where(x => x.Pais.ToLower().Contains(query.Pais.ToLower()) && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<InstitucionBancariaVm>()
                        .ToListAsync();

                    return index;
                }
            }
        }



        public class GetTiposInstitucionBancaria
        {
            public class TipoInstitucionBancariaVm
            {
                public int Id { get; set; }
                public string Descripcion { get; set; }
            }
            public class Command : IRequest<List<TipoInstitucionBancariaVm>>
            {

            }

            public class CommandHandler : IRequestHandler<Command, List<TipoInstitucionBancariaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<TipoInstitucionBancariaVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    // Enumeramos todos los valores del enum
                    var tipos = Enum.GetValues(typeof(TipoInstitucionBancaria))
                        .Cast<TipoInstitucionBancaria>()
                        .Select(t => new TipoInstitucionBancariaVm
                        {
                            Id = (int)t,
                            Descripcion = TipoInstitucionBancariaDescripcion.GetEstadoTexto((int)t)
                        })
                        .ToList();

                    return tipos;
                }
            }
        }








    }
}

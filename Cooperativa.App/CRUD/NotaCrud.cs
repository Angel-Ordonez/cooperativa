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
using Cooperativa.App.Domain.Model.Prestamos;
using Microsoft.EntityFrameworkCore;
using Cooperativa.App.Utilidades;
using Cooperativa.App.Domain.Model.Entidad;
using Microsoft.AspNetCore.Components.Forms;
using static Cooperativa.App.Domain.Model.Entidad.Nota;
using Mapster;
using static Cooperativa.App.CRUD.PrestamoCrud;

namespace Cooperativa.App.CRUD
{
    public class NotaCrud
    {
        public class Crear
        {
            public class Command : IRequest<AppResult>
            {
                public Guid? PrestamoId { get; set; }
                public Guid? PersonaId { get; set; }
                public TipoNota Tipo { get; set; }
                public string Titulo { get; set; }
                public string Contenido { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        if(cmd.PersonaId != null && cmd.PersonaId != Guid.Empty)
                        {
                            var persona = await _context.Persona.Where(x => x.Id == cmd.PersonaId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            persona.ThrowIfNull("Persona no existe");
                        }
                        else if(cmd.PrestamoId != null && cmd.PrestamoId != Guid.Empty)
                        {
                            var prestamo = await _context.Prestamo.Where(x => x.Id == cmd.PrestamoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            prestamo.ThrowIfNull("Prestamo no existe");
                        }
                        else
                        {
                            throw new Exception("Debe existir un elemento asociado para esta nota");
                        }

                        EstadoNota estado = new EstadoNota();


                        if(cmd.Tipo == TipoNota.Informativa)
                        {
                            estado = EstadoNota.Informativa;
                        }
                        else
                        {
                            estado = EstadoNota.Pendiente;
                        }

                        var newNota = Nota.New(cmd.PrestamoId, cmd.PersonaId, cmd.Titulo, cmd.Contenido, cmd.Tipo, estado, createdBy);


                        await _context.Nota.AddAsync(newNota);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Nota creada exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }




        public class CrearAvanzado
        {
            public class Command : IRequest<AppResult>
            {
                public string Razon { get; set; }
                public string Referencia { get; set; }
                public TipoNota Tipo { get; set; }
                public string Titulo { get; set; }
                public string Contenido { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        Guid? personaId = null;
                        Guid? prestamoId = null;



                        if (cmd.Razon.ToLower().Trim() ==  "persona")
                        {
                            var refrencia = cmd.Referencia.ToLower().Trim();
                            if (refrencia == null || refrencia.Length  == 0)
                            {
                                throw new Exception("Codigo Persona no es valido, por favor ingreselo como Referencia");
                            }

                            var persona = await _context.Persona.Where(x => refrencia.Contains(x.CodigoPersona) && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            persona.ThrowIfNull($"Persona no existe con Codigo {refrencia}");

                            personaId = persona.Id;
                        }
                        else if (cmd.Razon.ToLower().Trim() == "prestamo")
                        {
                            var refrencia = cmd.Referencia.ToLower().Trim();
                            if (refrencia == null || refrencia.Length == 0)
                            {
                                throw new Exception("Codigo de PRESTAMO no es valido, por favor ingreselo como Referencia");
                            }

                            var prestamo = await _context.Prestamo.Where(x => refrencia.Contains(x.CodigoPrestamo) && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            prestamo.ThrowIfNull($"Prestamo no existe con Codigo {refrencia}");

                            prestamoId = prestamo.Id;
                        }
                        else if (cmd.Razon.ToLower().Trim() == "general")
                        {

                        }
                        else
                        {
                            throw new Exception("Debe existir un elemento asociado o razon para crear esta nota");
                        }

                        EstadoNota estado = new EstadoNota();


                        if (cmd.Tipo == TipoNota.Informativa)
                        {
                            estado = EstadoNota.Informativa;
                        }
                        else
                        {
                            estado = EstadoNota.Pendiente;
                        }

                        var newNota = Nota.New(prestamoId, personaId, cmd.Titulo, cmd.Contenido, cmd.Tipo, estado, createdBy);


                        await _context.Nota.AddAsync(newNota);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Nota creada exitosamente");
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
            public class Command : IRequest<AppResult>
            {
                public List<Guid> Ids { get; set; }
                public Guid? UsuarioId { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        if(cmd.UsuarioId == null || cmd.UsuarioId == Guid.Empty)
                        {
                            cmd.UsuarioId = createdBy;
                        }


                        var ids = cmd.Ids.Distinct().ToList();

                        var notas = await _context.Nota.Where(x => ids.Contains(x.Id) && !x.IsSoftDeleted).ToListAsync();

                        if(ids.Count() != notas.Count())
                        {
                            for(int i=0; i< ids.Count; i++)
                            {
                                var id = ids.ElementAt(i);
                                var existe = notas.Where(x => x.Id == id).FirstOrDefault();

                                if(existe == null)
                                {
                                    throw new Exception($"No existe Nota con Id: {id}");
                                }
                            }
                        }


                        foreach(var nota in notas)
                        {
                            if (nota.Estado != EstadoNota.Informativa)
                            {
                                throw new Exception($"No es posible eliminar Nota porque no es Informativa: 👉 {nota.Contenido}");
                            }

                            nota.Eliminar();
                            nota.ModifiedBy = (Guid)cmd.UsuarioId;
                        }


                        await _context.SaveChangesAsync();

                        return AppResult.New(true, $"{notas.Count()} Nota eliminadas exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }




        public class AtenderNotaPendiente
        {
            public class Command : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Detalle { get; set; }
                public Guid? UsuarioId { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        if (cmd.UsuarioId == null || cmd.UsuarioId == Guid.Empty)
                        {
                            cmd.UsuarioId = createdBy;
                        }

                        var nota = await _context.Nota.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        nota.ThrowIfNull("No se encontro Registro de Nota");

                        if(nota.Estado != EstadoNota.Pendiente)
                        {
                            throw new Exception("Nota no se encuentra en estado Pendiente de atender");
                        }

                        nota.Estado = EstadoNota.Resuelta;
                        nota.Estado_Descripcion = EstadoNotaDescripcion.GetEstadoTexto((int)EstadoNota.Resuelta);
                        nota.Detalle = cmd.Detalle;
                        nota.ModifiedBy = (Guid)cmd.UsuarioId;
                        nota.ModifiedDate = DateTime.Now;

                        await _context.SaveChangesAsync();

                        return AppResult.New(true, $"Nota atendida exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }





        public class GetByPrestamosId
        {
            public class NotaRes
            {
                public Guid PrestamoId { get; set; }
                public List<NotaVm> Notas { get; set; }
            }

            public class Command : IRequest<List<NotaRes>>
            {
                public List<Guid> PrestamosId { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<NotaRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<NotaRes>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var notas = await _context.Nota.Where(x => cmd.PrestamosId.Contains((Guid)x.PrestamoId))
                        .ProjectToType<NotaVm>()
                        .ToListAsync();

                    List<NotaRes> res = new List<NotaRes>();
                    var prestamosIds = notas.Select(X => X.PrestamoId).Distinct().ToList();

                    for(int i = 0; i < prestamosIds.Count; i++)
                    {
                        var prestamoId = prestamosIds.ElementAt(i);

                        var notasPrestamo = notas.Where(x => x.PrestamoId == prestamoId).ToList();

                        var newRes = new NotaRes
                        {
                            PrestamoId = (Guid)prestamoId,
                            Notas = notasPrestamo
                        };

                        res.Add(newRes);
                    }

                    return res;
                }
            }
        }




        public class GetByPersonasId
        {
            public class NotaRes
            {
                public Guid PersonaId { get; set; }
                public List<NotaVm> Notas { get; set; }
            }

            public class Command : IRequest<List<NotaRes>>
            {
                public List<Guid> PersonasId { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<NotaRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<NotaRes>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var notas = await _context.Nota.Where(x => cmd.PersonasId.Contains((Guid)x.PersonaId))
                        .ProjectToType<NotaVm>()
                        .ToListAsync();

                    List<NotaRes> res = new List<NotaRes>();
                    var personasIds = notas.Select(X => X.PersonaId).Distinct().ToList();

                    for (int i = 0; i < personasIds.Count; i++)
                    {
                        var personaId = personasIds.ElementAt(i);

                        var notasPrestamo = notas.Where(x => x.PersonaId == personaId).ToList();

                        var newRes = new NotaRes
                        {
                            PersonaId = (Guid)personaId,
                            Notas = notasPrestamo
                        };
                        res.Add(newRes);
                    }


                    return res;
                }
            }
        }





        public class GetNotasByFiltros
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

            public class Command : IRequest<List<NotaRes>>
            {
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<NotaRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<NotaRes>> Handle(Command cmd, CancellationToken cancellationToken)
                {

                    List<NotaRes> notasRes = new List<NotaRes>();
                    var notas = new List<Nota>();

                    if (cmd.Index)
                    {
                        notas = await _context.Nota.Where(x => !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            //.ProjectToType<NotaVm>()
                            .ToListAsync();
                    }
                    else if (cmd.Anio != 0)
                    {
                        notas = await _context.Nota.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == cmd.Anio)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            //.ProjectToType<NotaVm>()
                            .ToListAsync();
                    }
                    else if (cmd.AnioMes != null)
                    {
                        notas = await _context.Nota.Where(x => x.CreatedDate.Date.Year == cmd.AnioMes.Anio && x.CreatedDate.Date.Month == cmd.AnioMes.Mes && !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            //.ProjectToType<NotaVm>()
                            .ToListAsync();
                    }
                    else if (cmd.RangoFechas != null)
                    {
                        var inicio = cmd.RangoFechas.FechaInicio.Date;
                        var fin = cmd.RangoFechas.FechaFin.Date;

                        notas = await _context.Nota.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin && !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            //.ProjectToType<NotaVm>()
                            .ToListAsync();
                    }

                    var personasIds = notas.Where(x => x.PersonaId != null && x.PersonaId != Guid.Empty).Select(X => X.PersonaId).ToList();
                    var prestamosIds = notas.Where(x => x.PrestamoId != null && x.PrestamoId != Guid.Empty).Select(X => X.PrestamoId).ToList();

                    var personas = await _context.Persona.Where(x => personasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .Select(x => new {x.Id, x.Nombre, x.Apellido})
                        .AsNoTracking()
                        .ToListAsync();
                    var prestamos = await _context.Prestamo.Where(x => prestamosIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .Select(x => new { x.Id, x.CodigoPrestamo })
                        .AsNoTracking()
                        .ToListAsync();

                    foreach(var nota in notas)
                    {
                        var notaVm = nota.Adapt<NotaRes>();
                        if(nota.PrestamoId != null && nota.PrestamoId != Guid.Empty)
                        {
                            notaVm.Razon = "Prestamo";
                            var prestamo = prestamos.Where(x => x.Id == nota.PrestamoId).FirstOrDefault();
                            if(prestamo != null)
                            {
                                notaVm.Referencia = prestamo.CodigoPrestamo;
                            }
                        }
                        else if(nota.PersonaId != null && nota.PersonaId != Guid.Empty)
                        {
                            var persona = personas.Where(x => x.Id == nota.PersonaId).FirstOrDefault();
                            notaVm.Razon = "Persona";
                            if(persona != null)
                            {
                                notaVm.Referencia = persona.Nombre + " " + persona.Apellido;
                            }
                        }
                        else
                        {
                            notaVm.Razon = "Nota en General";
                            notaVm.Referencia = "***";
                        }

                        notasRes.Add(notaVm);
                    }




                    return notasRes;
                }
            }
        }





        public class GetTiposNota
        {
            public class Command : IRequest<List<TipoNotaVm>>
            {

            }

            public class CommandHandler : IRequestHandler<Command, List<TipoNotaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<TipoNotaVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    // Enumeramos todos los valores del enum
                    var tipos = Enum.GetValues(typeof(TipoNota))
                        .Cast<TipoNota>()
                        .Select(t => new TipoNotaVm
                        {
                            Id = (int)t,
                            Tipo_Descripcion = TipoNotaDescripcion.GetEstadoTexto((int)t)
                        })
                        .ToList();

                    return tipos;
                }
            }
        }



        public class GetRazonRapidaNota
        {
            public class RazonRapidaVm
            {
                public int Id { get; set; }
                public string Razon { get; set; }
            }
            public class Command : IRequest<List<RazonRapidaVm>>
            {

            }

            public class CommandHandler : IRequestHandler<Command, List<RazonRapidaVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<RazonRapidaVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var razones = new List<RazonRapidaVm>();
                    razones.Add(new RazonRapidaVm { Id = 1, Razon = "General" });
                    razones.Add(new RazonRapidaVm { Id = 2, Razon = "Persona" });
                    razones.Add(new RazonRapidaVm { Id = 3, Razon = "Prestamo" });


                    return razones;
                }
            }
        }

























        public class TipoNotaVm
        {
            public int Id { get; set; }
            public string Tipo_Descripcion { get; set; }
        }

    }
}

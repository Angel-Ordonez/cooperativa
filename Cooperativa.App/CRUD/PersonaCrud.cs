using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Cooperativa.App.CRUD
{
    public class PersonaCrud
    {



        public class RecalcularEdades
        {
            public class CommandRecalcularEdades : IRequest<AppResult>
            {

            }

            public class CommandHandler : IRequestHandler<CommandRecalcularEdades, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandRecalcularEdades cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        //var usuario = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var personas = await _context.Persona.Where(x => !x.IsSoftDeleted).ToListAsync();
                        var hoy = DateTime.Today;
                        int contador = 0;

                        foreach (var persona in personas)
                        {
                            var fechaNacimiento = persona.FechaNacimiento;

                            int edad = hoy.Year - fechaNacimiento.Year;

                            // Si aún no ha cumplido años este año, restar 1
                            if (fechaNacimiento.Date > hoy.AddYears(-edad))
                            {
                                edad--;
                            }

                            if(edad != persona.Edad)
                            {
                                persona.Edad = edad;
                                contador++;
                            }
                        }

                        if(contador > 0)
                        {
                            await _context.SaveChangesAsync();
                            return AppResult.New(true, $"Se actualizaron {contador} edades exitosamente");
                        }
                        else
                        {
                            return AppResult.New(false, $"No hay edades que actualizar");
                        }

                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }




        public class GetCumpleanos
        {
            public class ResponseVm
            {
                public Guid Id { get; set; }
                public string NombrePersona { get; set; }
                public int Edad { get; set; }
                public int NuevaEdad { get; set; }
                public DateTime Cumpleanos { get; set; }
            }
            public class Query : IRequest<List<ResponseVm>>
            {

            }

            public class QueryHandler : IRequestHandler<Query, List<ResponseVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<ResponseVm>> Handle(Query cmd, CancellationToken cancellationToken)
                {
                    //var personas = await _context.Persona.Where(x => !x.IsSoftDeleted).AsNoTracking().ToListAsync();

                    //List < ResponseVm > respuestas = new List < ResponseVm >();

                    //foreach (var persona in personas)
                    //{
                    //    var newItem = new ResponseVm
                    //    {
                    //        Edad = persona.Edad,
                    //        Id = persona.Id,
                    //        NombrePersona = persona.Nombre + " " + persona.Apellido
                    //    };
                    //    respuestas.Add(newItem);
                    //}

                    var respuestas = await _context.Persona
                        .Where(x => !x.IsSoftDeleted)
                        .Select(x => new ResponseVm
                        {
                            Id = x.Id,
                            NombrePersona = x.Nombre + " " + x.Apellido,
                            Edad = x.Edad,
                            Cumpleanos = new DateTime(DateTime.Now.Year, x.FechaNacimiento.Month, x.FechaNacimiento.Day),
                            NuevaEdad = DateTime.Today.Year - x.FechaNacimiento.Year - (DateTime.Today < x.FechaNacimiento.AddYears(DateTime.Today.Year - x.FechaNacimiento.Year) ? 1 : 0)
                        })
                        .AsNoTracking()
                        .ToListAsync();

                    //Ventajas:
                    //    Solo trae las columnas necesarias
                    //    No necesitas crear la lista y hacer foreach manual
                    //    EF genera un SQL más ligero

                    return respuestas;
                }
            }
        }



















    }
}

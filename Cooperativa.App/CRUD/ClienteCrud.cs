using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Utilidades;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.SocioCrud;

namespace Cooperativa.App.CRUD
{
    public class ClienteCrud
    {
        public class Create
        {
            public class Command : IRequest<AppResult>
            {
                public string Nombre { get; set; }
                public string Apellido { get; set; }
                public DateTime FechaNacimiento { get; set; }
                public string TipoIdentificacion { get; set; }
                public string Identificacion { get; set; }
                public string LugarTrabajo { get; set; }
                public string Ocupacion { get; set; }
                public string EstadoCivil { get; set; }
                public string Genero { get; set; }
                public string Pais { get; set; }
                public string Ciudad { get; set; }
                public string Direccion { get; set; }
                public string Correo { get; set; }
                public string Telefono { get; set; }
                public string Telefono2 { get; set; }
                public string OtroContacto { get; set; }
                public string RTN { get; set; }
                public string Observacion { get; set; }
                public DateTime FechaDeIngreso { get; set; }
                public string RecomendadoPor { get; set; }
                public string Nota { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase utilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = utilidadesBase;

                }

                public async Task<AppResult> Handle(Command command, CancellationToken cancellationToken)
                {

                    var cliente = await _context.Cliente.Where(x => x.Identificacion == command.Identificacion && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                    if (cliente != null)
                    {
                        return AppResult.New(false, $"Ya existe Cliente con ese Numero de Identidad. Su nombre es: {cliente.Nombre + " " + cliente.Apellido}");
                    }



                    #region Codigo Cliente   SOC-AO-23-1

                    var codigoPersona = "";

                    var nombre = command.Nombre;
                    var apellido = command.Apellido;

                    // Eliminar espacios en blanco al principio y al final
                    nombre = nombre.Trim();
                    apellido = apellido.Trim();

                    if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(apellido))
                    {
                        var primeRLetraNombre = char.ToUpper(nombre[0]);
                        var primerLetraAppellido = char.ToUpper(apellido[0]);
                        var anio = DateTime.Now.Year % 100;

                        var codigoEmpresa = "COOPAZ";
                        var codigoBaseSecuencial = $"CTE-{codigoEmpresa}-{anio}";
                        var secuencial = await _iUtilidadesBase.GenerarSecuencial("Cliente", codigoBaseSecuencial);

                        if (secuencial <= 0)
                            return AppResult.New(false, "Error al generar secuencial de socio.");

                        var codigoBase = $"CTE-{codigoEmpresa}-{primeRLetraNombre}{primerLetraAppellido}{anio}";
                        codigoPersona = $"{codigoBase}-{secuencial:D5}";
                    }
                    else
                    {
                        return AppResult.New(false, $"Hubo error al crear el CodigoPersona, Porfavor revise su Nombre y Apellido.");
                    }

                    #endregion



                    var recomendadoPorEsGuid = false;
                    var recomendadoPorGuid = new Guid();
                    string recomendadoNombre = "";
                    try
                    {
                        recomendadoPorGuid = Guid.Parse(command.RecomendadoPor);
                        recomendadoPorEsGuid = true;

                        var recomendadoPor = await _context.Persona.Where(x => x.Id == recomendadoPorGuid && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        recomendadoNombre = (recomendadoPor != null) ? recomendadoPor.Nombre + " " + recomendadoPor.Apellido : "";

                    }
                    catch (Exception ex)
                    {
                        recomendadoNombre = command.RecomendadoPor;
                    }


                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var newCliente = Cliente.New(command.Nombre, command.Apellido, command.TipoIdentificacion, command.Identificacion, command.FechaNacimiento, command.EstadoCivil, command.Genero, command.LugarTrabajo,
                            command.Ocupacion, recomendadoNombre, command.Observacion, command.Pais, command.Ciudad, command.Direccion, command.Correo, command.Telefono, command.Telefono2,
                            command.OtroContacto, command.RTN, codigoPersona, createdBy);

                        await _context.Cliente.AddAsync(newCliente);
                        await _context.SaveChangesAsync();



                        try
                        {
                            //Crear cuenta efectivo
                            var newCuentaBancaria = CuentaBancaria.New(newCliente.Id, null, "Efectivo", "Efectivo", TipoCuentaBancaria.Otro, createdBy);

                            await _context.CuentaBancaria.AddAsync(newCuentaBancaria);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {

                        }



                        return AppResult.New(true, $"Cliente {newCliente.CodigoPersona}", newCliente.Adapt<ClienteVm>());

                    }
                    catch(Exception e)
                    {
                        return AppResult.New(false, $"Error no controlado." + e.Message);
                    }


                }


            }


        }


        public class Update
        {
            public class CommandUpdateUser : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Nombre { get; set; }
                public string Apellido { get; set; }
                public DateTime? FechaNacimiento { get; set; }
                public string TipoIdentificacion { get; set; }
                public string Identificacion { get; set; }
                public string LugarTrabajo { get; set; }
                public string Ocupacion { get; set; }
                public string EstadoCivil { get; set; }
                public string Genero { get; set; }
                public string Pais { get; set; }
                public string Ciudad { get; set; }
                public string Direccion { get; set; }
                public string Correo { get; set; }
                public string Telefono { get; set; }
                public string Telefono2 { get; set; }
                public string OtroContacto { get; set; }
                public string RTN { get; set; }
                public string Observacion { get; set; }
                public DateTime FechaDeIngreso { get; set; }
                //public Guid? RecomendadoPor { get; set; }
                public string RecomendadoPor { get; set; }
                public string Nota { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandUpdateUserHandler : IRequestHandler<CommandUpdateUser, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandUpdateUserHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandUpdateUser command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var cliente = await _context.Cliente.Where(x => x.Id == command.Id && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (cliente == null)
                        {
                            return AppResult.New(false, $"No existe Cliente");
                        }

                        var modificadoPor = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        var queSeActualizo = "";

                        var recomendadoPorEsGuid = false;
                        var recomendadoPorGuid = new Guid();
                        try
                        {
                            recomendadoPorGuid = Guid.Parse(command.RecomendadoPor);
                            recomendadoPorEsGuid = true;
                        }
                        catch(Exception ex)
                        {
                            
                        }
                        if(recomendadoPorEsGuid && recomendadoPorGuid != null && recomendadoPorGuid != Guid.Empty)
                        {
                            var recomendadoPor = await _context.Persona.Where(x => x.Id == recomendadoPorGuid && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                            if (recomendadoPor == null) { throw new Exception("Persona RecomendadoPor no existe"); }
                            string recomendadoNombre = (recomendadoPor != null) ? recomendadoPor.Nombre + " " + recomendadoPor.Apellido : "";

                            if (recomendadoNombre.Length > 0 && recomendadoNombre != cliente.RecomendadoPor)
                            {
                                cliente.RecomendadoPor = recomendadoNombre;
                                if (queSeActualizo.Length > 1) { queSeActualizo += ", RecomendadoPor"; } else { queSeActualizo += "Recomendado Por"; }
                            }
                        }
                        if (!recomendadoPorEsGuid && command.RecomendadoPor != null && command.RecomendadoPor.Length > 1 && command.RecomendadoPor != cliente.RecomendadoPor)
                        {
                            cliente.RecomendadoPor = command.RecomendadoPor;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", RecomendadoPor"; } else { queSeActualizo += "Recomendado Por"; }
                        }
                        if (command.Nombre != null && command.Nombre.Length > 1 && command.Nombre != cliente.Nombre)
                        {
                            cliente.Nombre = command.Nombre;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Nombre"; } else { queSeActualizo += "Nombre"; }
                        }
                        if (command.Apellido != null && command.Apellido.Length > 1 && command.Apellido != cliente.Apellido)
                        {
                            cliente.Apellido = command.Apellido;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Apellido"; } else { queSeActualizo += "Apellido"; }
                        }
                        if (command.FechaNacimiento != null && command.FechaNacimiento != DateTime.MinValue && command.FechaNacimiento != DateTime.MaxValue && command.FechaNacimiento != cliente.FechaNacimiento)
                        {
                            cliente.FechaNacimiento = (DateTime)command.FechaNacimiento;
                            cliente.Edad = DateTime.Now.Year - command.FechaNacimiento.Value.Year;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", FechaNacimiento"; } else { queSeActualizo += "FechaNacimiento"; }
                        }
                        if (command.FechaDeIngreso != null && command.FechaDeIngreso != DateTime.MinValue && command.FechaDeIngreso != DateTime.MaxValue && command.FechaDeIngreso != cliente.CreatedDate)
                        {
                            cliente.CreatedDate = (DateTime)command.FechaDeIngreso;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", FechaDeIngreso"; } else { queSeActualizo += "FechaDeIngreso"; }
                        }
                        if (command.TipoIdentificacion != null && command.TipoIdentificacion.Length > 1 && command.TipoIdentificacion != cliente.TipoIdentificacion)
                        {
                            cliente.TipoIdentificacion = command.TipoIdentificacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", TipoIdentificacion"; } else { queSeActualizo += "TipoIdentificacion"; }
                        }
                        if (command.Identificacion != null && command.Identificacion.Length > 1 && command.Identificacion != cliente.Identificacion)
                        {
                            cliente.Identificacion = command.Identificacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Identificación"; } else { queSeActualizo += "Identificación"; }
                        }
                        if (command.LugarTrabajo != null && command.LugarTrabajo.Length > 1 && command.LugarTrabajo != cliente.LugarTrabajo)
                        {
                            cliente.LugarTrabajo = command.LugarTrabajo;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", LugarTrabajo"; } else { queSeActualizo += "LugarTrabajo"; }
                        }
                        if (command.Ocupacion != null && command.Ocupacion.Length > 1 && command.Ocupacion != cliente.Ocupacion)
                        {
                            cliente.Ocupacion = command.Ocupacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Ocupación"; } else { queSeActualizo += "Ocupación"; }
                        }
                        if (command.EstadoCivil != null && command.EstadoCivil.Length > 1 && command.EstadoCivil != cliente.EstadoCivil)
                        {
                            cliente.EstadoCivil = command.EstadoCivil;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", EstadoCivil"; } else { queSeActualizo += "Estado Civil"; }
                        }
                        if (command.Genero != null && command.Genero.Length > 1 && command.Genero != cliente.Genero)
                        {
                            cliente.Genero = command.Genero;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Genero"; } else { queSeActualizo += "Genero"; }
                        }
                        if (command.Pais != null && command.Pais.Length > 1 && command.Pais != cliente.Pais)
                        {
                            cliente.Pais = command.Pais;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Pais"; } else { queSeActualizo += "Pais"; }
                        }
                        if (command.Ciudad != null && command.Ciudad.Length > 1 && command.Ciudad != cliente.Ciudad)
                        {
                            cliente.Ciudad = command.Ciudad;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Ciudad"; } else { queSeActualizo += "Ciudad"; }
                        }
                        if (command.Direccion != null && command.Direccion.Length > 1 && command.Direccion != cliente.Direccion)
                        {
                            cliente.Direccion = command.Direccion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Dirección"; } else { queSeActualizo += "Dirección"; }
                        }
                        if (command.Correo != null && command.Correo.Length > 1 && command.Correo != cliente.Correo)
                        {
                            cliente.Correo = command.Correo;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Correo"; } else { queSeActualizo += "Correo electronico"; }
                        }
                        if (command.Telefono != null && command.Telefono.Length > 1 && command.Telefono != cliente.Telefono)
                        {
                            cliente.Telefono = command.Telefono;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Telefono"; } else { queSeActualizo += "Telefono"; }
                        }
                        if (command.Telefono2 != null && command.Telefono2.Length > 1 && command.Telefono2 != cliente.Telefono2)
                        {
                            cliente.Telefono2 = command.Telefono2;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Telefono2"; } else { queSeActualizo += "Telefono2"; }
                        }
                        if (command.OtroContacto != null && command.OtroContacto.Length > 1 && command.OtroContacto != cliente.OtroContacto)
                        {
                            cliente.OtroContacto = command.OtroContacto;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Otro Contacto"; } else { queSeActualizo += "Otro contacto"; }
                        }
                        if (command.RTN != null && command.RTN.Length > 1 && command.RTN != cliente.RTN)
                        {
                            cliente.RTN = command.RTN;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", RTN"; } else { queSeActualizo += "RTN"; }
                        }
                        if (command.Observacion != null && command.Observacion.Length > 1 && command.Observacion != cliente.Observacion)
                        {
                            cliente.Observacion = command.Observacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Observación"; } else { queSeActualizo += "Observación"; }
                        }
                        if (command.Nota != null && command.Nota.Length > 1 && command.Nota != cliente.Nota)
                        {
                            cliente.Nota = command.Nota;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Nota"; } else { queSeActualizo += "Nota"; }
                        }

                        if(queSeActualizo.Length > 1)
                        {
                            if(command.ModifiedBy == null || command.ModifiedBy == Guid.Empty)
                            {
                                command.ModifiedBy = modificadoPor;
                            }

                            cliente.ModifiedBy = (Guid)command.ModifiedBy;
                            cliente.ModifiedDate = DateTime.Now;

                            await _context.SaveChangesAsync();
                            return AppResult.New(true, $"Se actualizo: {queSeActualizo}");
                        }
                        else
                        {
                            return AppResult.New(true, $"Nada que actualizar");
                        }

                    }
                    catch (Exception e)
                    {
                        return AppResult.New(false, e.Message);
                    }
                }
            }
        }



        public class HabilitarCliente
        {
            public class Commandhc : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public bool Enabled { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<Commandhc, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Commandhc query, CancellationToken cancellationToken)
                {
                    try
                    {
                        var persona = await _context.Cliente.Where(x => x.Id == query.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        if (persona == null)
                        {
                            throw new Exception("Usuario no existe");
                        }

                        var prestamosActivos = await _context.Prestamo.Where(x => x.Cliente.Id == query.Id && !x.IsSoftDeleted && ((int)x.Estado) == 1).ToListAsync();
                        if(prestamosActivos.Any() && query.Enabled == false)
                        {
                            throw new Exception($"Usuario tiene {prestamosActivos.Count()} Prestamos activos!");
                        }

                        persona.Enabled = query.Enabled;
                        persona.ModifiedDate = DateTime.Now;
                        if (query.ModifiedBy != null && query.ModifiedBy != Guid.Empty)
                        {
                            persona.ModifiedBy = (Guid)query.ModifiedBy;
                        }


                        await _context.SaveChangesAsync();

                        var mensaje = "";
                        if (query.Enabled == true)
                        {
                            mensaje = "Habilitado";
                        }
                        else
                        {
                            mensaje = "DesHabilitado";
                        }

                        return AppResult.New(true, "Usuario " + mensaje);
                    }
                    catch(Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }


        public class EliminarCliente
        {
            public class Commandec : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<Commandec, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Commandec query, CancellationToken cancellationToken)
                {
                    try
                    {
                        var persona = await _context.Cliente.Where(x => x.Id == query.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        if (persona == null)
                        {
                            throw new Exception("Usuario no existe");
                        }

                        var prestamosActivos = await _context.Prestamo.Where(x => x.Cliente.Id == query.Id && !x.IsSoftDeleted && ((int)x.Estado) == 1).ToListAsync();
                        if (prestamosActivos.Any())
                        {
                            throw new Exception($"Usuario tiene {prestamosActivos.Count()} Prestamos activos!");
                        }


                        persona.IsSoftDeleted = true;
                        persona.ModifiedDate = DateTime.Now;
                        if (query.ModifiedBy != null && query.ModifiedBy != Guid.Empty)
                        {
                            persona.ModifiedBy = (Guid)query.ModifiedBy;
                        }


                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Usuario eliminado con exito!");
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
            public class Query : IRequest<List<ClienteVm>>
            {

            }

            public class QueryHandler : IRequestHandler<Query, List<ClienteVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<ClienteVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var personas = await _context.Cliente.Where(x => !x.IsSoftDeleted).ProjectToType<ClienteVm>()
                        .AsNoTracking()
                        .ProjectToType<ClienteVm>()
                        .ToListAsync();

                    return personas;
                }
            }
        }





        public class IndexClientesYPrestamosActivos
        {
            public class InfoPrestamoCliente
            {
                public Guid ClienteId { get; set; }
                public string Nombre { get; set; }
                public string CodigoPersona { get; set; }
                public int CantidadPrestamosActivos { get; set; }
                public decimal DeudaTotalCapital { get; set; }
                public bool AplicaParaPrestamo { get; set; }
                public string Detalle { get; set; }
                public List<CuentaBancariaVm> CuentaBancarias { get; set; }
            }
            public class CuentaBancariaVm
            {
                public Guid Id { get; set; }
                public Guid? PersonaId { get; set; }
                public string NumeroCuenta { get; set; }
                public string NumeroTarjeta { get; set; }
                public string InstitucionBancariaNombre { get; set; }
                public string CuentaConcatenado { get; set; }
            }

            public class Query : IRequest<List<InfoPrestamoCliente>>
            {

            }

            public class QueryHandler : IRequestHandler<Query, List<InfoPrestamoCliente>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<InfoPrestamoCliente>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var personas = await _context.Cliente.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .ToListAsync();

                    var clientesIds = personas.Select(x => x.Id).ToList();

                    var prestamosActivos = await _context.Prestamo.Where(x => x.Estado == EstadoPrestamo.Vigente && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ToListAsync();

                    var cuentasBancariasAll = await _context.CuentaBancaria.Where(x => clientesIds.Contains((Guid)x.PersonaId) && !x.IsSoftDeleted)
                        .Include(x => x.InstitucionBancaria)
                        .ProjectToType<CuentaBancariaVm>()
                        .AsNoTracking()
                        .ToListAsync();

                    var configuracionPrestamos = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                    configuracionPrestamos.ThrowIfNull("No se encontro ConfiguracionPrestamo");
                     
                    
                    var clientesInfoPrestamos = new List<InfoPrestamoCliente>();

                    foreach (var cliente in personas)
                    {
                        var newRes = new InfoPrestamoCliente
                        {
                            ClienteId = cliente.Id,
                            Nombre = cliente.Nombre + " " + cliente.Apellido,
                            CodigoPersona = cliente.CodigoPersona,
                            Detalle = "",
                            CuentaBancarias = new List<CuentaBancariaVm>()
                        };

                        var pActivos = prestamosActivos.Where(x => x.ClienteId == cliente.Id).ToList();
                        if (pActivos.Any())
                        {
                            var deudaCapitalActual = pActivos.Sum(x => x.RestaCapital);
                            int cantidadPrestamos = pActivos.Count();

                            newRes.CantidadPrestamosActivos = cantidadPrestamos;
                            newRes.DeudaTotalCapital = deudaCapitalActual;

                            if(deudaCapitalActual < configuracionPrestamos.MontoMaximo && cantidadPrestamos < configuracionPrestamos.CantidadPrestamosPorCliente)
                            {
                                newRes.AplicaParaPrestamo = true;
                            }

                            if (deudaCapitalActual >= configuracionPrestamos.MontoMaximo)
                            {
                                newRes.Detalle += $"Cliente se encuentra en el monto maximo de {configuracionPrestamos.MontoMaximo.ToString("N2")} *** ";
                            }
                            if ( cantidadPrestamos >= configuracionPrestamos.CantidadPrestamosPorCliente)
                            {
                                newRes.Detalle += $"Cliente se encuentra en el maximo de {configuracionPrestamos.CantidadPrestamosPorCliente.ToString()} de prestamos configurados *** ";
                            }

                        }
                        else
                        {

                            newRes.CantidadPrestamosActivos = 0;
                            newRes.DeudaTotalCapital = 0;
                            newRes.AplicaParaPrestamo = true;
                        }

                        var cuentasBancarias = cuentasBancariasAll.Where(x => x.PersonaId == cliente.Id).ToList();
                        foreach(var cuenta in cuentasBancarias)
                        {
                            if(cuenta.NumeroCuenta.ToLower() != "efectivo")
                            {
                                cuenta.CuentaConcatenado = cuenta.NumeroCuenta + " - " + cuenta.InstitucionBancariaNombre;
                            }
                            else
                            {
                                cuenta.CuentaConcatenado = cuenta.NumeroCuenta;
                            }
                        }
                        newRes.CuentaBancarias = cuentasBancarias;

                        clientesInfoPrestamos.Add(newRes);
                    }



                    return clientesInfoPrestamos;
                }
            }
        }





        public class IndexHabilitados
        {
            public class Query : IRequest<List<ClienteVm>>
            {

            }

            public class QueryHandler : IRequestHandler<Query, List<ClienteVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<ClienteVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var personas = await _context.Cliente.Where(x => !x.IsSoftDeleted && x.Enabled).ProjectToType<ClienteVm>()
                        .AsNoTracking()
                        .ProjectToType<ClienteVm>()
                        .ToListAsync();

                    return personas;
                }
            }
        }





        public class GetClienteById
        {
            public class Query : IRequest<ClienteVm>
            {
                public Guid Id { get; set; }
            }

            public class QueryHandler : IRequestHandler<Query, ClienteVm>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<ClienteVm> Handle(Query query, CancellationToken cancellationToken)
                {

                    var cliente = await _context.Cliente.Where(x => x.Id == query.Id && !x.IsSoftDeleted).ProjectToType<ClienteVm>()
                        .AsNoTracking()
                        .ProjectToType<ClienteVm>()
                        .FirstOrDefaultAsync();

                    return cliente;
                }
            }
        }










        public class ClienteVm
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime FechaNacimiento { get; set; }
            public string TipoIdentificacion { get; set; }
            public string Identificacion { get; set; }
            public string LugarTrabajo { get; set; }
            public string Ocupacion { get; set; }
            public int Edad { get; set; }
            public string EstadoCivil { get; set; }
            public string Genero { get; set; }
            public string Pais { get; set; }
            public string Ciudad { get; set; }
            public string Direccion { get; set; }
            public string Correo { get; set; }
            public string Telefono { get; set; }
            public string Telefono2 { get; set; }
            public string OtroContacto { get; set; }
            public string RTN { get; set; }
            public string Observacion { get; set; }
            public string CodigoPersona { get; set; }
            public EstadoPersona Estado { get; set; }
            public string RecomendadoPor { get; set; }
            public int? CantidadPrestamos { get; set; }
            public string Nota { get; set; }
            public bool PrestamoActivo { get; set; }
            public bool ClienteBueno { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool Enabled { get; set; }
        }





    }










}

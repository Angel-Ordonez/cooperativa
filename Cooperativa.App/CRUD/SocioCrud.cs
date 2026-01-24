using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Migrations;
using Cooperativa.App.Utilidades;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;

namespace Cooperativa.App.CRUD
{
    public class SocioCrud
    {

        public class Crear
        {
            public class CommandCreateSocio : IRequest<AppResult>
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
                public string Beneficiario_Identificacion { get; set; }
                public string Beneficiario_Nombre { get; set; }
                public string Parentesco { get; set; }
                // public Guid CreatedBy { get; set; }

                public decimal PorcentajeGanancia { get; set; }
            }

            public class CommandValidator : AbstractValidator<CommandCreateSocio>
            {
                public CommandValidator()
                {
                    RuleFor(s => s.Nombre).NotEmpty().NotNull().WithMessage("Nombre no debe ser Vacio");
                    RuleFor(s => s.Apellido).NotEmpty().NotNull().WithMessage("Apellido no debe ser Vacio");
                    RuleFor(s => s.TipoIdentificacion).NotEmpty().NotNull().WithMessage("Tipo Identificacion no debe ser Vacio");
                    RuleFor(s => s.Identificacion).NotEmpty().NotNull().WithMessage("Identificacion no debe ser Vacio");
                    RuleFor(s => s.LugarTrabajo).NotEmpty().NotNull().WithMessage("LugarTrabajo no debe ser Vacio");
                    RuleFor(s => s.Ocupacion).NotEmpty().NotNull().WithMessage("Ocupacion no debe ser Vacio");
                    RuleFor(s => s.Direccion).NotEmpty().NotNull().WithMessage("Direccion no debe ser Vacio");
                    RuleFor(s => s.Telefono).NotEmpty().NotNull().WithMessage("Telefono no debe ser Vacio");
                    RuleFor(s => s.FechaNacimiento).NotEmpty().NotNull().WithMessage("FechaDeNacimiento no debe ser Vacio");
                    RuleFor(s => s.FechaDeIngreso).NotEmpty().NotNull().WithMessage("FechaDeIngreso no debe ser Vacio");
                }
            }


            public class CommandHandlerSocio : IRequestHandler<CommandCreateSocio, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandlerSocio(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase; 
                }

                public async Task<AppResult> Handle(CommandCreateSocio command, CancellationToken cancellationToken)
                {
                    //Solo me interesa saber si existe, con FirstOrDefaultAsync traera toda la data... entonces de esta forma paso de 1.3 ms a 0.6 ms
                    var exists = await _context.Socio
                        .AnyAsync(x => x.Identificacion == command.Identificacion && !x.IsSoftDeleted && x.Enabled);

                    if (exists)
                        return AppResult.New(false, $"Ya existe un socio con identidad {command.Identificacion}");


                    // ============================================
                    // GENERACIÓN DEL CÓDIGO DE SOCIO
                    // ============================================

                    if (string.IsNullOrWhiteSpace(command.Nombre) || string.IsNullOrWhiteSpace(command.Apellido))
                        return AppResult.New(false, "Nombre y apellido no pueden estar vacíos.");

                    var nombre = command.Nombre.Trim();
                    var apellido = command.Apellido.Trim();

                    var letraNombre = char.ToUpper(nombre[0]);
                    var letraApellido = char.ToUpper(apellido[0]);
                    var anio = DateTime.Now.Year;
                    var codigoEmpresa = "COOPAZ";

                    var codigoBaseSecuencial = $"SC-{codigoEmpresa}-{anio}";
                    var codigoBase = $"SC-{codigoEmpresa}-{letraNombre}{letraApellido}-{anio}";
                    var secuencial = await _iUtilidadesBase.GenerarSecuencial("Socio", codigoBaseSecuencial);

                    if (secuencial <= 0)
                        return AppResult.New(false, "Error al generar secuencial de socio.");

                    var codigoPersona = $"{codigoBase}-{secuencial:D4}";


                    // ============================================
                    // TRANSACCIÓN BD
                    // ============================================

                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                    try
                    {
                        var createdBy = Guid.Parse("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        // CREAR SOCIO
                        var socio = Socio.New(command.Nombre,
                            command.Apellido, command.TipoIdentificacion, command.Identificacion, command.FechaNacimiento,
                            command.EstadoCivil, command.Genero, command.LugarTrabajo, command.Ocupacion,
                            command.Observacion, command.Pais, command.Ciudad, command.Direccion, command.Correo,
                            command.Telefono, command.Telefono2, command.OtroContacto, command.RTN,
                            command.Beneficiario_Identificacion, command.Beneficiario_Nombre, command.Parentesco,
                            command.FechaDeIngreso, codigoPersona, command.PorcentajeGanancia, createdBy
                        );

                        await _context.Socio.AddAsync(socio);

                        // CREAR CUENTA BANCARIA DEFECTO
                        var cuenta = CuentaBancaria.New(socio.Id, null, "Efectivo", "Efectivo", TipoCuentaBancaria.Otro, createdBy);

                        await _context.CuentaBancaria.AddAsync(cuenta);

                        await _context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        return AppResult.New(true, $"Socio creado correctamente: {codigoPersona}");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }




        public class CreateSocio
        {
            public class CommandCreateSocio : IRequest<AppResult>
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
                public string Beneficiario_Identificacion { get; set; }
                public string Beneficiario_Nombre { get; set; }
                public string Parentesco { get; set; }
                // public Guid CreatedBy { get; set; }

                public decimal CantidadInversion { get; set; }
                public decimal PorcentajeGanancia { get; set; }


            }

            public class CommandValidator : AbstractValidator<CommandCreateSocio>
            {
                public CommandValidator()
                {
                    RuleFor(s => s.Nombre).NotEmpty().NotNull().WithMessage("Nombre no debe ser Vacio");
                    RuleFor(s => s.Apellido).NotEmpty().NotNull().WithMessage("Apellido no debe ser Vacio");
                    RuleFor(s => s.TipoIdentificacion).NotEmpty().NotNull().WithMessage("Tipo Identificacion no debe ser Vacio");
                    RuleFor(s => s.Identificacion).NotEmpty().NotNull().WithMessage("Identificacion no debe ser Vacio");
                    RuleFor(s => s.LugarTrabajo).NotEmpty().NotNull().WithMessage("LugarTrabajo no debe ser Vacio");
                    RuleFor(s => s.Ocupacion).NotEmpty().NotNull().WithMessage("Ocupacion no debe ser Vacio");
                    RuleFor(s => s.Direccion).NotEmpty().NotNull().WithMessage("Direccion no debe ser Vacio");
                    RuleFor(s => s.Telefono).NotEmpty().NotNull().WithMessage("Telefono no debe ser Vacio");
                    RuleFor(s => s.FechaNacimiento).NotEmpty().NotNull().WithMessage("FechaDeNacimiento no debe ser Vacio");
                    RuleFor(s => s.FechaDeIngreso).NotEmpty().NotNull().WithMessage("FechaDeIngreso no debe ser Vacio");
                    RuleFor(s => s.CantidadInversion).NotEmpty().NotNull().WithMessage("Cantidad a Invertir no debe der null ni cero!");
                }
            }


            public class CommandHandlerSocio : IRequestHandler<CommandCreateSocio, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCreateSocio command, CancellationToken cancellationToken)
                {
                    var socio = await _context.Socio.Where(x => x.Identificacion == command.Identificacion && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                    if (socio != null)
                    {
                        return AppResult.New(false, $"Ya existe Socio con ese Numero de Identidad. Su nombre es: {socio.Nombre + " " + socio.Apellido}");
                    }

                    if (command.PorcentajeGanancia > 0 && command.PorcentajeGanancia < 1)
                    {
                        throw new ArgumentException("El porcentaje de ganancia debe estar entre 1 y 100.");
                    }

                    //La caja sera luego por empresaId
                    var caja = await _context.Caja.Where(x => !x.IsSoftDeleted == x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                    caja.ThrowIfNull("No se encontro Caja para Empresa");

                    var ultimoSocio = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();

                    #region CodigoSocio   SOC-AO-23-1

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
                        var anio = DateTime.Now.Year % 100; // Obtener los dos últimos dígitos del año actual

                        codigoPersona = "SOC-" + primeRLetraNombre + primerLetraAppellido + "-" + anio + "-" + (ultimoSocio.Count() + 1).ToString();

                    }
                    else
                    {
                        return AppResult.New(false, $"Hubo error al crear el CodigoPersona, Porfavor revise su Nombre y Apellido.");
                    }

                    #endregion

                    var transaction = _context.Database.BeginTransaction();
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var newSocio = Socio.New(command.Nombre, command.Apellido, command.TipoIdentificacion, command.Identificacion, command.FechaNacimiento, command.EstadoCivil, command.Genero,
                                command.LugarTrabajo, command.Ocupacion, command.Observacion, command.Pais, command.Ciudad, command.Direccion, command.Correo,
                                command.Telefono, command.Telefono2, command.OtroContacto, command.RTN, command.Beneficiario_Identificacion, command.Beneficiario_Nombre, command.Parentesco, command.FechaDeIngreso, codigoPersona, command.PorcentajeGanancia, createdBy);



                        await _context.Socio.AddAsync(newSocio);




                        #region SocioInversion
                        var inversionesDelSocio = await _context.SocioInversion.Where(x => x.SocioId == newSocio.Id && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                        var ingresosTotales = await _context.Ingreso.AsNoTracking().ToListAsync();
                        var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();

                        #region CodigoInversion   INV-AO-1

                        var codigoInversion = "";
                        var codigoIngreso = "";
                        int numeroInversion = inversionesDelSocio.Count() + 1;
                        int numeroIngreso = ingresosTotales.Count() + 1;

                        if (!string.IsNullOrEmpty(newSocio.Nombre) && !string.IsNullOrEmpty(newSocio.Apellido))
                        {
                            var primeRLetraNombre = char.ToUpper(newSocio.Nombre[0]);
                            var primerLetraApellido = char.ToUpper(newSocio.Apellido[0]);
                            var anio = DateTime.Now.Year % 100;

                            string[] partes = newSocio.CodigoPersona.Split('-');
                            string numSocio = partes[partes.Length - 1];

                            codigoInversion = "INV-" + anio + "-" + primeRLetraNombre + primerLetraApellido + numSocio + "-" + numeroInversion;
                            //codigoIngreso = "I-" + primeRLetraNombre + primerLetraApellido + "-" + anio + "-" + numeroIngreso;
                            codigoIngreso = codigoInversion;

                        }
                        else
                        {
                            return AppResult.New(false, $"Hubo error al crear el CodigoInversion, Porfavor revise su Nombre y Apellido.");
                        }

                        #endregion

                        string descripcionInversion = $"Primer inversion de nuevo Socio: {newSocio.CodigoPersona}";
                        var newSocioInversion = SocioInversion.New(codigoInversion, command.FechaDeIngreso, command.CantidadInversion, "HNL", descripcionInversion, newSocio.Id, newSocio.Nombre + " " + newSocio.Apellido, createdBy);
                        await _context.SocioInversion.AddAsync(newSocioInversion);

                        newSocio.FechaUltimaInversion = DateTime.Now;
                        newSocio.CantidadInvensiones = 1;
                        newSocio.TotalMontoInvertido = newSocioInversion.Cantidad;

                        var newIngreso = Ingreso.NewPorSocioInversion(numeroInversion, codigoInversion, command.CantidadInversion, "Inversion Socio", descripcionInversion, newSocioInversion.Id, createdBy);
                        await _context.Ingreso.AddAsync(newIngreso);

                        var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, codigoIngreso, command.CantidadInversion, null, createdBy);
                        newTransaccion.IngresoId = newIngreso.Id;
                        newTransaccion.CajaId = caja.Id;
                        await _context.Transaccion.AddAsync(newTransaccion);
                        //await _context.SaveChangesAsync();


                        #region Proceso viejo caja
                        //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                        //if (ultimoRegistroCaja != null)
                        //{
                        //    var nuevoSaldo = ultimoRegistroCaja.SaldoActual + newIngreso.Monto;

                        //    var agregarACaja = Caja.NewIngreso(createdBy, codigoIngreso, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newIngreso.Id, createdBy);

                        //    ultimoRegistroCaja.Enabled = false;
                        //    ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                        //    ultimoRegistroCaja.ModifiedBy = createdBy;

                        //    await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();
                        //    transaction.Commit();
                        //    //return AppResult.New(true, $"Socio {newSocio.CodigoPersona}.");
                        //}
                        //else
                        //{
                        //    var nuevoSaldo = newIngreso.Monto;

                        //    var agregarACaja = Caja.NewIngreso(createdBy, codigoIngreso, 0, nuevoSaldo, newIngreso.Id, createdBy);

                        //    await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();
                        //    transaction.Commit();
                        //    //return AppResult.New(true, $"Socio {newSocio.CodigoPersona}.");
                        //}

                        #endregion

                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual + newIngreso.Monto;
                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, createdBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                        await _context.SaveChangesAsync();
                        transaction.Commit();





                        try
                        {
                            //Crear cuenta efectivo
                            var newCuentaBancaria = CuentaBancaria.New(newSocio.Id, null, "Efectivo", "Efectivo", TipoCuentaBancaria.Otro, createdBy);

                            await _context.CuentaBancaria.AddAsync(newCuentaBancaria);
                            await _context.SaveChangesAsync();
                        }
                        catch(Exception ex)
                        {
                            throw new Exception("Error al crear Cuenta Bancaria"); 
                        }



                        return AppResult.New(true, $"Socio {newSocio.CodigoPersona}.");
                        #endregion


                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return AppResult.New(false, e.Message);
                    }

                }
            }
        }



        public class UpdateSocio
        {
            public class CommandUpdateSocio : IRequest<AppResult>
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
                public string Beneficiario_Identificacion { get; set; }
                public string Beneficiario_Nombre { get; set; }
                public string Parentesco { get; set; }
                public decimal PorcentajeGanancia { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHandlerSocio : IRequestHandler<CommandUpdateSocio, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandUpdateSocio command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var socio = await _context.Socio.Where(x => x.Id == command.Id && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (socio == null) { throw new Exception("Socio no existe"); }

                        var queSeActualizo = "";
                        if (command.Nombre != null && command.Nombre.Length > 1 && command.Nombre != socio.Nombre)
                        {
                            socio.Nombre = command.Nombre;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Nombre"; } else { queSeActualizo += "Nombre"; }
                        }
                        if (command.Apellido != null && command.Apellido.Length > 1 && command.Apellido != socio.Apellido)
                        {
                            socio.Apellido = command.Apellido;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Apellido"; } else { queSeActualizo += "Apellido"; }
                        }
                        if (command.FechaNacimiento != null && command.FechaNacimiento != DateTime.MinValue && command.FechaNacimiento != DateTime.MaxValue && command.FechaNacimiento != socio.FechaNacimiento)
                        {
                            socio.FechaNacimiento = (DateTime)command.FechaNacimiento;
                            socio.Edad = DateTime.Now.Year - command.FechaNacimiento.Value.Year;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", FechaNacimiento"; } else { queSeActualizo += "FechaNacimiento"; }
                        }
                        if (command.FechaDeIngreso != null && command.FechaDeIngreso != DateTime.MinValue && command.FechaDeIngreso != DateTime.MaxValue && command.FechaDeIngreso != socio.CreatedDate)
                        {
                            socio.CreatedDate = (DateTime)command.FechaDeIngreso;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", FechaDeIngreso"; } else { queSeActualizo += "FechaDeIngreso"; }
                        }
                        if (command.TipoIdentificacion != null && command.TipoIdentificacion.Length > 1 && command.TipoIdentificacion != socio.TipoIdentificacion)
                        {
                            socio.TipoIdentificacion = command.TipoIdentificacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", TipoIdentificacion"; } else { queSeActualizo += "TipoIdentificacion"; }
                        }
                        if (command.Identificacion != null && command.Identificacion.Length > 1 && command.Identificacion != socio.Identificacion)
                        {
                            socio.Identificacion = command.Identificacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Identificación"; } else { queSeActualizo += "Identificación"; }
                        }
                        if (command.LugarTrabajo != null && command.LugarTrabajo.Length > 1 && command.LugarTrabajo != socio.LugarTrabajo)
                        {
                            socio.LugarTrabajo = command.LugarTrabajo;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", LugarTrabajo"; } else { queSeActualizo += "LugarTrabajo"; }
                        }
                        if (command.Ocupacion != null && command.Ocupacion.Length > 1 && command.Ocupacion != socio.Ocupacion)
                        {
                            socio.Ocupacion = command.Ocupacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Ocupación"; } else { queSeActualizo += "Ocupación"; }
                        }
                        if (command.EstadoCivil != null && command.EstadoCivil.Length > 1 && command.EstadoCivil != socio.EstadoCivil)
                        {
                            socio.EstadoCivil = command.EstadoCivil;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", EstadoCivil"; } else { queSeActualizo += "Estado Civil"; }
                        }
                        if (command.Genero != null && command.Genero.Length > 1 && command.Genero != socio.Genero)
                        {
                            socio.Genero = command.Genero;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Genero"; } else { queSeActualizo += "Genero"; }
                        }
                        if (command.Pais != null && command.Pais.Length > 1 && command.Pais != socio.Pais)
                        {
                            socio.Pais = command.Pais;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Pais"; } else { queSeActualizo += "Pais"; }
                        }
                        if (command.Ciudad != null && command.Ciudad.Length > 1 && command.Ciudad != socio.Ciudad)
                        {
                            socio.Ciudad = command.Ciudad;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Ciudad"; } else { queSeActualizo += "Ciudad"; }
                        }
                        if (command.Direccion != null && command.Direccion.Length > 1 && command.Direccion != socio.Direccion)
                        {
                            socio.Direccion = command.Direccion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Dirección"; } else { queSeActualizo += "Dirección"; }
                        }
                        if (command.Correo != null && command.Correo.Length > 1 && command.Correo != socio.Correo)
                        {
                            socio.Correo = command.Correo;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Correo"; } else { queSeActualizo += "Correo electronico"; }
                        }
                        if (command.Telefono != null && command.Telefono.Length > 1 && command.Telefono != socio.Telefono)
                        {
                            socio.Telefono = command.Telefono;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Telefono"; } else { queSeActualizo += "Telefono"; }
                        }
                        if (command.Telefono2 != null && command.Telefono2.Length > 1 && command.Telefono2 != socio.Telefono2)
                        {
                            socio.Telefono2 = command.Telefono2;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Telefono2"; } else { queSeActualizo += "Telefono2"; }
                        }
                        if (command.OtroContacto != null && command.OtroContacto.Length > 1 && command.OtroContacto != socio.OtroContacto)
                        {
                            socio.OtroContacto = command.OtroContacto;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Otro Contacto"; } else { queSeActualizo += "Otro contacto"; }
                        }
                        if (command.RTN != null && command.RTN.Length > 1 && command.RTN != socio.RTN)
                        {
                            socio.RTN = command.RTN;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", RTN"; } else { queSeActualizo += "RTN"; }
                        }
                        if (command.Observacion != null && command.Observacion.Length > 1 && command.Observacion != socio.Observacion)
                        {
                            socio.Observacion = command.Observacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Observación"; } else { queSeActualizo += "Observación"; }
                        }
                        if (command.Beneficiario_Identificacion != null && command.Beneficiario_Identificacion.Length > 1 && command.Beneficiario_Identificacion != socio.Beneficiario_Identificacion)
                        {
                            socio.Beneficiario_Identificacion = command.Beneficiario_Identificacion;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Beneficiario_Identificacion"; } else { queSeActualizo += "Beneficiario_Identificacion"; }
                        }
                        if (command.Beneficiario_Nombre != null && command.Beneficiario_Nombre.Length > 1 && command.Beneficiario_Nombre != socio.Beneficiario_Nombre)
                        {
                            socio.Beneficiario_Nombre = command.Beneficiario_Nombre;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Beneficiario_Nombre"; } else { queSeActualizo += "Beneficiario_Nombre"; }
                        }
                        if (command.Parentesco != null && command.Parentesco.Length > 1 && command.Parentesco != socio.Parentesco)
                        {
                            socio.Parentesco = command.Parentesco;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Parentesco"; } else { queSeActualizo += "Parentesco"; }
                        }
                        if (command.PorcentajeGanancia > 0 && command.PorcentajeGanancia != socio.PorcentajeGanancia)
                        {
                            if(command.PorcentajeGanancia > 0 && command.PorcentajeGanancia < 1)
                            {
                                throw new ArgumentException("El porcentaje de ganancia debe estar entre 1 y 100.");
                            }

                            socio.PorcentajeGanancia = command.PorcentajeGanancia;
                            if (queSeActualizo.Length > 1) { queSeActualizo += ", Porcentaje Ganancia"; } else { queSeActualizo += "Porcentaje Ganancia"; }
                        }


                        var modificadoPor = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        if (queSeActualizo.Length > 1)
                        {
                            if (command.ModifiedBy == null || command.ModifiedBy == Guid.Empty)
                            {
                                command.ModifiedBy = modificadoPor;
                            }

                            socio.ModifiedBy = (Guid)command.ModifiedBy;
                            socio.ModifiedDate = DateTime.Now;

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




        public class HabilitarSocio
        {
            public class Commandhs : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public bool Enabled { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<Commandhs, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Commandhs query, CancellationToken cancellationToken)
                {
                    try
                    {
                        var persona = await _context.Socio.Where(x => x.Id == query.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        if (persona == null)
                        {
                            throw new Exception("Socio no existe");
                        }

                        var inversionesActivos = await _context.SocioInversion.Where(x => x.SocioId == query.Id && !x.IsSoftDeleted && ((int)x.Estado) == 1).ToListAsync();
                        if (inversionesActivos.Any() && query.Enabled == false)
                        {
                            throw new Exception($"Socio tiene {inversionesActivos.Count()} Inversiones activas");
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

                        return AppResult.New(true, "Socio " + mensaje);
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }



        public class EliminarSocio
        {
            public class Commandes : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public Guid? ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<Commandes, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(Commandes query, CancellationToken cancellationToken)
                {
                    try
                    {
                        var persona = await _context.Socio.Where(x => x.Id == query.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        if (persona == null)
                        {
                            throw new Exception("Socio no existe");
                        }


                        var inversionesActivos = await _context.SocioInversion.Where(x => x.SocioId == query.Id && !x.IsSoftDeleted && ((int)x.Estado) == 1).ToListAsync();
                        if (inversionesActivos.Any())
                        {
                            throw new Exception($"Socio tiene {inversionesActivos.Count()} Inversiones activas");
                        }


                        persona.IsSoftDeleted = true;
                        persona.ModifiedDate = DateTime.Now;
                        if (query.ModifiedBy != null && query.ModifiedBy != Guid.Empty)
                        {
                            persona.ModifiedBy = (Guid)query.ModifiedBy;
                        }


                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Socio eliminado con exito!");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }








        public class IndexSocio
        {
            public class CommandIndexSocio : IRequest<List<SocioVm>>
            {

            }
            public class CommandHandlerSocio : IRequestHandler<CommandIndexSocio, List<SocioVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<SocioVm>> Handle(CommandIndexSocio command, CancellationToken cancellationToken)
                {

                    var socios = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .Include(x => x.SocioInversiones)
                        .ProjectToType<SocioVm>()
                        .ToListAsync();

                    return socios;
                }
            }
        }





        public class IndexV2
        {
            public class SocioVmV2
            {
                public Guid Id { get; set; }
                public string Nombre { get; set; }
                public string Apellido { get; set; }
                public string CodigoPersona { get; set; }
            }
            public class CommandIndexSocio : IRequest<List<SocioVmV2>>
            {

            }
            public class CommandHandlerSocio : IRequestHandler<CommandIndexSocio, List<SocioVmV2>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<SocioVmV2>> Handle(CommandIndexSocio command, CancellationToken cancellationToken)
                {
                    var socios = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .ProjectToType<SocioVmV2>()
                        .ToListAsync();

                    return socios;
                }
            }
        }







        public class GetSocioAndCuentasBancarias
        {
            public class SocioRes
            {
                public Guid Id { get; set; }
                public string Nombre { get; set; }
                public string Apellido { get; set; }
                public string CodigoPersona { get; set; }
                public List<CuentaBancariaRes> CuentaBancarias { get; set; }
            }
            public class CuentaBancariaRes
            {
                public Guid Id { get; set; }
                public string CuentaBancariaDescripcion { get; set; }
            }
            public class CommandGSCB : IRequest<List<SocioRes>>
            {

            }
            public class CommandHandlerSocio : IRequestHandler<CommandGSCB, List<SocioRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<SocioRes>> Handle(CommandGSCB command, CancellationToken cancellationToken)
                {
                    var socios = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        //.Include(x => x.CuentaBancarias)
                        //.ProjectToType<SocioRes>()
                        .ToListAsync();

                    var sociosIds = socios.Select(x => x.Id).ToList();

                    var cuentasBancariasAll = await _context.CuentaBancaria.Where(x => sociosIds.Contains((Guid)x.PersonaId) && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();


                    var sociosRes = socios.Adapt<List<SocioRes>>();

                    sociosRes.ForEach(x =>
                    {
                        var cuentasBancarias = cuentasBancariasAll.Where(z => z.PersonaId == x.Id).ToList();
                        x.CuentaBancarias = new List<CuentaBancariaRes>();

                        cuentasBancarias.ForEach(c =>
                        {
                            var cuentaCocatenado = "";
                            if(c.InstitucionBancaria != null)
                            {
                                cuentaCocatenado = c.InstitucionBancaria.Nombre + " - " + c.NumeroCuenta;
                            }
                            else
                            {
                                cuentaCocatenado = c.NumeroCuenta;
                            }

                            var newCuenta = new CuentaBancariaRes
                            {
                                Id = c.Id,
                                CuentaBancariaDescripcion = cuentaCocatenado,
                            };

                            x.CuentaBancarias.Add(newCuenta);
                        });
                    });

                    return sociosRes;
                }
            }
        }







        public class GetSocioById
        {
            public class QueryGetSocio : IRequest<SocioVm>
            {
                public Guid Id { get; set; }
            }

            public class CommandHandlerSocio : IRequestHandler<QueryGetSocio, SocioVm>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<SocioVm> Handle(QueryGetSocio command, CancellationToken cancellationToken)
                {
                    var socio = await _context.Socio.Where(x => x.Id == command.Id && !x.IsSoftDeleted && x.Enabled)
                        //.Include(x => x.SocioInversiones)
                        //.AsNoTracking()
                        //.ProjectToType<SocioVm>()
                        .FirstOrDefaultAsync();
                    if(socio == null) { throw new Exception("Socio no existe"); }


                    await _context.SocioInversion.Where(x => x.SocioId == socio.Id && !x.IsSoftDeleted && x.Enabled).ToListAsync();
                    await _context.CuentaBancaria.Where(x => x.PersonaId == socio.Id && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();

                    var socioVm = socio.Adapt<SocioVm>();
                    socioVm.CantidadInversion = socioVm.TotalMontoInvertido;

                    return socioVm;
                }
            }
        }












        public class SocioVm
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime FechaNacimiento { get; set; }
            public string Identidad { get; set; }
            public string Genero { get; set; }
            public string TipoIdentificacion { get; set; }
            public string Identificacion { get; set; }
            public string LugarTrabajo { get; set; }
            public string Ocupacion { get; set; }
            public int Edad { get; set; }
            public string EstadoCivil { get; set; }
            public string Pais { get; set; }
            public string Region { get; set; }
            public string Ciudad { get; set; }
            public string Direccion { get; set; }
            public string Correo { get; set; }
            public string Telefono { get; set; }
            public string Telefono2 { get; set; }
            public string OtroContacto { get; set; }
            public string RTN { get; set; }
            public string Observacion { get; set; }
            public bool TieneUsuario { get; set; }
            public string CodigoPersona { get; set; }
            public DateTime FechaDeIngreso { get; set; }
            public EstadoPersona Estado { get; set; }
            public DateTime FechaUltimaInversion { get; set; }
            public decimal? TotalMontoInvertido { get; set; }
            public int? CantidadInvensiones { get; set; }
            public string Beneficiario_Identificacion { get; set; }
            public string Beneficiario_Nombre { get; set; }
            public string Parentesco { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool Enabled { get; set; }
            public List<SocioInversionRes> SocioInversiones { get; set; }
            public decimal? CantidadInversion { get; set; }
            public decimal? PorcentajeGanancia { get; set; }
            public decimal? GananciaTotal { get; set; }
            public decimal? TotalRetirado { get; set; }
            public decimal? SaldoDisponibleARetirar { get; set; }
            public int CantidadRetiraros { get; set; }
            public List<CuentaBancariaVm> CuentaBancarias { get; set; }
        }

        public class SocioInversionRes
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CodigoInversion { get; set; }
            public DateTime FechaIngreso { get; set; }
            public decimal Cantidad { get; set; }
            public decimal CantidadActiva { get; set; }
            public string Moneda_Descripcion { get; set; }
            public string Descripcion { get; set; }
            public EstadoInversion Estado { get; set; }
            public Guid SocioId { get; set; }
            public string SocioNombre { get; set; }
            public decimal Ganancia { get; set; }
            public decimal GananciaDisponible { get; set; }
            public decimal CantidadDisponibleRetirar { get; set; }      //NoPrestado + GananciaDisponible
            public decimal Retirado { get; set; }
        }



    }






}

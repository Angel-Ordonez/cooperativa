using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Utilidades;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.SocioCrud;
using static Cooperativa.App.Domain.Model.Socios.GananciaDetalleSocio;

namespace Cooperativa.App.CRUD
{
    public  class SocioInversionCrud
    {


        public class CreateSocioInversion
        {
            public class CommandCreateSocioInversion : IRequest<AppResult>
            {
                public Guid SocioId { get; set; }
                public DateTime FechaIngreso { get; set; }
                public decimal Cantidad { get; set; }
                public string Descripcion { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                //public Guid CreatedBy { get; set; }
            }

            public class CommandHandlerSocioInversion : IRequestHandler<CommandCreateSocioInversion, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocioInversion(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCreateSocioInversion command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var socio = await _context.Socio.Where(x => x.Id == command.SocioId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();

                        var inversionesDelSocio = await _context.SocioInversion.Where(x => x.SocioId == command.SocioId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                        var ingresosTotales = await _context.Ingreso.AsNoTracking().ToListAsync();
                        var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();


                        var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaDestinoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cuentaDestino.ThrowIfNull($"No se encontro cuenta destino");
                        var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaOrigenId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cuentaOrigen.ThrowIfNull($"No se encontro cuenta Origen");


                        #region CodigoInversion   INV-AO-1

                        var codigoInversion = "";
                        var codigoIngreso = "";
                        int numeroInversion = inversionesDelSocio.Count() + 1;
                        int numeroIngreso = ingresosTotales.Count() + 1;

                        if (!string.IsNullOrEmpty(socio.Nombre) && !string.IsNullOrEmpty(socio.Apellido))
                        {
                            var primeRLetraNombre = char.ToUpper(socio.Nombre[0]);
                            var primerLetraApellido = char.ToUpper(socio.Apellido[0]);
                            var anio = DateTime.Now.Year % 100;

                            string[] partes = socio.CodigoPersona.Split('-');
                            string numSocio = partes[partes.Length - 1];

                            codigoInversion = "INV-" + anio + "-" + primeRLetraNombre + primerLetraApellido + numSocio + "-" + numeroInversion;
                            //codigoIngreso = "I-" + primeRLetraNombre + primerLetraApellido + "-" + anio + "-" + numeroIngreso;
                            codigoIngreso = codigoInversion + "-I" + numeroIngreso;
                        }
                        else
                        {
                            return AppResult.New(false, $"Hubo error al crear el CodigoInversion, Porfavor revise su Nombre y Apellido.");
                        }

                        #endregion



                        //La caja sera luego por empresaId
                        var caja = await _context.Caja.Where(x => !x.IsSoftDeleted == x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                        caja.ThrowIfNull("No se encontro Caja para Empresa");


                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var newSocioInversion = SocioInversion.New(codigoInversion, command.FechaIngreso, command.Cantidad, "HNL", command.Descripcion, socio.Id, socio.Nombre + " " + socio.Apellido, createdBy);
                        await _context.SocioInversion.AddAsync(newSocioInversion);

                        socio.FechaUltimaInversion = DateTime.Now;
                        socio.CantidadInvensiones = (socio.CantidadInvensiones != null) ? (socio.CantidadInvensiones + 1) : 1;
                        socio.TotalMontoInvertido = (socio.TotalMontoInvertido != null) ? (socio.TotalMontoInvertido + newSocioInversion.Cantidad) : newSocioInversion.Cantidad;

                        var newIngreso = Ingreso.NewPorSocioInversion(numeroInversion, codigoInversion, command.Cantidad, "Inversion Socio", command.Descripcion, newSocioInversion.Id, createdBy);
                        await _context.Ingreso.AddAsync(newIngreso);

                        var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, codigoIngreso, command.Cantidad, cuentaOrigen.Id, cuentaDestino.Id, createdBy);
                        newTransaccion.IngresoId = newIngreso.Id;

                        await _context.Transaccion.AddAsync(newTransaccion);
                        //await _context.SaveChangesAsync();

                        #region Proceso viejo caja
                        //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                        //if(ultimoRegistroCaja != null)
                        //{
                        //    var nuevoSaldo = ultimoRegistroCaja.SaldoActual + newIngreso.Monto;

                        //    var agregarACaja = Caja.NewIngreso(createdBy, codigoIngreso, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newIngreso.Id, createdBy);

                        //    ultimoRegistroCaja.Enabled = false;
                        //    ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                        //    ultimoRegistroCaja.ModifiedBy = createdBy;

                        //    await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();
                        //}
                        //else
                        //{
                        //    var nuevoSaldo = newIngreso.Monto;

                        //    var agregarACaja = Caja.NewIngreso(createdBy, codigoIngreso, 0, nuevoSaldo, newIngreso.Id, createdBy);

                        //    await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();
                        //}
                        #endregion


                        cuentaOrigen.SumarMovimiento(command.Cantidad);             //Socio
                        cuentaDestino.RestarMovimiento(command.Cantidad);           //Caja


                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual + newIngreso.Monto;
                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, createdBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                        await _context.SaveChangesAsync();


                        return AppResult.New(true, $"Se registro Inversion de: {newSocioInversion.SocioNombre} con CodigoInversion: {newSocioInversion.CodigoInversion}.");

                    }
                    catch (Exception e)
                    {
                        return AppResult.New(false, e.Message);

                    }

                }
            }
        }



        public class Crear
        {
            public class CommandCreateSocioInversion : IRequest<AppResult>
            {
                public Guid SocioId { get; set; }
                public DateTime FechaIngreso { get; set; }
                public decimal Cantidad { get; set; }
                public string Descripcion { get; set; }
                public Guid? CajaId { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                public string Referencia { get; set; }
                //public Guid CreatedBy { get; set; }
            }

            public class CommandHandlerSocioInversion : IRequestHandler<CommandCreateSocioInversion, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _utilidadesBase;
                public CommandHandlerSocioInversion(CooperativaDbContext context, IUtilidadesBase utilidadesBase)
                {
                    _context = context;
                    _utilidadesBase = utilidadesBase;
                }

                public async Task<AppResult> Handle(CommandCreateSocioInversion command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var socio = await _context.Socio.Where(x => x.Id == command.SocioId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();

                        var inversionesDelSocio = await _context.SocioInversion.Where(x => x.SocioId == command.SocioId && !x.IsSoftDeleted && x.Enabled).CountAsync();
                        var ingresosTotales = await _context.Ingreso.AsNoTracking().ToListAsync();
                        var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();


                        var caja = await _context.Caja.Where(x => x.Id == command.CajaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        caja.ThrowIfNull($"No existe Caja solicitada");

                        var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaOrigenId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cuentaOrigen.ThrowIfNull($"No se encontro cuenta Origen");

                        var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaDestinoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cuentaDestino.ThrowIfNull($"No se encontro cuenta destino");

                        if(cuentaDestino.CajaId != caja.Id)
                        {
                            throw new Exception("Cuenta Destino no pertenece a Caja");
                        }

                        if (cuentaOrigen.NumeroCuenta.ToLower().Contains("efectivo") && !cuentaDestino.NumeroCuenta.ToLower().Contains("efectivo"))
                        {
                            throw new Exception("La inversión no puede realizarse: la cuenta de origen es de efectivo y la de destino es bancaria. Por favor, verifique las cuentas seleccionadas.");
                        }
                        if (cuentaDestino.NumeroCuenta.ToLower().Contains("efectivo") && !cuentaOrigen.NumeroCuenta.ToLower().Contains("efectivo"))
                        {
                            throw new Exception("La inversión no puede realizarse: la cuenta de destino es de efectivo y la de origen es bancaria. Por favor, verifique las cuentas seleccionadas.");
                        }
                        if (!cuentaOrigen.NumeroCuenta.ToLower().Contains("efectivo") && (command.Referencia == null || command.Referencia.Length < 1))
                        {
                            throw new Exception("Referencia bancaria es obligatoria");
                        }


                        #region CodigoInversion

                        var codigoBase = socio.CodigoPersona + "-I";
                        var secuencial = await _utilidadesBase.GenerarSecuencial("SocioInversion", codigoBase);
                        var anio = DateTime.Now.Year % 100;
                        var codigoInversion = $"{codigoBase}{anio}{secuencial:D3}";

                        #endregion


                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var newSocioInversion = SocioInversion.New(codigoInversion, command.FechaIngreso, command.Cantidad, "HNL", command.Descripcion, socio.Id, socio.Nombre + " " + socio.Apellido, createdBy);
                        await _context.SocioInversion.AddAsync(newSocioInversion);

                        socio.FechaUltimaInversion = DateTime.Now;
                        socio.CantidadInvensiones = (socio.CantidadInvensiones != null) ? (socio.CantidadInvensiones + 1) : 1;
                        socio.TotalMontoInvertido = (socio.TotalMontoInvertido != null) ? (socio.TotalMontoInvertido + newSocioInversion.Cantidad) : newSocioInversion.Cantidad;

                        var newIngreso = Ingreso.NewPorSocioInversion(inversionesDelSocio + 1, codigoInversion, command.Cantidad, "Inversion Socio", command.Descripcion, newSocioInversion.Id, createdBy);
                        await _context.Ingreso.AddAsync(newIngreso);

                        var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, command.Referencia, command.Cantidad, cuentaOrigen.Id, cuentaDestino.Id, createdBy);
                        newTransaccion.IngresoId = newIngreso.Id;
                        newTransaccion.CajaId = caja.Id;

                        await _context.Transaccion.AddAsync(newTransaccion);
                        //await _context.SaveChangesAsync();


                        cuentaOrigen.SumarMovimiento(command.Cantidad);             //Socio
                        cuentaDestino.SumarMovimiento(command.Cantidad);           //Caja


                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual + newIngreso.Monto;
                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, createdBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                        await _context.SaveChangesAsync();


                        return AppResult.New(true, $"Inversion: {newSocioInversion.CodigoInversion} exitosa");

                    }
                    catch (Exception e)
                    {
                        return AppResult.New(false, e.Message);

                    }

                }
            }
        }






        public class Delete
        {
            public class CommandDeleteSocioInversion : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Descripcion { get; set; }
                //public Guid ModifiedBy { get; set; }
            }

            public class CommandHandlerSocioInversion : IRequestHandler<CommandDeleteSocioInversion, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocioInversion(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandDeleteSocioInversion command, CancellationToken cancellationToken)
                {
                    var transaction = _context.Database.BeginTransaction();
                    try
                    {
                        var modifiedBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var inversion = await _context.SocioInversion.Where(x => x.Id == command.Id && !x.IsSoftDeleted && x.Enabled)
                            .Include(x => x.Socio)
                            .FirstOrDefaultAsync();
                        if (inversion == null) { return AppResult.New(false, "No se encontro Inversion de Socio!"); }

                        var ingreso = await _context.Ingreso.Where(x => x.SocioInversionId == inversion.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        var transaccion = await _context.Transaccion.Where(x => x.IngresoId == ingreso.Id && !x.IsSoftDeleted)
                            .Include(x => x.Caja)
                            .Include(x => x.CuentaBancariaOrigen)
                            .Include(x => x.CuentaBancariaDestino)
                            .FirstOrDefaultAsync();
                        transaccion.ThrowIfNull("No se encontro transaccion");

                        var caja = transaccion.Caja;
                        var cuentaOrigen = transaccion.CuentaBancariaOrigen;
                        var cuentaDestino = transaccion.CuentaBancariaDestino;
                        caja.ThrowIfNull("No se encontro Caja");
                        cuentaOrigen.ThrowIfNull("No se encontro CuentaBancariaOrigen");
                        cuentaDestino.ThrowIfNull("No se encontro CuentaBancariaDestino");


                        //var cajaInversion = await _context.Caja.Where(x => x.Ingreso.SocioInversionId == inversion.Id).AsNoTracking().FirstOrDefaultAsync();
                        //if (cajaInversion == null)
                        //{
                        //    inversion.Enabled = false;
                        //    inversion.ModifiedBy = modifiedBy;
                        //    await _context.SaveChangesAsync();
                        //    return AppResult.New(false, $"Esta Inversion no se vio afectada en Caja. Se elimino Inversion {inversion.CodigoInversion}.");
                        //}



                        if (command.Descripcion == null || command.Descripcion.Count() == 0)
                        {
                            command.Descripcion = $"Inversion cancelada {inversion.Socio.Nombre} {inversion.Socio.Apellido} por {inversion.Cantidad.ToString()} {inversion.Moneda_Descripcion}";
                        }


                        var egresosCantidad = await _context.Egreso.ToListAsync();
                        var newEgreso = Egreso.NewPorInversionSocio(egresosCantidad.Count() + 1, inversion.CodigoInversion, inversion.Cantidad, "Inversion Socio Cancelado", command.Descripcion, inversion.Id, modifiedBy);
                        await _context.Egreso.AddAsync(newEgreso);

                        var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();
                        var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, inversion.CodigoInversion, inversion.Cantidad, cuentaOrigen.Id, cuentaDestino.Id, modifiedBy);
                        newTransaccion.EgresoId = newEgreso.Id;
                        newTransaccion.CajaId = caja.Id;

                        cuentaOrigen.RestarMovimiento(inversion.Cantidad);             //Caja
                        cuentaDestino.RestarMovimiento(inversion.Cantidad);           //Socio

                        await _context.Transaccion.AddAsync(newTransaccion);



                        #region Proceso viejo caja

                        //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();


                        //if (ultimoRegistroCaja != null)
                        //{

                        //    if (ultimoRegistroCaja.SaldoActual < inversion.Cantidad)
                        //    {
                        //        return AppResult.New(false, $"No hay Dinero suficiente en Caja. Saldo Actual en Caja: {Math.Round(ultimoRegistroCaja.SaldoActual, 2)} $");
                        //    }

                        //    var nuevoSaldo = ultimoRegistroCaja.SaldoActual - newEgreso.Monto;

                        //    var agregarACaja = Caja.NewEgreso(modifiedBy, inversion.CodigoInversion, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newEgreso.Id, modifiedBy);

                        //    ultimoRegistroCaja.Enabled = false;
                        //    ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                        //    ultimoRegistroCaja.ModifiedBy = modifiedBy;

                        //    inversion.Socio.CantidadInvensiones = inversion.Socio.CantidadInvensiones - 1;
                        //    inversion.Socio.TotalMontoInvertido = inversion.Socio.TotalMontoInvertido - inversion.Cantidad;
                        //    inversion.Enabled = false;
                        //    inversion.ModifiedBy = modifiedBy;

                        //    await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();
                        //    transaction.Commit();

                        //    return AppResult.New(true, $"Inversion {inversion.CodigoInversion} cancelado con exito!");
                        //}
                        //else
                        //{  
                        //    return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                        //}

                        #endregion



                        if (caja.SaldoActual < inversion.Cantidad)
                        {
                            return AppResult.New(false, $"No hay Dinero suficiente en Caja. Saldo Actual en Caja: {Math.Round(caja.SaldoActual, 2)} $");
                        }

                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;

                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, modifiedBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;


                        inversion.Socio.CantidadInvensiones = inversion.Socio.CantidadInvensiones - 1;
                        inversion.Socio.TotalMontoInvertido = inversion.Socio.TotalMontoInvertido - inversion.Cantidad;
                        inversion.Enabled = false;
                        inversion.ModifiedBy = modifiedBy;

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        return AppResult.New(true, $"Inversion {inversion.CodigoInversion} cancelado con exito!");


                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return AppResult.New(false, e.Message);

                    }



                }
            }
        }


        public class CalcularGananciaSocioInversiones
        {
            public class CommandCGSI : IRequest<AppResult>
            {
                public Guid SocioId { get; set; }
                public Guid ModifiedBy { get; set; }
            }

            public class CommandHlHandler : IRequestHandler<CommandCGSI, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHlHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandCGSI cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var socio = await _context.Socio.Where(x => x.Id == cmd.SocioId && !x.IsSoftDeleted)
                            .Include(x => x.SocioInversiones)
                            .FirstOrDefaultAsync();
                        if(socio == null ) { throw new Exception("Socio no existe"); }

                        var inversiones = socio.SocioInversiones.Where(x => x.Enabled && !x.IsSoftDeleted).ToList();

                        var sociosInversionIds = inversiones.Select(x => x.Id).ToList();

                        var detalles = await _context.DetalleSocioInversion.Where(x => sociosInversionIds.Contains(x.SocioInversionId) && !x.IsSoftDeleted && x.Enabled)
                            //.Include(x => x.GananciaDetalleSocio)     //Para que funcione debo hacer las configuraciones en el DbContext
                            .ToListAsync();

                        var detallesSocioInversiones = inversiones.SelectMany(x => x.DetallesSocioInversion).ToList();

                        if (detallesSocioInversiones.Any())
                        {
                            var detallesSocioInversionesIds = detallesSocioInversiones.Select(x => x.Id).ToList();

                            var ganacias = await _context.GananciaDetalleSocio.Where(x => detallesSocioInversionesIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted && x.Enabled)
                                .ToListAsync();


                            decimal gananciaSocio = 0M;
                            //decimal retirosSocio = 0M;      //Esta no la puse, porque seria en el otro modulo retiros mejor

                            foreach (var inversion in inversiones)
                            {
                                var detallesInversiones = inversion.DetallesSocioInversion.Where(s => s.Enabled ).ToList();
                                var detallesIds = detallesInversiones.Select(x => x.Id).Distinct().ToList();

                                var gananciaInversion = ganacias.Where(x => detallesIds.Contains(x.DetalleSocioInversionId)).ToList().Sum(x => x.Ganancia);
                                inversion.Ganancia = gananciaInversion;
                                inversion.GananciaDisponible = gananciaInversion;
                                inversion.CantidadDisponibleRetirar = inversion.GananciaDisponible + inversion.CantidadActiva;
                                inversion.ModifiedBy = cmd.ModifiedBy;
                                inversion.ModifiedDate = DateTime.Now;

                                gananciaSocio += gananciaInversion;
                            }

                            socio.GananciaTotal = gananciaSocio;

                            await _context.SaveChangesAsync();
                            return AppResult.New(true, "Trasaccion exitosa");
                        }
                        else
                        {
                            return AppResult.New(true, "No hay Movimientos a afectuar");
                        }

                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }



        public class GetSocioInversionBySocioId
        {
            public class ResponseVm
            {
                public int CantidadInversiones { get; set; }
                public decimal TotalInvertido { get; set; }
                public List<SocioInversionVm> Inversiones { get; set; }
            }
            public class CommandIndexSocioInversionBySocio : IRequest<ResponseVm>
            {
                public Guid SocioId { get; set; }
            }
            public class CommandHandlerSocioInversionBySocio : IRequestHandler<CommandIndexSocioInversionBySocio, ResponseVm>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocioInversionBySocio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<ResponseVm> Handle(CommandIndexSocioInversionBySocio command, CancellationToken cancellationToken)
                {
                    var sociosInversiones = await _context.SocioInversion.Where(x => x.SocioId == command.SocioId && !x.IsSoftDeleted && x.Enabled /*&& x.DetallesSocioInversion.Any(s => !s.IsSoftDeleted)*/)
                        .AsNoTracking()
                        .ProjectToType<SocioInversionVm>()
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync();

                    var ganaciasIds = sociosInversiones.SelectMany(x => x.DetallesSocioInversion).Where(x => x.GananciaDetalleSocioId != null).Select(s => s.GananciaDetalleSocioId).Distinct().ToList();
                    var gananciasDetallesSocios = await _context.GananciaDetalleSocio.Where(x => ganaciasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .ProjectToType<GananciaDetalleSocioVm>()
                        .ToListAsync();

                    var totalInvertido = 0M;

                    foreach (var i in sociosInversiones)
                    {
                        if (i.Enabled)
                        {
                            totalInvertido += i.Cantidad;
                        }
                    }

                    var prestamosId = sociosInversiones.SelectMany(x => x.DetallesSocioInversion.Select(s => s.PrestamoId)).ToList();
                    var prestamos = await _context.Prestamo.Where(x => prestamosId.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                    foreach(var inv in sociosInversiones)
                    {
                        foreach(var de in inv.DetallesSocioInversion)
                        {
                            var prestamo = prestamos.Where(x => x.Id == de.PrestamoId).FirstOrDefault();
                            de.CodigoPrestamo = (prestamo != null) ? prestamo.CodigoPrestamo : "";

                            if(de.GananciaDetalleSocioId != null && de.GananciaDetalleSocioId != Guid.Empty)
                            {
                                var ganaciaDetalle = gananciasDetallesSocios.Where(x => x.Id == de.GananciaDetalleSocioId).FirstOrDefault();
                                if(ganaciaDetalle != null)
                                {
                                    de.GananciaDetalleSocio = ganaciaDetalle;
                                }
                            }


                        }
                    }


                    var newRes = new ResponseVm
                    {
                        CantidadInversiones = sociosInversiones.Count(),
                        TotalInvertido = totalInvertido,
                        Inversiones = sociosInversiones
                    };


                    return newRes;
                }
            }
        }



        public class GetSocioInversionBySocioIdAndAnio
        {
            public class ResponseVm
            {
                public int CantidadInversiones { get; set; }
                public decimal TotalInvertido { get; set; }
                public List<SocioInversionVm> Inversiones { get; set; }
            }
            public class CommandIndexSocioInversionBySocioAndAnio : IRequest<ResponseVm>
            {
                public Guid SocioId { get; set; }
                public int Anio { get; set; }
            }
            public class CommandHandlerSocioInversionBySocioAndAnio : IRequestHandler<CommandIndexSocioInversionBySocioAndAnio, ResponseVm>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocioInversionBySocioAndAnio(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<ResponseVm> Handle(CommandIndexSocioInversionBySocioAndAnio command, CancellationToken cancellationToken)
                {
                    var anio = DateTime.Now.Year;

                    var sociosInversiones = await _context.SocioInversion.Where(x => x.SocioId == command.SocioId && x.FechaIngreso.Year == command.Anio && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.DetallesSocioInversion.Where(s => s.IsSoftDeleted == false))
                        .AsNoTracking()
                        .ProjectToType<SocioInversionVm>()
                        .OrderByDescending(x => x.CreatedDate)
                        .ToListAsync();

                    var ganaciasIds = sociosInversiones.SelectMany(x => x.DetallesSocioInversion).Where(x => x.GananciaDetalleSocioId != null).Select(s => s.GananciaDetalleSocioId).Distinct().ToList();
                    var gananciasDetallesSocios = await _context.GananciaDetalleSocio.Where(x => ganaciasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .ProjectToType<GananciaDetalleSocioVm>()
                        .ToListAsync();

                    var totalInvertido = 0M;
                    foreach (var i in sociosInversiones)
                    {
                        if (i.Enabled)
                        {
                            totalInvertido += i.Cantidad;
                        }
                    }

                    var prestamosId = sociosInversiones.SelectMany(x => x.DetallesSocioInversion.Select(s => s.PrestamoId)).ToList();
                    var prestamos = await _context.Prestamo.Where(x => prestamosId.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                    foreach (var inv in sociosInversiones)
                    {
                        inv.DetallesSocioInversion = inv.DetallesSocioInversion.Where(x => !x.IsSoftDeleted).ToList();      //Siempre me mapea los que estan eliminados, entonces asi lo solucione

                        foreach (var de in inv.DetallesSocioInversion)
                        {
                            var prestamo = prestamos.Where(x => x.Id == de.PrestamoId).FirstOrDefault();
                            de.CodigoPrestamo = (prestamo != null) ? prestamo.CodigoPrestamo : "";

                            if (de.GananciaDetalleSocioId != null && de.GananciaDetalleSocioId != Guid.Empty)
                            {
                                var ganaciaDetalle = gananciasDetallesSocios.Where(x => x.Id == de.GananciaDetalleSocioId).FirstOrDefault();
                                if (ganaciaDetalle != null)
                                {
                                    de.GananciaDetalleSocio = ganaciaDetalle;
                                }
                            }
                        }
                    }


                    var newRes = new ResponseVm
                    {
                        CantidadInversiones = sociosInversiones.Count(),
                        TotalInvertido = totalInvertido,
                        Inversiones = sociosInversiones
                    };


                    return newRes;
                }
            }
        }





        public class IndexSocioInversion
        {
            public class CommandIndexSocioInversion : IRequest<List<SocioInversionVm>>
            {

            }
            public class CommandHandlerSocioInversion : IRequestHandler<CommandIndexSocioInversion, List<SocioInversionVm>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerSocioInversion(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<SocioInversionVm>> Handle(CommandIndexSocioInversion command, CancellationToken cancellationToken)
                {
                    var socios = await _context.SocioInversion.Where(x => !x.IsSoftDeleted && x.Enabled && x.DetallesSocioInversion.Any(s => !s.IsSoftDeleted))
                        .AsNoTracking()
                        .ProjectToType<SocioInversionVm>()
                        .ToListAsync();

                    var prestamosId = socios.SelectMany(x => x.DetallesSocioInversion.Select(s => s.PrestamoId)).ToList();
                    var prestamos = await _context.Prestamo.Where(x => prestamosId.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                    foreach (var inv in socios)
                    {
                        inv.DetallesSocioInversion = inv.DetallesSocioInversion.Where(x => !x.IsSoftDeleted).ToList();      //Siempre me mapea los que estan eliminados, entonces asi lo solucione

                        foreach (var de in inv.DetallesSocioInversion)
                        {
                            var prestamo = prestamos.Where(x => x.Id == de.PrestamoId).FirstOrDefault();
                            de.CodigoPrestamo = (prestamo != null) ? prestamo.CodigoPrestamo : "";
                        }
                    }

                    return socios;
                }
            }
        }







        //Mantenimientos de nuevas popiedades
        public class LlenarNuevosCampos          //Este es temporal por razon de prueba de retiros de socios inversiones
        {
            public class CommandLlenarNuevosCampos : IRequest<AppResult>
            {

            }
            public class CommandHandlerLlenarNuevosCampos : IRequestHandler<CommandLlenarNuevosCampos, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandlerLlenarNuevosCampos(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandLlenarNuevosCampos command, CancellationToken cancellationToken)
                {
                    var socios = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled).ToListAsync();
                    var sociosIds = socios.Select(x => x.Id).ToList();

                    await _context.SocioInversion.Where(x => !x.IsSoftDeleted && x.Enabled).ToListAsync();


                    foreach(var socio in socios)
                    {
                        var inversiones = socio.SocioInversiones.ToList();
                        var socioSaldoDisponibleARetirar = 0M;
                        foreach (var inv in inversiones)
                        {
                            inv.CantidadActiva = inv.Cantidad;
                            inv.GananciaDisponible = inv.Ganancia;
                            inv.CantidadDisponibleRetirar = inv.CantidadActiva + inv.GananciaDisponible;

                            socioSaldoDisponibleARetirar += inv.CantidadDisponibleRetirar;
                        }

                        socio.SaldoDisponibleARetirar = socioSaldoDisponibleARetirar;
                    }

                    await _context.SaveChangesAsync();


                    return AppResult.New(true, "Todo cheque");
                }
            }
        }









        public class SocioInversionVm
        {
            public Guid Id { get; set; }
            public Guid SocioId { get; set; }
            public Guid CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CodigoInversion { get; set; }
            public DateTime FechaIngreso { get; set; }
            public decimal Cantidad { get; set; }
            public string Moneda_Descripcion { get; set; }
            public string Descripcion { get; set; }
            public EstadoInversion Estado { get; set; }
            public string Estado_Descripcion { get; set; }
            public decimal CantidadPrestada { get; set; }
            public decimal PorcetajePrestado { get; set; }
            public decimal NoPrestado { get; set; }
            public int Movimientos { get; set; }
            public string SocioNombre { get; set; }
            public bool Enabled { get; set; }
            public bool IsSoftDeleted { get; set; }
            public decimal Ganancia { get; set; }

            public decimal GananciaDisponible { get; set; }
            public decimal CantidadDisponibleRetirar { get; set; }
            public decimal Retirado { get; set; }
            public decimal CantidadActiva { get; set; }
            public decimal CantidadEnEuro { get; set; }
            public decimal CantidadEnDolar { get; set; }


            public List<DetalleSocioInversionVm> DetallesSocioInversion { get; set; }
        }
        public class DetalleSocioInversionVm
        {
            public Guid Id { get; set; }
            public Guid SocioInversionId { get; set; }
            public decimal Habia { get; set; }
            public decimal SePresto { get; set; }
            public decimal Quedan { get; set; }
            public decimal PorcentajeEnPrestamo { get; set; }
            public decimal CantidadPagadaDePrestamo { get; set; }
            public Guid PrestamoId { get; set; }
            public string CodigoPrestamo { get; set; }
            public Guid CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool Enabled { get; set; }
            public bool IsSoftDeleted { get; set; }

            public GananciaDetalleSocioVm GananciaDetalleSocio { get; set; }
            public Guid? GananciaDetalleSocioId { get; set; }

        }




    }
}

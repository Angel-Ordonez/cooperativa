using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Engine;
using Cooperativa.App.Utilidades;
using Hangfire;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.ClienteCrud;
using static Cooperativa.App.CRUD.PrestamoCrud;
using static Cooperativa.App.CRUD.PrestamoCrud.Create;
using static Cooperativa.App.CRUD.SocioCrud;
using static Cooperativa.App.CRUD.SocioInversionCrud;

namespace Cooperativa.App.CRUD
{
    public class PrestamoDetalleCrud
    {

        public class Create
        {
            public class Ajuste
            {
                public decimal Interes { get; set; }
                public decimal Capital { get; set; }
                //public int DiasDeGracias { get; set; }
                public DateTime PagaHasta { get; set; }
            }
            public class CommandPrestamoDetalleCreate : IRequest<AppResult>
            {
                public decimal TotalAPagar { get; set; }
                public Ajuste Ajuste { get; set; }
                public Guid PrestamoId { get; set; }
                public DateTime FechaPago { get; set; }
                public Guid ClienteId { get; set; }
                public string Observacion { get; set; }
                //public Guid? CuentaBancariaId { get; set; }
                public int DiasDeGracias { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                public string ReferenciaBancaria { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoDetalleCreate, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly ICalculationService _calculationService;
                private readonly INotificacionesEngine _inotificacionesEngine;
                public CommandHandler(CooperativaDbContext context, ICalculationService calculationService, INotificacionesEngine notificacionesEngine)
                {
                    _context = context;
                    _calculationService = calculationService;
                    _inotificacionesEngine = notificacionesEngine;
                }

                public async Task<AppResult> Handle(CommandPrestamoDetalleCreate command, CancellationToken cancellationToken)
                {

                    try
                    {

                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        if ((command.TotalAPagar == 0 || command.TotalAPagar < 1) && command.Ajuste == null)
                        {
                            return AppResult.New(false, $"Total a pagar no puede ser 0");
                        }

                        var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (cliente == null)
                        {
                            return AppResult.New(false, "Cliente no existe o se ha eliminado");
                        }


                        var prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted && x.Enabled)
                            .Include(x => x.PrestamoDetalles)
                            .FirstOrDefaultAsync();

                        if (prestamo == null)
                        {
                            prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted).FirstOrDefaultAsync();

                            if (prestamo != null && prestamo.Estado == EstadoPrestamo.Pagado)
                            {
                                return AppResult.New(false, "El Prestamo ya ha sido pagado en su totalidad!");
                            }

                            return AppResult.New(false, "No existe prestamo o se ha eliminado!");
                        }



                        var egreso = await _context.Egreso.Where(x => x.PrestamoId == prestamo.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                        var transaccion = await _context.Transaccion.Where(x => x.EgresoId == egreso.Id && !x.IsSoftDeleted)
                            .Include(x => x.Caja)
                            .FirstOrDefaultAsync();
                        transaccion.ThrowIfNull("No se encontro transaccion");
                        //var caja = transaccion.Caja;
                        var caja = await _context.Caja.Where(x => x.Referencia == "COOPAZ-PRE-2025-CAJ001").FirstOrDefaultAsync();
                        caja.ThrowIfNull("No se encontro Caja");
                        if (caja.IsSoftDeleted || !caja.Enabled)
                        {
                            throw new Exception("Se esta tomando una Caja eliminada");
                        }

                        var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaDestinoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cuentaDestino.ThrowIfNull($"No se encontro cuenta destino");
                        var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaOrigenId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cuentaOrigen.ThrowIfNull($"No se encontro cuenta Origen");


                        if(cuentaOrigen.NumeroCuenta.ToLower() != "efectivo" || cuentaOrigen.NumeroCuenta.ToLower() != "efectivo")
                        {
                            if(command.ReferenciaBancaria == null || command.ReferenciaBancaria.Length < 1)
                            {
                                throw new Exception("Se requiere Referencia Bancaria del pago");
                            }
                        }

                        var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                        //var cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        //if (cuentaBancaria == null) { return AppResult.New(false, "Cuenta Bancaria no valida"); }
                        //cuentaBancaria.SumarMovimiento(command.TotalAPagar);

                        decimal capitalActual = 0;
                        decimal interesAPagar = 0;
                        decimal capitalAPagar = 0;
                        decimal restaCapital = 0;
                        decimal proximoPago = 0;
                        decimal deudaTotal = 0;
                        var obsevacion = "";
                        var fechaPago = DateTime.Now;

                        if (prestamo != null)
                        {
                            var calculos = _calculationService.InteresDiario_CalcularInteresAndCapitalByTotalAPagar(prestamo, configuracion, command.TotalAPagar, command.DiasDeGracias);


                            //Aqui pueden haber varios escenarios
                            //1. Como normal viene funcionando
                            //2. Por Dias de Gracias (Aqui tengo que validar si concuerda los dias de gracia)
                            //3. Por Ajuste (El usuario poner el el capital e interes a pagar y la fecha hasta donde)


                            if (command.Ajuste != null)
                            {
                                //Aqui es cuando el usuario paga de manera manual
                                if (command.Ajuste.Interes > 0 && (calculos.InteresGenerado == 0 || calculos.InteresGenerado < 0))
                                {
                                    throw new Exception("Interes no valido, el prestamo no tiene interes generado");
                                }
                                if (command.Ajuste.PagaHasta > DateTime.Now)
                                {
                                    throw new Exception("Hasta que fecha paga, no puede ser mayor al dia de hoy");
                                }

                                interesAPagar = command.Ajuste.Interes;
                                capitalAPagar = command.Ajuste.Capital;
                                fechaPago = command.Ajuste.PagaHasta;

                                command.TotalAPagar = command.Ajuste.Capital + command.Ajuste.Interes;


                                restaCapital = prestamo.RestaCapital - command.Ajuste.Capital;
                                var interesPorcentaje = 0M;
                                if (prestamo.InteresPorcentaje > 1)
                                {
                                    interesPorcentaje = prestamo.InteresPorcentaje / 100;
                                }
                                else
                                {
                                    interesPorcentaje = prestamo.InteresPorcentaje;
                                }
                                proximoPago = restaCapital * interesPorcentaje;

                                //capitalActual
                                //deudaTotal
                                if(command.Observacion != null && command.Observacion.Length > 0)
                                {
                                    obsevacion = command.Observacion;
                                }
                                else
                                {
                                    obsevacion = "Pagó de Prestamo (Pago avanzado manual) " + prestamo.CodigoPrestamo + " Interes: " + interesAPagar.ToString("N2") + " " + prestamo.Moneda + " " + " Capital: " + capitalAPagar.ToString("N2") + " " + prestamo.Moneda;

                                }

                            }
                            else if (command.DiasDeGracias > 0)
                            {

                                if (calculos.ErrorDescripcion != null && calculos.ErrorDescripcion.Length > 0)
                                {
                                    throw new Exception("No es posible aplicar dias de gracias; " + calculos.ErrorDescripcion);
                                }

                                capitalActual = calculos.CapitalActual;

                                interesAPagar = calculos.CalculoConDiasDeGracias.InteresAPagar;
                                capitalAPagar = calculos.CalculoConDiasDeGracias.CapitalAPagar;
                                proximoPago = calculos.CalculoConDiasDeGracias.ProximoPago;

                                restaCapital = calculos.CalculoConDiasDeGracias.RestaCapital;
                                deudaTotal = calculos.CalculoConDiasDeGracias.DeudaTotal;

                                fechaPago = calculos.CalculoConDiasDeGracias.HastaFecha;

                                obsevacion = $"Pagó de interes {interesAPagar.ToString("N2")} {prestamo.Moneda} y de Capital pago {capitalAPagar.ToString("N2")} {prestamo.Moneda} ***Pago con {command.DiasDeGracias} Dias de gracias)";

                            }
                            else
                            {

                                capitalActual = calculos.CapitalActual;

                                interesAPagar = calculos.InteresAPagar;
                                capitalAPagar = calculos.CapitalAPagar;
                                proximoPago = calculos.ProximoPago;

                                restaCapital = calculos.RestaCapital;
                                deudaTotal = calculos.DeudaTotal;

                                if (command.FechaPago != null && command.FechaPago != DateTime.MinValue)
                                {
                                    fechaPago = command.FechaPago;  //Aqui es donde podrian hacer trampa, porque podrian seleccionar dias que no se pagaron
                                }

                                obsevacion = "Pagó de interes " + interesAPagar.ToString("N2") + " " + prestamo.Moneda + " " + " y de Capital pago " + capitalAPagar.ToString("N2") + " " + prestamo.Moneda;
                            }

                            


                        }
                        else
                        {
                            return AppResult.New(false, $"No se encontro Prestamo con ClienteId: {cliente.Nombre + " " + cliente.Apellido}");
                        }




                        var UltimoPago = await _context.PrestamoDetalle.Where(x => x.ClienteId == command.ClienteId && x.PrestamoId == command.PrestamoId && !x.IsSoftDeleted && x.Enabled)
                           .AsNoTracking()
                           .OrderByDescending(x => x.NumeroCuota)
                           .FirstOrDefaultAsync();

                        var numeroCouta = (UltimoPago != null) ? UltimoPago.NumeroCuota : 0;





                        var newDetalle = PrestamoDetalle.New(numeroCouta + 1, capitalAPagar, interesAPagar, capitalAPagar + interesAPagar, restaCapital, fechaPago, /*obsevacion*/ command.Observacion, prestamo.Id, cliente.Id, command.CuentaBancariaOrigenId, createdBy);
                        newDetalle.Moneda = prestamo.Moneda;
                        newDetalle.ProximoPago = proximoPago;
                        newDetalle.FechaProximoPago = fechaPago.AddMonths(1);
                        newDetalle.DiasDeGraciasAplicados = command.DiasDeGracias;

                        await _context.PrestamoDetalle.AddAsync(newDetalle);

                        prestamo.MesesPagados += 1;         //Este ver como arregarlo
                        prestamo.CuotasPagadas += 1;
                        prestamo.MontoPagado += newDetalle.TotalAPagar;
                        prestamo.RestaCapital = restaCapital;
                        prestamo.EstimadoAPagarMes = proximoPago;
                        prestamo.FechaUltimoPago = fechaPago;
                        prestamo.Ganancia += newDetalle.MontoInteres;

                        if (prestamo.RestaCapital == 0)
                        {
                            prestamo.Estado = EstadoPrestamo.Pagado;
                            prestamo.Enabled = false;
                            cliente.PrestamoActivo = false;
                            prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)prestamo.Estado);
                            prestamo.Observacion = prestamo.Observacion + " (Pagado)";
                        }


                        var ingresos = await _context.Ingreso.Where(x => !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                        var ingresosPrestamoDetalles = ingresos.Where(x => x.PrestamoDetalleId != null && x.PrestamoDetalleId != Guid.Empty).ToList();
                        var numeroIngreso = ingresos.OrderByDescending(x => x.NumeroIngreso).Select(x => x.NumeroIngreso).FirstOrDefault();

                        var transacciones = await _context.Transaccion.OrderByDescending(x => x.NumeroTransaccion).Select(x => x.NumeroTransaccion).FirstOrDefaultAsync();

                        var codigoIngreso = /*"I-" +*/ prestamo.CodigoPrestamo + "-I" + (ingresosPrestamoDetalles.Count() + 1).ToString();

                        var newIngreso = Ingreso.NewPorPrestamoDetalle(numeroIngreso + 1, codigoIngreso, newDetalle.TotalAPagar, $"Pago de couta de prestamo {prestamo.CodigoPrestamo}", obsevacion, newDetalle.Id, createdBy);
                        await _context.Ingreso.AddAsync(newIngreso);

                        var newTransaccion = Transaccion.New(transacciones + 1, command.ReferenciaBancaria, newDetalle.TotalAPagar, cuentaOrigen.Id, cuentaDestino.Id, createdBy);
                        newTransaccion.IngresoId = newIngreso.Id;
                        newTransaccion.CajaId = caja.Id;
                        await _context.Transaccion.AddAsync(newTransaccion);


                        cuentaOrigen.RestarMovimiento(newIngreso.Monto);                 //Cliente
                        cuentaDestino.SumarMovimiento(newIngreso.Monto);                 //Caja


                        //OJO; ME HACE FALTA PONER ESTA PARTE AL ELIMINAR UN DETALLEPRESTAMO, AHORITA ESTA DANDO ERROR PORQUE NECESITO HACER EN EL DELETE

                        #region SocioInversion y sus Detalles


                        //Aqui voy a tener errores de decimales, con redondeos

                        //Aqui podria ir calculando las ganacias cuando se vaya pagando el interes
                        decimal letocaASocios = 0.0M;
                        if (capitalAPagar > 0)
                        {
                            List<MovimientoDetalleSocio> newMovimientosDetalleSocios = new List<MovimientoDetalleSocio>();
                            decimal suma = 0.0M;

                            var detallesSociosInversiones = await _context.DetalleSocioInversion
                                .Include(s => s.SocioInversion)
                                .Where(x => x.PrestamoId == prestamo.Id && !x.IsSoftDeleted && x.Enabled && x.SocioInversion.Enabled)
                                .ToListAsync();



                            var porcentajeSocios = detallesSociosInversiones.Sum(x => x.PorcentajeEnPrestamo);
                            //letocaASocios = Math.Round(capitalAPagar * (porcentajeSocios / 100), 2);
                            letocaASocios = capitalAPagar * (porcentajeSocios / 100);
                            //letocaASocios = detallesSociosInversiones.Sum(x => x.SePresto);

                            decimal centavosQueNecesitoQuitar = 0.0M;
                            foreach (var detalleS in detallesSociosInversiones)
                            {
                                var socioInversion = detalleS.SocioInversion;
                                var pagoDelCapital = capitalAPagar * (detalleS.PorcentajeEnPrestamo / 100);
                                detalleS.CantidadPagadaDePrestamo += pagoDelCapital;

                                var restaParaValidar = detalleS.SePresto - detalleS.CantidadPagadaDePrestamo;

                                if (restaParaValidar < 1 && restaParaValidar > 0 && detalleS.SePresto > detalleS.CantidadPagadaDePrestamo)               //Aqui seria si, hace falta centavos
                                {
                                    detalleS.CantidadPagadaDePrestamo += restaParaValidar;
                                    pagoDelCapital += restaParaValidar;

                                    centavosQueNecesitoQuitar = restaParaValidar;
                                }
                                else if (restaParaValidar < 0 && restaParaValidar > -1 && detalleS.CantidadPagadaDePrestamo > detalleS.SePresto)        //Aqui es cuando hay centavos de mas
                                {
                                    pagoDelCapital += restaParaValidar;
                                    detalleS.CantidadPagadaDePrestamo += restaParaValidar;
                                }
                                else if (centavosQueNecesitoQuitar > 0 && detalleS.CantidadPagadaDePrestamo > detalleS.SePresto)                         //Aqui es cuando quito lo centavos de arriba
                                {
                                    pagoDelCapital -= centavosQueNecesitoQuitar;
                                    detalleS.CantidadPagadaDePrestamo -= centavosQueNecesitoQuitar;                                                     //Se lo quito al siguiente
                                }

                                var newMovimientoDetalleSocio = MovimientoDetalleSocio.New(detalleS.Id, pagoDelCapital, createdBy);
                                newMovimientosDetalleSocios.Add(newMovimientoDetalleSocio);

                                if (detalleS.SePresto < detalleS.CantidadPagadaDePrestamo)
                                {
                                    return AppResult.New(false, $"ERROR: discripancia en cantidades de DetalleInversion {socioInversion.CodigoInversion}");
                                }

                                if (detalleS.SePresto == detalleS.CantidadPagadaDePrestamo)
                                {
                                    detalleS.Enabled = false;
                                }

                                socioInversion.CantidadPrestada -= pagoDelCapital;
                                socioInversion.NoPrestado += pagoDelCapital;
                                socioInversion.PorcetajePrestado = (socioInversion.CantidadPrestada / socioInversion.Cantidad) * 100;

                                if (socioInversion.CantidadPrestada < 0 || socioInversion.PorcetajePrestado < 0)
                                {
                                    return AppResult.New(false, $"ERROR: discripancia en cantidades de DetalleInversion {socioInversion.CodigoInversion}");
                                }

                                if (socioInversion.PorcetajePrestado < 100 && socioInversion.Estado == EstadoInversion.Agotado)
                                {
                                    socioInversion.Estado = EstadoInversion.EnCaja;
                                    socioInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)socioInversion.Estado);
                                }

                                suma += pagoDelCapital;
                            }

                            var mergenCentavos = letocaASocios - suma;
                            if (mergenCentavos > 0.50M)                              //No todos los calculos salen perfectos, e¿queda un margen de centavos... por ahora lo deje que no supere los 50 centavos (Por ahora no afecta en nada)
                            {
                                return AppResult.New(false, $"ERROR: El margen de calculo sobrepasa");
                            }


                            if (newMovimientosDetalleSocios.Any())
                            {
                                await _context.MovimientoDetalleSocio.AddRangeAsync(newMovimientosDetalleSocios);
                            }

                        }

                        #endregion






                        await _context.SaveChangesAsync();




                        #region Proceso viejo caja

                        //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                        //var nuevoSaldo = ultimoRegistroCaja.SaldoActual + newIngreso.Monto;

                        //var agregarACaja = Caja.NewIngreso(createdBy, codigoIngreso, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newIngreso.Id, createdBy);

                        //ultimoRegistroCaja.Enabled = false;
                        //ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                        //ultimoRegistroCaja.ModifiedBy = createdBy;


                        //var calculosNuevasGanancias = await CalculoGananciaSocioDetalle(prestamo.Id, createdBy);

                        //await _context.Caja.AddAsync(agregarACaja);
                        //await _context.SaveChangesAsync();

                        #endregion


                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual + newIngreso.Monto;

                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, createdBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                        var calculosNuevasGanancias = await CalculoGananciaSocioDetalle(prestamo.Id, createdBy);

                        await _context.SaveChangesAsync();




                        if (calculosNuevasGanancias.Any())
                        {
                            var calculosGananciasSocios = await CalculoGananciaSocios(prestamo.Id, createdBy);
                            await _context.SaveChangesAsync();
                        }




                        string salida = "";
                        if (proximoPago == 0)
                        {
                            salida = "PRESTAMO FINALIZADO!";
                        }
                        else
                        {
                            salida = $"Proximo pago: {proximoPago.ToString("N2")}";
                        }


                        //var enviarNotificacionCorreo = await _inotificacionesEngine.EnviarCorreoPagoPIM(newDetalle.Id);
                        BackgroundJob.Enqueue(() => _inotificacionesEngine.EnviarCorreoPagoPIM(newDetalle.Id));


                        return AppResult.New(true, salida);
                    }
                    catch (Exception e)
                    {
                        return AppResult.New(false, e.Message);
                    }

                }




                public async Task<List<GananciaDetalleSocio>> CalculoGananciaSocioDetalle(Guid prestamoId, Guid createdBy)
                {
                    var pagosPrestamo = await _context.PrestamoDetalle.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted).ToListAsync();
                    var sumaInteres = pagosPrestamo.Sum(x => x.MontoInteres);

                    var detallesSociosInversiones = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.SocioInversion)
                        .Include(x => x.SocioInversion).ThenInclude(x => x.Socio)
                        .ToListAsync();

                    List<GananciaDetalleSocio> nuevasGananciasDetalle = new List<GananciaDetalleSocio>();

                    if (detallesSociosInversiones.Any())
                    {
                        var detallesSociosInversionIds = detallesSociosInversiones.Select(x => x.Id).ToList();
                        var gananciasDetalleActuales = await _context.GananciaDetalleSocio.Where(x => detallesSociosInversionIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted && x.Enabled).ToListAsync();

                        foreach (var detalle in detallesSociosInversiones)
                        {
                            var socio = detalle.SocioInversion.Socio;
                            if (socio == null)
                            {
                                socio = await _context.Socio.Where(x => x.Id == detalle.SocioInversion.SocioId).FirstOrDefaultAsync();
                            }

                            //var newCalculoGanancia = sumaInteres * (detalle.PorcentajeEnPrestamo / 100);  //Este funciona si el socio ganara 100% del interes, pero cuando se crea un Socio se le pone cuando es el % de ganancia
                            var gananciaPrevia = sumaInteres * (detalle.PorcentajeEnPrestamo / 100);
                            var newCalculoGanancia = gananciaPrevia * (socio.PorcentajeGanancia / 100);

                            var newGananciaDetalle = GananciaDetalleSocio.New(detalle.Id, sumaInteres, newCalculoGanancia, createdBy);

                            var gananciaActual = gananciasDetalleActuales.Where(x => x.DetalleSocioInversionId == detalle.Id).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                            if (gananciaActual != null)
                            {
                                newGananciaDetalle.Retirado = gananciaActual.Retirado;
                                newGananciaDetalle.CantidadRetirada = gananciaActual.CantidadRetirada;
                                newGananciaDetalle.CantidadDisponibleARetirar = newGananciaDetalle.Ganancia - gananciaActual.CantidadRetirada;

                                //Al anterior la deshabulito porque la sustituye la nueva
                                newGananciaDetalle.GananciaDetalleSocioAnteriorId = gananciaActual.Id;
                                gananciaActual.Enabled = false;
                                gananciaActual.ModifiedBy = createdBy;
                                gananciaActual.ModifiedDate = DateTime.Now;
                                //gananciaActual.Observacion = "Se sustituyo por: " + newGananciaDetalle.Id.ToString();
                            }

                            await _context.GananciaDetalleSocio.AddAsync(newGananciaDetalle);


                            if (gananciaActual != null) { gananciaActual.Observacion = "Se sustituyo por: " + newGananciaDetalle.Id.ToString(); }
                            detalle.GananciaDetalleSocioId = newGananciaDetalle.Id;

                            nuevasGananciasDetalle.Add(newGananciaDetalle);
                        }
                    }


                    return nuevasGananciasDetalle;
                }






                public async Task<List<Guid>> CalculoGananciaSocios(Guid prestamoId, Guid createdBy)
                {

                    var detallesSociosInversiones = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted/* && x.Enabled*/)
                        .Include(x => x.SocioInversion)
                        .AsNoTracking()
                        .ToListAsync();


                    if (detallesSociosInversiones.Any())
                    {
                        var sociosInversionesId = detallesSociosInversiones.Select(x => x.SocioInversionId).ToList();

                        var sociosInversiones = await _context.SocioInversion.Where(x => sociosInversionesId.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();

                        var sociosIds = sociosInversiones.Select(x => x.SocioId).Distinct().ToList();

                        var socios = await _context.Socio.Where(x => sociosIds.Contains(x.Id) && !x.IsSoftDeleted)
                            .Include(x => x.SocioInversiones)
                            .ToListAsync();
                        var sociosInversionesAll = socios.SelectMany(x => x.SocioInversiones).ToList();
                        var sociosInversionAllIds = sociosInversionesAll.Select(x => x.Id).ToList();

                        var detallesAll = await _context.DetalleSocioInversion.Where(x => sociosInversionAllIds.Contains(x.SocioInversionId) && !x.IsSoftDeleted && x.Enabled)
                            //.Include(x => x.GananciaDetalleSocio)
                            .ToListAsync();

                        var detallesAllsIds = detallesAll.Select(x => x.Id).ToList();
                        var ganaciasAll = await _context.GananciaDetalleSocio.Where(x => detallesAllsIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted && x.Enabled)
                            .ToListAsync();

                        foreach (var socio in socios)
                        {
                            var inversiones = socio.SocioInversiones.Where(x => x.Enabled).ToList();

                            var sociosInversionIds = inversiones.Select(x => x.Id).ToList();

                            var detalles = detallesAll.Where(x => sociosInversionIds.Contains(x.SocioInversionId)).ToList();

                            if (detalles.Any())
                            {
                                decimal gananciaSocio = 0M;
                                //decimal retirosSocio = 0M;      //Esta no la puse, porque seria en el otro modulo retiros mejor

                                foreach (var inversion in inversiones)
                                {
                                    var detallesInversiones = inversion.DetallesSocioInversion.Where(s => s.Enabled).ToList();
                                    var detallesIds = detallesInversiones.Select(x => x.Id).Distinct().ToList();

                                    var gananciaInversion = ganaciasAll.Where(x => detallesIds.Contains(x.DetalleSocioInversionId)).ToList().Sum(x => x.Ganancia);
                                    inversion.Ganancia = gananciaInversion;
                                    inversion.GananciaDisponible = gananciaInversion - inversion.GananciaDisponible;
                                    inversion.ModifiedBy = createdBy;
                                    inversion.ModifiedDate = DateTime.Now;

                                    gananciaSocio += gananciaInversion;
                                }

                                socio.GananciaTotal = gananciaSocio;
                            }


                        }

                        return sociosIds;
                    }


                    return null;
                }







            }



        }



















        public class GetInteresAndFechaByCantidad
        {

            public class DetalleAPagarVm
            {
                public decimal InteresAPagar { get; set; }
                public decimal CapitalAPagar { get; set; }
                public DateTime HastaFecha { get; set; }
            }

            public class CommandPrestamoDetalleIn : IRequest<AppResult>
            {
                public decimal TotalAPagar { get; set; }
                public Guid PrestamoId { get; set; }
                public Guid ClienteId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoDetalleIn, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandPrestamoDetalleIn command, CancellationToken cancellationToken)
                {
                    if (command.TotalAPagar == 0 || command.TotalAPagar < 0)
                    {
                        return AppResult.New(false, $"Total a pagar no puede ser 0");
                    }

                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                    if( configuracion == null ) { return AppResult.New(false, "No se encontro Configuración."); }

                    var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().FirstOrDefaultAsync();
                    if (cliente == null)
                    {
                        return AppResult.New(false, "Cliente no existe o se ha eliminado");
                    }

                    var prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.PrestamoDetalles)
                        .FirstOrDefaultAsync();

                    if (prestamo == null)
                    {
                        prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                        if (prestamo != null && prestamo.Estado == EstadoPrestamo.Pagado)
                        {
                            return AppResult.New(false, "El Prestamo ya ha sido pagado en su totalidad!");
                        }

                        return AppResult.New(false, "No existe prestamo o se ha eliminado!");
                    }


                    decimal capitalActual = 0;
                    decimal interesAPagar = 0;
                    decimal capitalAPagar = 0;
                    decimal restaCapital = 0;
                    decimal proximoPago = 0;
                    decimal deudaTotal = 0;
                    var obsevacion = "";
                    DateTime hastaFecha = DateTime.Now;

                    if (prestamo != null)
                    {
                        if (prestamo.RestaCapital > 0)
                        {
                            capitalActual = prestamo.RestaCapital;
                        }
                        else
                        {
                            capitalActual = prestamo.CantidadInicial;
                        }


                        #region Calcular el interes a pagar

                        TimeSpan restarFechas = DateTime.Now - prestamo.FechaUltimoPago;                     // Restar las fechas: Desde el ultimo pago hasta la actualida
                        int cantidadDias = restarFechas.Days;                                                // Obtener la cantidad de días que se generaron interes
                        var ultimoDetalle = prestamo.PrestamoDetalles.Where(x => !x.IsSoftDeleted).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                        var interes = prestamo.InteresPorcentaje;
                        if (interes > 1)
                        {
                            interes = interes / 100;
                        }

                        if(((double)interes) < 0.05 || ((double)interes) > 0.1)
                        {
                            return AppResult.New(false, $"Interes no valido. No cumple entre 5% y 10%");
                        }

                        var interesMes = Math.Round(prestamo.RestaCapital * interes, 2);


                        if (prestamo.FechaUltimoPago == DateTime.MinValue || prestamo.FechaUltimoPago == null)  //Si no han pagado ningun cuota
                        {
                            restarFechas = DateTime.Now - prestamo.FechaEntragado;
                            cantidadDias = restarFechas.Days;

                            if (configuracion.PrimerMesInteresObligatorio && cantidadDias <= 30)
                            {
                                interesAPagar = interesMes;
                            }
                            else   //Pero que pasa cuando es febrero? solo tiene 28 o 29 dias
                            {
                                var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;
                                //var interesAlDia = interesMes / 30;     //30 dias
                                var interesAlDia = interesMes / Convert.ToDecimal(restaConproximoMes.TotalDays);
                                interesAPagar = interesAlDia * cantidadDias;
                            }


                            hastaFecha = prestamo.FechaEntragado.AddDays(cantidadDias);
                        }
                        else if (ultimoDetalle != null)
                        {
                            var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;

                            interesMes = ultimoDetalle.ProximoPago;

                            //Calcular el interes a pagar por dias

                            var interesBaseMes = ultimoDetalle.ProximoPago;
                            //var interesAlDia = interesBaseMes / 30;     //30 dias
                            var interesAlDia = interesBaseMes / Convert.ToDecimal(restaConproximoMes.TotalDays);

                            interesAPagar = interesAlDia * cantidadDias;
                            hastaFecha = ultimoDetalle.FechaPago.AddDays(cantidadDias);
                        }

                        #endregion






                        interesAPagar = Math.Round(interesAPagar, 2);

                        if(command.TotalAPagar < interesAPagar && command.TotalAPagar >= interesMes)
                        {
                            interesAPagar = interesMes;
                            if (ultimoDetalle != null)
                            {
                                //hastaFecha = ultimoDetalle.FechaPago.AddDays(30);
                                hastaFecha = ultimoDetalle.FechaPago.AddMonths(1);
                            }
                            else
                            {
                                //hastaFecha = prestamo.FechaEntragado.AddDays(30);
                                hastaFecha = prestamo.FechaEntragado.AddMonths(1);
                            }      
                        }


                        deudaTotal = Math.Round(interesAPagar + capitalActual, 2);

                        if (command.TotalAPagar < interesAPagar)
                        {
                            return AppResult.New(false, $"Cantidad es insuficiente: | Interes Mes: {interesMes} {prestamo.Moneda} | Interes Generado: {interesAPagar.ToString("N2")} {prestamo.Moneda} | Capital: {capitalActual.ToString("N2")} {prestamo.Moneda} |");
                        }
                        else
                        {
                            capitalAPagar = Math.Round(command.TotalAPagar - interesAPagar, 2);
                        }

                        if (command.TotalAPagar > deudaTotal)
                        {
                            return AppResult.New(false, $"El total a pagar es mayor a la deuda total: {deudaTotal.ToString("N2")} {prestamo.Moneda}");
                        }

                        restaCapital = Math.Round(capitalActual - capitalAPagar, 2);
                        if (prestamo.InteresPorcentaje > 1)
                        {
                            proximoPago = Math.Round(restaCapital * (prestamo.InteresPorcentaje / 100), 2);
                        }
                        else
                        {
                            proximoPago = Math.Round(restaCapital * prestamo.InteresPorcentaje, 2);
                        }

                        obsevacion = "Pagó de interes " + interesAPagar.ToString("N2") + " " + prestamo.Moneda + " " + " y de Capital pago " + capitalAPagar.ToString("N2") + " " + prestamo.Moneda + ".";
                    }
                    else
                    {
                        return AppResult.New(false, $"No se encontro Prestamo con ClienteId: {cliente.Nombre + " " + cliente.Apellido}");
                    }

                    var newDetalle = new DetalleAPagarVm
                    {
                        InteresAPagar = interesAPagar,
                        CapitalAPagar = capitalAPagar,
                        HastaFecha = hastaFecha
                    };


                    return AppResult.New(true, newDetalle);

                }
            }



        }




        public class GetInteresAndFechaByCantidadV2
        {

            public class DetalleAPagarVm
            {
                public decimal InteresAPagar { get; set; }
                public decimal CapitalAPagar { get; set; }
                public decimal Mora { get; set; }
                public DateTime HastaFecha { get; set; }
            }

            public class CommandPrestamoDetalleIn : IRequest<AppResult>
            {
                public decimal TotalAPagar { get; set; }
                public Guid PrestamoId { get; set; }
                public Guid ClienteId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoDetalleIn, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandPrestamoDetalleIn command, CancellationToken cancellationToken)
                {
                    if (command.TotalAPagar <= 0 )
                    {
                        return AppResult.New(false, $"Total a pagar no puede ser 0 o negativo");
                    }

                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                    if (configuracion == null) { return AppResult.New(false, "No se encontro Configuración."); }

                    var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().FirstOrDefaultAsync();
                    if (cliente == null)
                    {
                        return AppResult.New(false, "Cliente no existe o se ha eliminado");
                    }

                    var prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.PrestamoDetalles)
                        .FirstOrDefaultAsync();

                    if (prestamo == null)
                    {
                        prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                        if (prestamo != null && prestamo.Estado == EstadoPrestamo.Pagado)
                        {
                            return AppResult.New(false, "El Prestamo ya ha sido pagado en su totalidad!");
                        }

                        return AppResult.New(false, "No existe prestamo o se ha eliminado!");
                    }


                    decimal capitalActual = 0;
                    decimal interesAPagar = 0;
                    decimal capitalAPagar = 0;
                    decimal restaCapital = 0;
                    decimal proximoPago = 0;
                    decimal deudaTotal = 0;
                    var obsevacion = "";
                    DateTime hastaFecha = DateTime.Now;

                    if (prestamo != null)
                    {
                        if (prestamo.RestaCapital > 0)
                        {
                            capitalActual = prestamo.RestaCapital;
                        }
                        else
                        {
                            capitalActual = prestamo.CantidadInicial;
                        }


                        #region Calcular el interes a pagar


                        TimeSpan restarFechas = DateTime.Now - prestamo.FechaUltimoPago;                     // Restar las fechas: Desde el ultimo pago hasta la actualida
                        int cantidadDias = restarFechas.Days;                                                // Obtener la cantidad de días que se generaron interes
                        var ultimoDetalle = prestamo.PrestamoDetalles.Where(x => !x.IsSoftDeleted).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                        var interes = prestamo.InteresPorcentaje;
                        if (interes > 1)
                        {
                            interes = interes / 100;
                        }

                        if (((double)interes) < 0.05 || ((double)interes) > 0.1)
                        {
                            return AppResult.New(false, $"Interes no valido. No cumple entre 5% y 10%");
                        }

                        var interesMes = Math.Round(prestamo.RestaCapital * interes, 2);


                        if (prestamo.FechaUltimoPago == DateTime.MinValue || prestamo.FechaUltimoPago == null)  //Si no han pagado ningun cuota
                        {
                            restarFechas = DateTime.Now - prestamo.FechaEntragado;
                            cantidadDias = restarFechas.Days;

                            if (configuracion.PrimerMesInteresObligatorio && cantidadDias <= 30)
                            {
                                interesAPagar = interesMes;
                            }
                            else   //Pero que pasa cuando es febrero? solo tiene 28 o 29 dias
                            {
                                var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;
                                //var interesAlDia = interesMes / 30;     //30 dias
                                var interesAlDia = interesMes / Convert.ToDecimal(restaConproximoMes.TotalDays);
                                interesAPagar = interesAlDia * cantidadDias;
                            }


                            hastaFecha = prestamo.FechaEntragado.AddDays(cantidadDias);
                        }
                        else if (ultimoDetalle != null)
                        {
                            var restaConproximoMes = prestamo.FechaEntragado.AddMonths(1) - prestamo.FechaEntragado;

                            interesMes = ultimoDetalle.ProximoPago;

                            //Calcular el interes a pagar por dias

                            var interesBaseMes = ultimoDetalle.ProximoPago;
                            //var interesAlDia = interesBaseMes / 30;     //30 dias
                            var interesAlDia = interesBaseMes / Convert.ToDecimal(restaConproximoMes.TotalDays);

                            interesAPagar = interesAlDia * cantidadDias;
                            hastaFecha = ultimoDetalle.FechaPago.AddDays(cantidadDias);
                        }

                        #endregion






                        interesAPagar = Math.Round(interesAPagar, 2);

                        if (command.TotalAPagar < interesAPagar && command.TotalAPagar >= interesMes)
                        {
                            interesAPagar = interesMes;
                            if (ultimoDetalle != null)
                            {
                                //hastaFecha = ultimoDetalle.FechaPago.AddDays(30);
                                hastaFecha = ultimoDetalle.FechaPago.AddMonths(1);
                            }
                            else
                            {
                                //hastaFecha = prestamo.FechaEntragado.AddDays(30);
                                hastaFecha = prestamo.FechaEntragado.AddMonths(1);
                            }
                        }


                        deudaTotal = Math.Round(interesAPagar + capitalActual, 2);

                        if (command.TotalAPagar < interesAPagar)
                        {
                            return AppResult.New(false, $"Cantidad es insuficiente: | Interes Mes: {interesMes} {prestamo.Moneda} | Interes Generado: {interesAPagar.ToString("N2")} {prestamo.Moneda} | Capital: {capitalActual.ToString("N2")} {prestamo.Moneda} |");
                        }
                        else
                        {
                            capitalAPagar = Math.Round(command.TotalAPagar - interesAPagar, 2);
                        }

                        if (command.TotalAPagar > deudaTotal)
                        {
                            return AppResult.New(false, $"El total a pagar es mayor a la deuda total: {deudaTotal.ToString("N2")} {prestamo.Moneda}");
                        }

                        restaCapital = Math.Round(capitalActual - capitalAPagar, 2);
                        if (prestamo.InteresPorcentaje > 1)
                        {
                            proximoPago = Math.Round(restaCapital * (prestamo.InteresPorcentaje / 100), 2);
                        }
                        else
                        {
                            proximoPago = Math.Round(restaCapital * prestamo.InteresPorcentaje, 2);
                        }

                        obsevacion = "Pagó de interes " + interesAPagar.ToString("N2") + " " + prestamo.Moneda + " " + " y de Capital pago " + capitalAPagar.ToString("N2") + " " + prestamo.Moneda + ".";
                    }
                    else
                    {
                        return AppResult.New(false, $"No se encontro Prestamo con ClienteId: {cliente.Nombre + " " + cliente.Apellido}");
                    }

                    var newDetalle = new DetalleAPagarVm
                    {
                        InteresAPagar = interesAPagar,
                        CapitalAPagar = capitalAPagar,
                        HastaFecha = hastaFecha
                    };


                    return AppResult.New(true, newDetalle);

                }
            }



        }




        public class GetInteresAndFechaByCantidadAndFechaCotizacion
        {

            public class Respuesta
            {
                public string CodigoPrestamo { get;set; }
                public string NombreCliente { get; set; }

                public decimal DeudaTotal { get; set; }
                public decimal CapitalActual { get; set; }
                public decimal InteresMes { get; set; }
                public decimal InteresDiario { get; set; }
                public int DiasGenerados { get; set; }
                public decimal InteresGenerado { get; set; }
                public int DiasDelPrimerMesDeInteres { get; set; }

                public InteresDiario_CalcularInteresAndCapitalVm PorTotalAPagar { get; set; }
                public InteresDiario_CalcularInteresAndCapitalVm PorFechaCotizacion { get; set; }

            }

            public class CommandGetInteresAndFechaByCantidadAndFechaCotizacion : IRequest<AppResult>
            {
                public decimal? TotalAPagar { get; set; }
                public DateTime? FechaACotizar { get; set; }
                public int DiasDeGracias { get; set; }
                public Guid PrestamoId { get; set; }
                public Guid ClienteId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandGetInteresAndFechaByCantidadAndFechaCotizacion, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly ICalculationService _calculationService;

                public CommandHandler(CooperativaDbContext context, ICalculationService calculationService)
                {
                    _context = context;
                    _calculationService = calculationService;
                }

                public async Task<AppResult> Handle(CommandGetInteresAndFechaByCantidadAndFechaCotizacion command, CancellationToken cancellationToken)
                {
                    if (command.TotalAPagar == 0 || command.TotalAPagar < 0)
                    {
                        return AppResult.New(false, $"Total a pagar no puede ser 0");
                    }

                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                    if (configuracion == null) { return AppResult.New(false, "No se encontro Configuración."); }

                    var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().FirstOrDefaultAsync();
                    if (cliente == null)
                    {
                        return AppResult.New(false, "Cliente no existe o se ha eliminado");
                    }

                    var prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.PrestamoDetalles)
                        .FirstOrDefaultAsync();

                    if (prestamo == null)
                    {
                        prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                        if (prestamo != null && prestamo.Estado == EstadoPrestamo.Pagado)
                        {
                            return AppResult.New(false, "El Prestamo ya ha sido pagado en su totalidad!");
                        }

                        return AppResult.New(false, "No existe prestamo o se ha eliminado!");
                    }




                    var newRes = new Respuesta { };

                    if(command.TotalAPagar > 0)
                    {
                        var calculosPorTotalAPagar = _calculationService.InteresDiario_CalcularInteresAndCapitalByTotalAPagar(prestamo, configuracion, (decimal)command.TotalAPagar, command.DiasDeGracias);
                        
                        if (calculosPorTotalAPagar.Error)
                        {
                            return AppResult.New(false, calculosPorTotalAPagar.ErrorDescripcion);
                        }
                        
                        newRes.PorTotalAPagar = calculosPorTotalAPagar;
                    }
                    if (command.FechaACotizar != null && command.FechaACotizar != DateTime.MinValue)
                    {
                        if (command.FechaACotizar > DateTime.Now.Date)
                        {
                            command.FechaACotizar = DateTime.Now;   //Porque basicamente cotizar por fecha solo le devolvera la parte de intereses
                        }

                        var calculosPorFechaCotizacion = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, (DateTime)command.FechaACotizar, command.DiasDeGracias);

                        if (calculosPorFechaCotizacion.Error)
                        {
                            return AppResult.New(false, calculosPorFechaCotizacion.ErrorDescripcion);
                        }

                        newRes.PorFechaCotizacion = calculosPorFechaCotizacion;
                    }



                    InteresDiario_CalcularInteresAndCapitalVm calculoGeneral = null;
                    if(newRes.PorFechaCotizacion != null)
                    {
                        calculoGeneral = newRes.PorFechaCotizacion;
                    }
                    else
                    {
                        calculoGeneral = newRes.PorTotalAPagar;
                    }

                    newRes.CodigoPrestamo = prestamo.CodigoPrestamo;
                    newRes.NombreCliente = $"{cliente.CodigoPersona} / {cliente.Nombre} {cliente.Apellido}";
                    newRes.CapitalActual = calculoGeneral.CapitalActual;
                    newRes.DeudaTotal = calculoGeneral.DeudaTotal;
                    newRes.InteresMes = calculoGeneral.InteresMes;
                    newRes.InteresDiario = calculoGeneral.InteresDiario;
                    newRes.InteresGenerado = calculoGeneral.InteresGenerado;
                    newRes.DiasGenerados = calculoGeneral.DiasGenerados;
                    newRes.DiasDelPrimerMesDeInteres = calculoGeneral.DiasDelPrimerMesDeInteres;

                    return AppResult.New(true, newRes);
                }
            }
        }






        public class GetInteresAndFechaByCantidadAndFechaCotizacionAvanzado
        {

            public class Respuesta
            {
                public string CodigoPrestamo { get; set; }
                public InteresDiario_CalcularInteresAndCapitalVm PorTotalAPagar { get; set; }
                public InteresDiario_CalcularInteresAndCapitalVm PorFechaCotizacion { get; set; }
            }

            public class CommandGetInteresAndFechaByCantidadAndFechaCotizacion : IRequest<AppResult>
            {
                public Guid PrestamoId { get; set; }
                public decimal? TotalAPagar { get; set; }
                public DateTime? FechaACotizar { get; set; }
                public int DiasDeGracia { get; set; }

                public Guid ClienteId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandGetInteresAndFechaByCantidadAndFechaCotizacion, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly ICalculationService _calculationService;

                public CommandHandler(CooperativaDbContext context, ICalculationService calculationService)
                {
                    _context = context;
                    _calculationService = calculationService;
                }

                public async Task<AppResult> Handle(CommandGetInteresAndFechaByCantidadAndFechaCotizacion command, CancellationToken cancellationToken)
                {
                    if (command.TotalAPagar == 0 || command.TotalAPagar < 0)
                    {
                        return AppResult.New(false, $"Total a pagar no puede ser 0");
                    }

                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                    if (configuracion == null) { return AppResult.New(false, "No se encontro Configuración."); }

                    var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().FirstOrDefaultAsync();
                    if (cliente == null)
                    {
                        return AppResult.New(false, "Cliente no existe o se ha eliminado");
                    }

                    var prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.PrestamoDetalles)
                        .FirstOrDefaultAsync();

                    if (prestamo == null)
                    {
                        prestamo = await _context.Prestamo.Where(x => x.Id == command.PrestamoId && x.ClienteId == command.ClienteId && !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                        if (prestamo != null && prestamo.Estado == EstadoPrestamo.Pagado)
                        {
                            return AppResult.New(false, "El Prestamo ya ha sido pagado en su totalidad!");
                        }

                        return AppResult.New(false, "No existe prestamo o se ha eliminado!");
                    }




                    var newRes = new Respuesta { };

                    if (command.TotalAPagar > 0)
                    {
                        var calculosPorTotalAPagar = _calculationService.InteresDiario_CalcularInteresAndCapitalByTotalAPagar(prestamo, configuracion, (decimal)command.TotalAPagar, command.DiasDeGracia);

                        if (calculosPorTotalAPagar.Error)
                        {
                            return AppResult.New(false, calculosPorTotalAPagar.ErrorDescripcion);
                        }

                        newRes.PorTotalAPagar = calculosPorTotalAPagar;
                    }
                    if (command.FechaACotizar != null && command.FechaACotizar != DateTime.MinValue)
                    {
                        if (command.FechaACotizar > DateTime.Now.Date)
                        {
                            command.FechaACotizar = DateTime.Now;   //Porque basicamente cotizar por fecha solo le devolvera la parte de intereses
                        }

                        var calculosPorFechaCotizacion = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, (DateTime)command.FechaACotizar, command.DiasDeGracia);

                        if (calculosPorFechaCotizacion.Error)
                        {
                            return AppResult.New(false, calculosPorFechaCotizacion.ErrorDescripcion);
                        }

                        newRes.PorFechaCotizacion = calculosPorFechaCotizacion;
                    }

                    newRes.CodigoPrestamo = prestamo.CodigoPrestamo;

                    return AppResult.New(true, newRes);
                }
            }
        }







        public class GetPrestamoDetalleByClienteId
        {
            public class QueryPrestamoId : IRequest<List<PrestamoDetalleVm>>
            {
                public Guid ClienteId { get; set; }
            }

            public class QueryHandler : IRequestHandler<QueryPrestamoId, List<PrestamoDetalleVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoDetalleVm>> Handle(QueryPrestamoId query, CancellationToken cancellationToken)
                {

                    var prestamo = await _context.PrestamoDetalle.Where(x => x.ClienteId == query.ClienteId && !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .ProjectToType<PrestamoDetalleVm>()
                        .ToListAsync();

                    return prestamo;

                }


            }


        }





        public class Delete
        {
            public class CommandDetallePrestamoDelete : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Observacion { get; set; }
                //public Guid ModifiedBy { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandDetallePrestamoDelete, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandDetallePrestamoDelete command, CancellationToken cancellationToken)
                {
                    var prestamoDetalle = await _context.PrestamoDetalle.Where(x => x.Id == command.Id && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.CuentaBancaria)
                        .FirstOrDefaultAsync();
                    if (prestamoDetalle == null) { return AppResult.New(false, "No se encontro PrestamoDetalle"); }

                    var ingresoDetalle = await _context.Ingreso.Where(x => x.PrestamoDetalleId == prestamoDetalle.Id && x.Enabled).FirstOrDefaultAsync();
                    if (ingresoDetalle == null) { return AppResult.New(false, "No se encontro Ingreso a Caja que valide Cuota."); }

                    var prestamo = await _context.Prestamo.Where(x => x.Id == prestamoDetalle.PrestamoId && !x.IsSoftDeleted)
                        .Include(x => x.Cliente)
                        .FirstOrDefaultAsync();
                    if (prestamo == null) { return AppResult.New(false, "No se encontro, o ha sido eliminado antes!"); }


                    var transaccion = await _context.Transaccion.Where(x => x.IngresoId == ingresoDetalle.Id && !x.IsSoftDeleted)
                        .Include(x => x.Caja)
                        .Include(x => x.CuentaBancariaOrigen)
                        .Include(x => x.CuentaBancariaDestino)
                        .FirstOrDefaultAsync();
                    transaccion.ThrowIfNull("No se encontro transaccion");
                    var caja = transaccion.Caja;
                    if (caja.IsSoftDeleted || !caja.Enabled)
                    {
                        throw new Exception("Se esta tomando una Caja eliminada");
                    }

                    var cuentaOrigen = transaccion.CuentaBancariaOrigen;
                    var cuentaDestino = transaccion.CuentaBancariaDestino;
                    caja.ThrowIfNull("No se encontro Caja");
                    cuentaOrigen.ThrowIfNull("No se encontro CuentaBancariaOrigen");
                    cuentaDestino.ThrowIfNull("No se encontro CuentaBancariaDestino");
                    var cuentaBancaria = prestamoDetalle.CuentaBancaria;
                    //if(cuentaBancaria != null)
                    //{
                    //    cuentaBancaria.RestarMovimiento();
                    //}


                    var prestamosDetalles = await _context.PrestamoDetalle.Where(x => x.PrestamoId == prestamo.Id && !x.IsSoftDeleted && x.Enabled).OrderByDescending(s => s.NumeroCuota).ToListAsync();
                    var numerosCuotas = prestamosDetalles.Select(x => x.NumeroCuota).Distinct().ToList();
                    var ultimaCuota = numerosCuotas.Max();
                    var penultimoDetalle = prestamosDetalles.Where(x => x.NumeroCuota == (ultimaCuota-1)).FirstOrDefault();


                    if (penultimoDetalle != null)
                    {
                        prestamo.FechaUltimoPago = penultimoDetalle.FechaPago;
                    }
                    else if (penultimoDetalle == null && prestamoDetalle.NumeroCuota == 1) 
                    {
                        prestamo.FechaUltimoPago = DateTime.MinValue;
                    }
                    else
                    {
                        return AppResult.New(false, $"# Cuota {ultimaCuota - 1} no existe");
                    }

                    if (prestamoDetalle.NumeroCuota != ultimaCuota) { return AppResult.New(false, "Cuota a eliminar no es a ultima"); }

                    var diasValidos = DateTime.Now - prestamoDetalle.CreatedDate;
                    if(diasValidos.TotalDays > 30) { return AppResult.New(false, "Accion no es valida, ya pasaron 30 dias"); }


                    var modifiedBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                    prestamoDetalle.IsSoftDeleted = true;
                    prestamoDetalle.Enabled = false;
                    prestamoDetalle.ModifiedDate = DateTime.Now;
                    prestamoDetalle.ModifiedBy = modifiedBy;
                    prestamoDetalle.Observacion += "***" + command.Observacion;

                    prestamo.MesesPagados -= 1;         //Este ver como arregarlo
                    prestamo.CuotasPagadas -= 1;
                    prestamo.MontoPagado -= prestamoDetalle.TotalAPagar;
                    prestamo.RestaCapital += prestamoDetalle.MontoCapital;


                    decimal interes = prestamo.InteresPorcentaje;
                    if(interes > 1)
                    {
                        interes = interes / 100;
                    }
                    var proximoPago =prestamo.RestaCapital * interes;
                    proximoPago = Math.Round(proximoPago, 2);

                    prestamo.EstimadoAPagarMes = proximoPago;
                    prestamo.Observacion = prestamo.Observacion + " / Cuota " + prestamoDetalle.NumeroCuota + " eliminada con #Ingreso: " + ingresoDetalle.Correlativo;

                    prestamo.Ganancia -= prestamoDetalle.MontoInteres;


                    if (prestamo.Estado == EstadoPrestamo.Pagado)
                    {
                        prestamo.Estado = EstadoPrestamo.Vigente;
                        prestamo.Enabled = true;
                        prestamo.Cliente.PrestamoActivo = true;
                        prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)prestamo.Estado);
                    }
                    prestamo.ModifiedDate = DateTime.Now;
                    prestamo.ModifiedBy = modifiedBy;


                    var cantidadEgresos = await _context.Egreso.CountAsync();
                    var newEgreso = Egreso.New(cantidadEgresos + 1, ingresoDetalle.Correlativo, prestamoDetalle.TotalAPagar, $"Se elimino Cuota con # de Ingreso: {ingresoDetalle.Correlativo} de Prestamo {prestamo.CodigoPrestamo}", command.Observacion, prestamo.Id, modifiedBy);
                    await _context.Egreso.AddAsync(newEgreso);

                    var numTransacciones = await _context.Transaccion.CountAsync();
                    var newTransaccion = Transaccion.New(numTransacciones + 1, ingresoDetalle.Correlativo, prestamoDetalle.TotalAPagar, cuentaOrigen.Id, cuentaDestino.Id, modifiedBy);
                    newTransaccion.EgresoId = newEgreso.Id;
                    newTransaccion.CajaId = caja.Id;
                    await _context.Transaccion.AddAsync(newTransaccion);


                    cuentaOrigen.SumarMovimiento(prestamoDetalle.TotalAPagar);                  //Cliente
                    cuentaDestino.RestarMovimiento(prestamoDetalle.TotalAPagar);                  //Caja




                    #region SocioInversion y sus Detalles
                    //Aqui voy a tener errores de decimales, con redondeos

                    //Aqui podria ir calculando las ganacias cuando se vaya pagando el interes
                    var capitalAPagar = prestamoDetalle.MontoCapital;

                    if (capitalAPagar > 0)
                    {
                        var detallesSociosInversiones = await _context.DetalleSocioInversion.Include(s => s.SocioInversion)
                            .Where(x => x.PrestamoId == prestamo.Id && !x.IsSoftDeleted /*&& x.Enabled*/ && x.SocioInversion.Enabled)
                            .ToListAsync();
                        var detallesInversionesIds = detallesSociosInversiones.Select(x => x.Id).ToList();

                        await _context.MovimientoDetalleSocio.Where(x => detallesInversionesIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted).ToListAsync();

                        foreach (var detalleS in detallesSociosInversiones)
                        {
                            var socioInversion = detalleS.SocioInversion;
                            var ultimoMovimiento = detalleS.MovimientosDetalleSocio.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            if (ultimoMovimiento == null) { return AppResult.New(false, "No se encontro UltimoMovimiento"); }

                            detalleS.CantidadPagadaDePrestamo -= Math.Round(ultimoMovimiento.Cantidad, 2);  //Lo redonde porque aqui guardo 3 cifras

                            if (detalleS.SePresto > detalleS.CantidadPagadaDePrestamo && !detalleS.Enabled)
                            {
                                detalleS.Enabled = true;
                            }

                            socioInversion.CantidadPrestada += Math.Round(ultimoMovimiento.Cantidad, 2);
                            socioInversion.PorcetajePrestado = (socioInversion.CantidadPrestada / socioInversion.Cantidad) * 100;
                            socioInversion.NoPrestado -= Math.Round(ultimoMovimiento.Cantidad, 2);

                            ultimoMovimiento.IsSoftDeleted = true;
                            ultimoMovimiento.Enabled = false;
                            ultimoMovimiento.ModifiedDate = DateTime.Now;
                            ultimoMovimiento.ModifiedBy = modifiedBy;

                            if (socioInversion.CantidadPrestada < 0 || socioInversion.PorcetajePrestado < 0 || detalleS.CantidadPagadaDePrestamo < 0)
                            {
                                return AppResult.New(false, $"ERROR: discripancia en cantidades de DetalleInversion {socioInversion.CodigoInversion}");
                            }

                            if (socioInversion.PorcetajePrestado < 100 && socioInversion.Estado != EstadoInversion.EnCaja)
                            {
                                socioInversion.Estado = EstadoInversion.EnCaja;
                                socioInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)socioInversion.Estado);
                            }

                            if (socioInversion.PorcetajePrestado == 100)
                            {
                                socioInversion.Estado = EstadoInversion.Agotado;
                                socioInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)socioInversion.Estado);
                            }

                        }




                    }

                    #endregion




                    #region proceso viejo de Caja

                    //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                    //if (ultimoRegistroCaja != null)
                    //{
                    //    var nuevoSaldo = ultimoRegistroCaja.SaldoActual - newEgreso.Monto;
                    //    if(nuevoSaldo < 0)
                    //    {
                    //        return AppResult.New(false, $"No hay dinero en Caja para sacar {prestamoDetalle.TotalAPagar.ToString("N2")} {prestamo.Moneda}");
                    //    }

                    //    var agregarACaja = Caja.NewEgreso(modifiedBy, newEgreso.Correlativo, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newEgreso.Id, modifiedBy);

                    //    ultimoRegistroCaja.Enabled = false;
                    //    ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                    //    ultimoRegistroCaja.ModifiedBy = modifiedBy;

                    //    await _context.Caja.AddAsync(agregarACaja);
                    //    await _context.SaveChangesAsync();




                    //    #region 

                    //    var calculosNuevasGanancias = await CalculoGananciaSocioDetalle(prestamoDetalle.Id, prestamo.Id, modifiedBy);
                    //    if (calculosNuevasGanancias.Any())
                    //    {
                    //        //await _context.GananciaDetalleSocio.AddRangeAsync(calculosNuevasGanancias);
                    //        await _context.SaveChangesAsync();
                    //    }

                    //    if (calculosNuevasGanancias.Any())
                    //    {
                    //        var calculosGananciasSocios = await CalculoGananciaSocios(prestamo.Id, modifiedBy);
                    //        await _context.SaveChangesAsync();
                    //    }


                    //    #endregion





                    //    var prestamosCliente = await _context.Prestamo.Where(x => x.ClienteId == prestamo.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                    //    if (prestamosCliente.Count == 0)
                    //    {
                    //        prestamo.Cliente.PrestamoActivo = false;
                    //        await _context.SaveChangesAsync();
                    //    }

                    //    return AppResult.New(true, $"Couta {ingresoDetalle.Correlativo} eliminado con exito!");
                    //}
                    //else
                    //{
                    //    return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                    //}

                    #endregion




                    //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                    if (caja != null)
                    {
                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;
                        if (nuevoSaldo < 0)
                        {
                            return AppResult.New(false, $"No hay dinero en Caja para sacar {prestamoDetalle.TotalAPagar.ToString("N2")}");
                        }

                        //var agregarACaja = Caja.NewEgreso(modifiedBy, newEgreso.Correlativo, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newEgreso.Id, modifiedBy);

                        //caja.SaldoAnterior = saldoActual;
                        //caja.SaldoActual = nuevoSaldo;
                        //caja.UltimaTransaccionId = newTransaccion.Id;
                        //caja.ModifiedDate = DateTime.Now;
                        //caja.ModifiedBy = modifiedBy;

                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, modifiedBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;


                        //await _context.Caja.AddAsync(agregarACaja);
                        await _context.SaveChangesAsync();




                        #region 

                        var calculosNuevasGanancias = await CalculoGananciaSocioDetalle(prestamoDetalle.Id, prestamo.Id, modifiedBy);
                        if (calculosNuevasGanancias.Any())
                        {
                            //await _context.GananciaDetalleSocio.AddRangeAsync(calculosNuevasGanancias);
                            await _context.SaveChangesAsync();
                        }

                        if (calculosNuevasGanancias.Any())
                        {
                            var calculosGananciasSocios = await CalculoGananciaSocios(prestamo.Id, modifiedBy);
                            await _context.SaveChangesAsync();
                        }


                        #endregion





                        var prestamosCliente = await _context.Prestamo.Where(x => x.ClienteId == prestamo.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                        if (prestamosCliente.Count == 0)
                        {
                            prestamo.Cliente.PrestamoActivo = false;
                            await _context.SaveChangesAsync();
                        }

                        return AppResult.New(true, $"Couta {ingresoDetalle.Correlativo} eliminado con exito!");
                    }
                    else
                    {
                        return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                    }






                }





                public async Task<List<GananciaDetalleSocio>> CalculoGananciaSocioDetalle(Guid prestamoId, Guid prestamoDetalleId, Guid createdBy)
                {
                    var pagosPrestamo = await _context.PrestamoDetalle.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted).ToListAsync();
                    var sumaInteres = pagosPrestamo.Sum(x => x.MontoInteres);

                    var detallesSociosInversiones = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted/* && x.Enabled*/)
                        .Include(x => x.SocioInversion)
                        .Include(x => x.SocioInversion).ThenInclude(s => s.Socio)
                        .ToListAsync();


                    List<GananciaDetalleSocio> nuevasGananciasDetalle = new List<GananciaDetalleSocio>();

                    if (detallesSociosInversiones.Any())
                    {
                        var detallesSociosInversionIds = detallesSociosInversiones.Select(x => x.Id).ToList();
                        var gananciasDetalleActuales = await _context.GananciaDetalleSocio.Where(x => detallesSociosInversionIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted && x.Enabled).ToListAsync();

                        foreach (var detalle in detallesSociosInversiones)
                        {
                            var socio = detalle.SocioInversion.Socio;
                            if(socio == null)
                            {
                                socio = await _context.Socio.Where(x => x.Id == detalle.SocioInversion.SocioId).FirstOrDefaultAsync();
                            }

                            //var newCalculoGanancia = sumaInteres * (detalle.PorcentajeEnPrestamo / 100);  //Este funciona si el socio ganara 100% del interes, pero cuando se crea un Socio se le pone cuando es el % de ganancia
                            var gananciaPrevia = sumaInteres * (detalle.PorcentajeEnPrestamo / 100);
                            var newCalculoGanancia = gananciaPrevia * (socio.PorcentajeGanancia / 100);


                            var newGananciaDetalle = GananciaDetalleSocio.New(detalle.Id, sumaInteres, newCalculoGanancia, createdBy);

                            var gananciaActual = gananciasDetalleActuales.Where(x => x.DetalleSocioInversionId == detalle.Id).OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                            if (gananciaActual != null)
                            {
                                newGananciaDetalle.Retirado = gananciaActual.Retirado;
                                newGananciaDetalle.CantidadRetirada = gananciaActual.CantidadRetirada;
                                newGananciaDetalle.CantidadDisponibleARetirar = newGananciaDetalle.Ganancia - gananciaActual.CantidadRetirada;

                                //Al anterior la deshabulito porque la sustituye la nueva
                                newGananciaDetalle.GananciaDetalleSocioAnteriorId = gananciaActual.Id;
                                gananciaActual.Enabled = false;
                                gananciaActual.ModifiedBy = createdBy;
                                gananciaActual.ModifiedDate = DateTime.Now;
                                gananciaActual.Observacion = "Se elimino PrestamoDetalle: " + prestamoDetalleId.ToString();
                            }

                            await _context.GananciaDetalleSocio.AddAsync(newGananciaDetalle);

                            detalle.GananciaDetalleSocioId = newGananciaDetalle.Id;

                            nuevasGananciasDetalle.Add(newGananciaDetalle);
                        }
                    }


                    return nuevasGananciasDetalle;
                }



                public async Task<List<Guid>> CalculoGananciaSocios(Guid prestamoId, Guid createdBy)
                {

                    var detallesSociosInversiones = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == prestamoId && !x.IsSoftDeleted/* && x.Enabled*/)
                        .Include(x => x.SocioInversion)
                        .AsNoTracking()
                        .ToListAsync();


                    if (detallesSociosInversiones.Any())
                    {
                        var sociosInversionesId = detallesSociosInversiones.Select(x => x.SocioInversionId).ToList();

                        var sociosInversiones = await _context.SocioInversion.Where(x => sociosInversionesId.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();

                        var sociosIds = sociosInversiones.Select(x => x.SocioId).Distinct().ToList();

                        var socios = await _context.Socio.Where(x => sociosIds.Contains(x.Id) && !x.IsSoftDeleted)
                            .Include(x => x.SocioInversiones)
                            .ToListAsync();
                        var sociosInversionesAll = socios.SelectMany(x => x.SocioInversiones).ToList();
                        var sociosInversionAllIds = sociosInversionesAll.Select(x => x.Id).ToList();

                        var detallesAll = await _context.DetalleSocioInversion.Where(x => sociosInversionAllIds.Contains(x.SocioInversionId) && !x.IsSoftDeleted && x.Enabled)
                            //.Include(x => x.GananciaDetalleSocio)
                            .ToListAsync();

                        var detallesAllsIds = detallesAll.Select(x => x.Id).ToList();
                        var ganaciasAll = await _context.GananciaDetalleSocio.Where(x => detallesAllsIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted && x.Enabled)
                            .ToListAsync();

                        foreach (var socio in socios)
                        {
                            var inversiones = socio.SocioInversiones.Where(x => x.Enabled).ToList();

                            var sociosInversionIds = inversiones.Select(x => x.Id).ToList();

                            var detalles = detallesAll.Where(x => sociosInversionIds.Contains(x.SocioInversionId)).ToList();

                            if (detalles.Any())
                            {
                                decimal gananciaSocio = 0M;
                                //decimal retirosSocio = 0M;      //Esta no la puse, porque seria en el otro modulo retiros mejor

                                foreach (var inversion in inversiones)
                                {
                                    var detallesInversiones = inversion.DetallesSocioInversion.Where(s => s.Enabled).ToList();
                                    var detallesIds = detallesInversiones.Select(x => x.Id).Distinct().ToList();

                                    var gananciaInversion = ganaciasAll.Where(x => detallesIds.Contains(x.DetalleSocioInversionId)).ToList().Sum(x => x.Ganancia);
                                    inversion.Ganancia = gananciaInversion;
                                    inversion.GananciaDisponible = gananciaInversion - inversion.GananciaDisponible;
                                    inversion.ModifiedBy = createdBy;
                                    inversion.ModifiedDate = DateTime.Now;

                                    gananciaSocio += gananciaInversion;
                                }

                                socio.GananciaTotal = gananciaSocio;
                            }


                        }

                        return sociosIds;
                    }


                    return null;
                }







            }


        }

































        public class PrestamoDetalleVm
        {
            public Guid Id { get; set; }
            public int NumeroCuota { get; set; }
            public double MontoCapital { get; set; }
            public double MontoInteres { get; set; }
            public double TotalAPagar { get; set; }
            public double RestaCapital { get; set; }
            public string Moneda_Descripcion { get; set; }
            public DateTime FechaPago { get; set; }
            public double ProximoPago { get; set; }
            public DateTime FechaProximoPago { get; set; }
            public string Observacion { get; set; }

            public Guid PrestamoId { get; set; }
            public Guid ClienteId { get; set; }

            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
        }






    }
}

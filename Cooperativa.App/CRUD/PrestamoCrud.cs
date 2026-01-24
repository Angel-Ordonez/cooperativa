using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Configuraciones;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Engine;
using Cooperativa.App.Utilidades;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.CajaCrud;
using static Cooperativa.App.CRUD.ClienteCrud;
using static Cooperativa.App.CRUD.PrestamoCrud;
using static Cooperativa.App.CRUD.SocioCrud;
using static Cooperativa.App.CRUD.SocioInversionCrud;
using static Cooperativa.App.CRUD.TransaccionCrud;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;
using static Cooperativa.App.Domain.Model.Prestamos.Prestamo;
using static Cooperativa.App.Domain.Model.Prestamos.PrestamoDetalle;

namespace Cooperativa.App.CRUD
{
    public class PrestamoCrud
    {

        public class Create
        {
            public class CommandPrestamoCreate : IRequest<AppResult>
            {
                public Guid ClienteId { get; set; }
                public Guid ResponsableId { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                public string ReferenciaBancaria { get; set; }
                public Guid? RazonId { get; set; }
                public decimal CantidadInicial { get; set; }
                public int TipoPrestamo { get; set; }
                public decimal CuotaMensual { get; set; }
                //public Moneda Moneda { get; set; }
                public decimal InteresPorcentaje { get; set; }
                public int CantidadMeses { get; set; }
                //public double EstimadoAPagarMes { get; set; }  //Hacer un calculo rapido por: Cantidad - %Interes - cantidad de meses
                public string Garantia { get; set; }
                public DateTime FechaEntregado { get; set; }
                //public DateTime FechaEstimadoFinPrestamo { get; set; }
                public string Observacion { get; set; }


                public bool? RequiereDocumento { get; set; }
                public decimal CantidadEnEuro { get; set; }
                public decimal CantidadEnDolar { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoCreate, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;

                }

                public async Task<AppResult> Handle(CommandPrestamoCreate command, CancellationToken cancellationToken)
                {

                    var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                    if (cliente == null)
                    {
                        return AppResult.New(false, $"No existe ClienteId");
                    }

                    var responsable = await _context.Socio.Where(x => x.Id == command.ResponsableId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().FirstOrDefaultAsync();
                    if (responsable == null)
                    {
                        return AppResult.New(false, $"No existe Responsable / Socio");
                    }

                    //La caja sera luego por empresaId
                    var caja = await _context.Caja.Where(x => !x.IsSoftDeleted == x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                    caja.ThrowIfNull("No se encontro Caja para Empresa");


                    //Maximo 2 prestamos activos
                    var tienePrestamos = await _context.Prestamo.Where(x => x.ClienteId == command.ClienteId && x.Estado == EstadoPrestamo.Vigente && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                    if (tienePrestamos.Count() >= 5 )
                    {
                        string codigosPrestamosActivos = "";
                        for(int i=0; i<tienePrestamos.Count; i++)
                        {
                            var p = tienePrestamos.ElementAt(i);
                            codigosPrestamosActivos += " " + p.CodigoPrestamo;

                            if( i != (tienePrestamos.Count() - 1))
                            {
                                codigosPrestamosActivos = codigosPrestamosActivos + " y ";
                            }
                        }

                        return AppResult.New(false, $"El cliente {tienePrestamos.FirstOrDefault().ClienteNombre} tiene el maximo (2) de prestamos vigentes: {codigosPrestamosActivos}");
                    }

                    if(command.CantidadInicial < 1 || command.InteresPorcentaje < 1)
                    {
                         return AppResult.New(false, $"Cantidad o Interes del prestamo no Valido");
                    
                    }

                    var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaDestinoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                    cuentaDestino.ThrowIfNull($"No se encontro cuenta destino");
                    var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaOrigenId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                    cuentaOrigen.ThrowIfNull($"No se encontro cuenta Origen");

                    if(cuentaOrigen.SaldoActual < command.CantidadInicial)
                    {
                        throw new Exception("Cuenta Origen no cuenta con fondos solicitados");
                    }

                    #region CodigoPrestamo   AñoDia-P0004
                    var inicialesNombre = "";
                    if (!string.IsNullOrEmpty(cliente.Nombre) && !string.IsNullOrEmpty(cliente.Apellido))
                    {
                        var primeRLetraNombre = char.ToUpper(cliente.Nombre.Trim()[0]);
                        var primerLetraApellido = char.ToUpper(cliente.Apellido.Trim()[0]);
                        inicialesNombre = primeRLetraNombre.ToString() + primerLetraApellido.ToString();
                    }
                    else
                    {
                        return AppResult.New(false, $"Hubo error al crear el CodigoInversion, Porfavor revise su Nombre y Apellido.");
                    }



                    #region Correlativo antes
                    //var prestamosTotal = await _context.Prestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                    //var numPrestamosTotal = prestamosTotal.Count() + 1;

                    //var prestamos = prestamosTotal.Where(x => !x.IsSoftDeleted && x.TipoPrestamo == (TipoPrestamo)command.TipoPrestamo).ToList();
                    //var codigoPrestamo = "";
                    //bool codigoAprobado = false;
                    //while (!codigoAprobado)
                    //{
                    //    var numPrestamos = prestamos.Count() + 1;
                    //    var nuevoNumero = $"{numPrestamos:D5}";                               //{nuevoNumero:D4}  asegura que el número generado sea formateado con ceros iniciales para ocupar al menos 4 caracteres. Si el número generado es 5, por ejemplo, se formateará como "0005".
                    //    var anio = DateTime.Now.Year % 100;

                    //    if (command.TipoPrestamo == 1)
                    //    {
                    //        //codigoPrestamo = $"PIM-{nuevoNumero}-{anio}";                      //PIM: Prestamo Interes Mensual
                    //        codigoPrestamo = $"PIM{nuevoNumero}{inicialesNombre}{anio}";
                    //    }
                    //    else if (command.TipoPrestamo == 2)
                    //    {
                    //        //codigoPrestamo = $"PCF-{nuevoNumero}-{anio}";                           //PCF: Prestamo Cuota Fija
                    //        codigoPrestamo = $"PCF{nuevoNumero}{inicialesNombre}{anio}";
                    //    }
                    //    else
                    //    {
                    //        throw new Exception("TipoPrestamo no valido.");
                    //    }

                    //    var existe = prestamos.Where(x => x.CodigoPrestamo == codigoPrestamo).Any();
                    //    if (!existe)
                    //    {
                    //        codigoAprobado = true ;
                    //    }
                    //}

                    #endregion



                    var anio = DateTime.Now.Year % 100;
                    var codigoEmpresa = "COOPAZ";

                    var codigoBaseSecuencial = $"PIM-{codigoEmpresa}-{anio}";
                    var secuencial = await _iUtilidadesBase.GenerarSecuencial("Prestamo", codigoBaseSecuencial);
                    if (secuencial <= 0)
                        return AppResult.New(false, "Error al generar secuencial de Prestamo.");

                    var codigoPrestamo = $"PIM{secuencial:D4}{inicialesNombre}{anio}";


                    #endregion

                    var transaction = _context.Database.BeginTransaction();
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var newPrestamo = Prestamo.NewAprobado(secuencial, command.ResponsableId, codigoPrestamo, command.CantidadInicial, command.InteresPorcentaje, command.CantidadMeses,
                            command.Garantia, command.FechaEntregado, command.Observacion, cliente.Id, cliente.Nombre + " " + cliente.Apellido, command.TipoPrestamo, command.CuotaMensual, command.ResponsableId);

                        newPrestamo.RequiereDocumento = command.RequiereDocumento;
                        newPrestamo.CantidadEnDolar = command.CantidadEnDolar;
                        newPrestamo.CantidadEnEuro = command.CantidadEnEuro;
                        newPrestamo.ReferenciaBancaria = command.ReferenciaBancaria;
                        newPrestamo.CuentaBancariaId = cuentaDestino.Id;


                        if (command.TipoPrestamo == 2)
                        {
                            //Cuota Fija
                            newPrestamo.CantidadMeses = command.CantidadMeses;
                            newPrestamo.DebeMeses = command.CantidadMeses;
                            newPrestamo.MesesPagados = 0;
                            newPrestamo.CuotasPagadas = 0;
                            newPrestamo.FechaEstimadoFinPrestamo = DateTime.Now.AddMonths(command.CantidadMeses);
                        }


                        newPrestamo.Ganancia = 0;
                        if (command.InteresPorcentaje > 1)
                        {
                            newPrestamo.EstimadoAPagarMes = (command.InteresPorcentaje / 100) * command.CantidadInicial;
                        }
                        else
                        {
                            newPrestamo.EstimadoAPagarMes = command.InteresPorcentaje * command.CantidadInicial;
                        }


                        await _context.Prestamo.AddAsync(newPrestamo);


                        var egresosCantidad = await _context.Egreso.CountAsync(); //ToListAsync();
                        var newEgreso = Egreso.New(egresosCantidad+1, codigoPrestamo, command.CantidadInicial, "Nuevo Prestamo", $"Nuevo Prestamo {cliente.Nombre} {cliente.Apellido} por {Math.Round(command.CantidadInicial, 2)} HNL ", newPrestamo.Id, createdBy);
                        await _context.Egreso.AddAsync(newEgreso);

                        var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();
                        var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, codigoPrestamo, command.CantidadInicial, cuentaOrigen.Id, cuentaDestino.Id, createdBy);
                        newTransaccion.EgresoId = newEgreso.Id;
                        newTransaccion.CajaId = caja.Id;

                        cuentaOrigen.RestarMovimiento(command.CantidadInicial);             //Caja
                        cuentaDestino.SumarMovimiento(command.CantidadInicial);             //Cliente

                        await _context.Transaccion.AddAsync(newTransaccion);
                        //await _context.SaveChangesAsync();

                        #region Proceso viejo de caja
                        //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                        //if (ultimoRegistroCaja != null)
                        //{
                        //    string saldoEnMoneda = Math.Round(ultimoRegistroCaja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

                        //    if (ultimoRegistroCaja.SaldoActual < command.CantidadInicial)
                        //    {      
                        //        return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {saldoEnMoneda} ");
                        //    }

                        //    var nuevoSaldo = ultimoRegistroCaja.SaldoActual - newEgreso.Monto;

                        //    var agregarACaja = Caja.NewEgreso(createdBy, codigoPrestamo, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newEgreso.Id, createdBy);

                        //    ultimoRegistroCaja.Enabled = false;
                        //    ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                        //    ultimoRegistroCaja.ModifiedBy = createdBy;
                        //    cliente.PrestamoActivo = true;
                        //    cliente.CantidadPrestamos = cliente.CantidadPrestamos + 1;



                        //    #region DetalleSocioInversion

                        //    var configuracionPrestamo = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        //    if (configuracionPrestamo == null) { return AppResult.New(false, "No se encontro ConfiguracionPrestamo"); }     //Es necesario para validar si primero se toma las inversiones de los socios

                        //    var sociosInversion = await _context.SocioInversion.Where(x => !x.IsSoftDeleted && x.Enabled && x.NoPrestado > 0 && x.PorcetajePrestado != 100)
                        //        .Include(x => x.DetallesSocioInversion)
                        //        .OrderBy(x => x.CreatedDate)
                        //        .ToListAsync();
                        //    var cantidadSinPrestarInversio = sociosInversion.Sum(s => s.NoPrestado);
                        //    decimal diferenciaEnCaja = ultimoRegistroCaja.SaldoActual - cantidadSinPrestarInversio;

                        //    var cantidadPrestamo = newPrestamo.CantidadInicial;
                        //    decimal cantidadATrabajar = 0.0M;

                        //    if (cantidadPrestamo > diferenciaEnCaja)
                        //    {
                        //        cantidadATrabajar = cantidadPrestamo - diferenciaEnCaja;        //Es por que el prestamo hagarra dinero de las inversiones, xq no se ajusto lo que habia en caja
                        //    }
                        //    if (configuracionPrestamo.TomarSocioInversion)
                        //    {
                        //        cantidadATrabajar = cantidadPrestamo;                           //En caso que sea prioridad tomar la inversion del socio
                        //    }


                        //    if(configuracionPrestamo != null)
                        //    {
                        //        if (configuracionPrestamo.TomarSocioInversion || cantidadATrabajar > 0) //Si esta en true, es porque tomara de la inversion de un socio (en orden) y no cualquier cantidad de caja...
                        //        {
                        //            List<DetalleSocioInversion> detalleSocioInversiones = new List<DetalleSocioInversion>();

                        //            int centinelaStop = 0;
                        //            while (cantidadATrabajar > 0)
                        //            {
                        //                var sInversion = sociosInversion.Where(x => x.NoPrestado != 0).FirstOrDefault();
                        //                if (sInversion != null)
                        //                {
                        //                    //Aqui seria ir viendo como ir tomando la cantidad por socio y si se va a ocupar mas de un socio
                        //                    decimal habia = 0;
                        //                    decimal sePresto = 0;
                        //                    decimal quedan = 0;
                        //                    habia = sInversion.NoPrestado;

                        //                    quedan = sInversion.NoPrestado - cantidadATrabajar;
                        //                    if (quedan < 0)
                        //                    {
                        //                        sePresto = sInversion.NoPrestado;
                        //                        quedan = 0;
                        //                    }
                        //                    else
                        //                    {
                        //                        sePresto = habia - quedan;
                        //                    }

                        //                    var newDetalleSocioInversion = DetalleSocioInversion.New(sInversion.Id, newPrestamo.Id, habia, sePresto, quedan, createdBy);
                        //                    newDetalleSocioInversion.PorcentajeEnPrestamo = (sePresto / command.CantidadInicial) * 100;     //Que porcentaje ocupo en cantidad dentro del prestamo (Esto se rondea)
                        //                    detalleSocioInversiones.Add(newDetalleSocioInversion);

                        //                    sInversion.NoPrestado = quedan;
                        //                    sInversion.CantidadPrestada += sePresto;
                        //                    sInversion.Movimientos += 1;
                        //                    sInversion.PorcetajePrestado = (sInversion.CantidadPrestada / sInversion.Cantidad) * 100;
                        //                    if(sInversion.PorcetajePrestado == 100)
                        //                    {
                        //                        sInversion.Estado = EstadoInversion.Agotado;
                        //                        sInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)sInversion.Estado);
                        //                    }

                        //                    cantidadATrabajar -= sePresto;
                        //                }
                        //                else
                        //                {
                        //                    break;          //Es porque ya no habian mas Inversiones de Socios para trabajar... Dinero si hay porque todo tipo de ingreso se guarda en caja
                        //                }



                        //                centinelaStop++;
                        //                if (centinelaStop == 15)
                        //                {
                        //                    return AppResult.New(false, "Error en While");
                        //                }

                        //            }

                        //            if (detalleSocioInversiones.Any())
                        //            {
                        //                await _context.DetalleSocioInversion.AddRangeAsync(detalleSocioInversiones);
                        //            }

                        //        }
                        //    }
                        //    #endregion






                        //    //await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();
                        //    transaction.Commit();

                        //    return AppResult.New(true, $"Prestamo: {newPrestamo.CodigoPrestamo}");
                        //}
                        //else
                        //{
                        //    //throw new ArgumentNullException($"No se encontro Caja activa!");
                        //    return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                        //}


                        #endregion




                        if (caja != null)
                        {
                            string saldoEnMoneda = Math.Round(caja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

                            if (caja.SaldoActual < command.CantidadInicial)
                            {
                                return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {saldoEnMoneda} ");
                            }
                            var saldoActual = caja.SaldoActual;




                            #region DetalleSocioInversion

                            var configuracionPrestamo = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                            if (configuracionPrestamo == null) { return AppResult.New(false, "No se encontro ConfiguracionPrestamo"); }     //Es necesario para validar si primero se toma las inversiones de los socios

                            var sociosInversion = await _context.SocioInversion.Where(x => !x.IsSoftDeleted && x.Enabled && x.NoPrestado > 0 && x.PorcetajePrestado != 100)
                                .Include(x => x.DetallesSocioInversion)
                                .OrderBy(x => x.CreatedDate)
                                .ToListAsync();
                            var cantidadSinPrestarInversio = sociosInversion.Sum(s => s.NoPrestado);
                            decimal diferenciaEnCaja = saldoActual - cantidadSinPrestarInversio;

                            var cantidadPrestamo = newPrestamo.CantidadInicial;
                            decimal cantidadATrabajar = 0.0M;

                            if (cantidadPrestamo > diferenciaEnCaja)
                            {
                                cantidadATrabajar = cantidadPrestamo - diferenciaEnCaja;        //Es por que el prestamo hagarra dinero de las inversiones, xq no se ajusto lo que habia en caja
                            }
                            if (configuracionPrestamo.TomarSocioInversion)
                            {
                                cantidadATrabajar = cantidadPrestamo;                           //En caso que sea prioridad tomar la inversion del socio
                            }


                            if (configuracionPrestamo != null)
                            {
                                if (configuracionPrestamo.TomarSocioInversion || cantidadATrabajar > 0) //Si esta en true, es porque tomara de la inversion de un socio (en orden) y no cualquier cantidad de caja...
                                {
                                    List<DetalleSocioInversion> detalleSocioInversiones = new List<DetalleSocioInversion>();

                                    int centinelaStop = 0;
                                    while (cantidadATrabajar > 0)
                                    {
                                        var sInversion = sociosInversion.Where(x => x.NoPrestado != 0).FirstOrDefault();
                                        if (sInversion != null)
                                        {
                                            //Aqui seria ir viendo como ir tomando la cantidad por socio y si se va a ocupar mas de un socio
                                            decimal habia = 0;
                                            decimal sePresto = 0;
                                            decimal quedan = 0;
                                            habia = sInversion.NoPrestado;

                                            quedan = sInversion.NoPrestado - cantidadATrabajar;
                                            if (quedan < 0)
                                            {
                                                sePresto = sInversion.NoPrestado;
                                                quedan = 0;
                                            }
                                            else
                                            {
                                                sePresto = habia - quedan;
                                            }

                                            var newDetalleSocioInversion = DetalleSocioInversion.New(sInversion.Id, newPrestamo.Id, habia, sePresto, quedan, createdBy);
                                            newDetalleSocioInversion.PorcentajeEnPrestamo = (sePresto / command.CantidadInicial) * 100;     //Que porcentaje ocupo en cantidad dentro del prestamo (Esto se rondea)
                                            detalleSocioInversiones.Add(newDetalleSocioInversion);

                                            sInversion.NoPrestado = quedan;
                                            sInversion.CantidadPrestada += sePresto;
                                            sInversion.Movimientos += 1;
                                            sInversion.PorcetajePrestado = (sInversion.CantidadPrestada / sInversion.CantidadActiva) * 100;
                                            if (sInversion.PorcetajePrestado == 100)
                                            {
                                                sInversion.Estado = EstadoInversion.Agotado;
                                                sInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)sInversion.Estado);
                                            }

                                            cantidadATrabajar -= sePresto;
                                        }
                                        else
                                        {
                                            break;          //Es porque ya no habian mas Inversiones de Socios para trabajar... Dinero si hay porque todo tipo de ingreso se guarda en caja
                                        }



                                        centinelaStop++;
                                        if (centinelaStop == 15)
                                        {
                                            return AppResult.New(false, "Error en While");
                                        }

                                    }

                                    if (detalleSocioInversiones.Any())
                                    {
                                        await _context.DetalleSocioInversion.AddRangeAsync(detalleSocioInversiones);
                                    }

                                }
                            }
                            #endregion



                            var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;


                            caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, createdBy);
                            newTransaccion.SaldoCajaEnElMomento = saldoActual;
                            newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                            cliente.PrestamoActivo = true;
                            cliente.CantidadPrestamos = cliente.CantidadPrestamos + 1;


                            //await _context.Caja.AddAsync(agregarACaja);
                            await _context.SaveChangesAsync();
                            transaction.Commit();

                            return AppResult.New(true, $"Prestamo: {newPrestamo.CodigoPrestamo}");
                        }
                        else
                        {
                            //throw new ArgumentNullException($"No se encontro Caja activa!");
                            return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                        }








                    }
                    catch (Exception e)
                    {
                        // Si se produce una excepción, deshaz todas las operaciones de la transacción
                        transaction.Rollback();

                        return AppResult.New(false, e.Message);
                    }




                    //await _context.SaveChangesAsync();


                }


            }


        }





        public class CrearPrestamoPIM
        {
            public class CommandPrestamoCreate : IRequest<AppResult>
            {
                public Guid ClienteId { get; set; }
                public Guid ResponsableId { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                public string ReferenciaBancaria { get; set; }
                public Guid? RazonId { get; set; }
                public decimal CantidadInicial { get; set; }
                public int TipoPrestamo { get; set; }
                public decimal CuotaMensual { get; set; }
                //public Moneda Moneda { get; set; }
                public decimal InteresPorcentaje { get; set; }
                public int CantidadMeses { get; set; }
                //public double EstimadoAPagarMes { get; set; }  //Hacer un calculo rapido por: Cantidad - %Interes - cantidad de meses
                public string Garantia { get; set; }
                public DateTime? FechaEntregado { get; set; }
                //public DateTime FechaEstimadoFinPrestamo { get; set; }
                public string Observacion { get; set; }


                public bool? RequiereDocumento { get; set; }
                public decimal CantidadEnEuro { get; set; }
                public decimal CantidadEnDolar { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoCreate, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;

                }

                public async Task<AppResult> Handle(CommandPrestamoCreate command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var cliente = await _context.Cliente.Where(x => x.Id == command.ClienteId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        cliente.ThrowIfNull("Cliente no existe");

                        var responsable = await _context.Socio.Where(x => x.Id == command.ResponsableId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().FirstOrDefaultAsync();
                        responsable.ThrowIfNull("No existe Responsable / Socio");


                        //La caja sera luego por empresaId
                        var caja = await _context.Caja.Where(x => !x.IsSoftDeleted == x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                        caja.ThrowIfNull("No se encontro Caja para Empresa");


                        //Maximo 2 prestamos activos
                        var tienePrestamos = await _context.Prestamo.Where(x => x.ClienteId == command.ClienteId && x.Estado == EstadoPrestamo.Vigente && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();





                        #region CodigoPrestamo   AñoDia-P0004
                        var inicialesNombre = "";
                        if (!string.IsNullOrEmpty(cliente.Nombre) && !string.IsNullOrEmpty(cliente.Apellido))
                        {
                            var primeRLetraNombre = char.ToUpper(cliente.Nombre.Trim()[0]);
                            var primerLetraApellido = char.ToUpper(cliente.Apellido.Trim()[0]);
                            inicialesNombre = primeRLetraNombre.ToString() + primerLetraApellido.ToString();
                        }
                        else
                        {
                            return AppResult.New(false, $"Hubo error al crear el CodigoInversion, Porfavor revise su Nombre y Apellido.");
                        }



                        var anio = DateTime.Now.Year % 100;
                        var codigoEmpresa = "COOPAZ";

                        var codigoBaseSecuencial = $"PIM-{codigoEmpresa}-{anio}";
                        var secuencial = await _iUtilidadesBase.GenerarSecuencial("Prestamo", codigoBaseSecuencial);
                        if (secuencial <= 0)
                            return AppResult.New(false, "Error al generar secuencial de Prestamo.");

                        var codigoPrestamo = $"PIM{secuencial:D4}{inicialesNombre}{anio}";


                        #endregion

                        
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");



                        if (command.RequiereDocumento == false)
                        {
                            //Fue un prestamo directo sin necesidad de aprobacion


                            var configuracionPrestamo = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                            if (tienePrestamos.Count() >= configuracionPrestamo.CantidadPrestamosPorCliente)
                            {
                                string codigosPrestamosActivos = "";
                                for (int i = 0; i < tienePrestamos.Count; i++)
                                {
                                    var p = tienePrestamos.ElementAt(i);
                                    codigosPrestamosActivos += " " + p.CodigoPrestamo;

                                    if (i != (tienePrestamos.Count() - 1))
                                    {
                                        codigosPrestamosActivos = codigosPrestamosActivos + " y ";
                                    }
                                }

                                return AppResult.New(false, $"El cliente {tienePrestamos.FirstOrDefault().ClienteNombre} tiene el maximo {configuracionPrestamo.CantidadPrestamosPorCliente} de prestamos vigentes: {codigosPrestamosActivos}");
                            }

                            if (command.CantidadInicial < 1 || command.InteresPorcentaje < 1)
                            {
                                return AppResult.New(false, $"Cantidad o Interes del prestamo no Valido");

                            }

                            var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaDestinoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            cuentaDestino.ThrowIfNull($"No se encontro cuenta destino");
                            var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaOrigenId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            cuentaOrigen.ThrowIfNull($"No se encontro cuenta Origen");

                            if (cuentaOrigen.SaldoActual < command.CantidadInicial)
                            {
                                throw new Exception("Cuenta Origen no cuenta con fondos solicitados");
                            }



                            var newPrestamo = Prestamo.NewAprobado(secuencial, command.ResponsableId, codigoPrestamo, command.CantidadInicial, command.InteresPorcentaje, command.CantidadMeses,
                                command.Garantia, (DateTime)command.FechaEntregado, command.Observacion, cliente.Id, cliente.Nombre + " " + cliente.Apellido, command.TipoPrestamo, command.CuotaMensual, command.ResponsableId);

                            newPrestamo.RequiereDocumento = command.RequiereDocumento;
                            newPrestamo.CantidadEnDolar = command.CantidadEnDolar;
                            newPrestamo.CantidadEnEuro = command.CantidadEnEuro;
                            newPrestamo.ReferenciaBancaria = command.ReferenciaBancaria;
                            newPrestamo.CuentaBancariaId = cuentaDestino.Id;


                            if (command.TipoPrestamo == 2)
                            {
                                //Cuota Fija
                                newPrestamo.CantidadMeses = command.CantidadMeses;
                                newPrestamo.DebeMeses = command.CantidadMeses;
                                newPrestamo.MesesPagados = 0;
                                newPrestamo.CuotasPagadas = 0;
                                newPrestamo.FechaEstimadoFinPrestamo = DateTime.Now.AddMonths(command.CantidadMeses);
                            }


                            newPrestamo.Ganancia = 0;
                            if (command.InteresPorcentaje > 1)
                            {
                                newPrestamo.EstimadoAPagarMes = (command.InteresPorcentaje / 100) * command.CantidadInicial;
                            }
                            else
                            {
                                newPrestamo.EstimadoAPagarMes = command.InteresPorcentaje * command.CantidadInicial;
                            }


                            await _context.Prestamo.AddAsync(newPrestamo);


                            var egresosCantidad = await _context.Egreso.CountAsync(); //ToListAsync();
                            var newEgreso = Egreso.New(egresosCantidad + 1, codigoPrestamo, command.CantidadInicial, "Nuevo Prestamo", $"Nuevo Prestamo {cliente.Nombre} {cliente.Apellido} por {Math.Round(command.CantidadInicial, 2)} HNL ", newPrestamo.Id, createdBy);
                            await _context.Egreso.AddAsync(newEgreso);

                            var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();
                            var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, codigoPrestamo, command.CantidadInicial, cuentaOrigen.Id, cuentaDestino.Id, createdBy);
                            newTransaccion.EgresoId = newEgreso.Id;
                            newTransaccion.CajaId = caja.Id;

                            cuentaOrigen.RestarMovimiento(command.CantidadInicial);             //Caja
                            cuentaDestino.SumarMovimiento(command.CantidadInicial);             //Cliente

                            await _context.Transaccion.AddAsync(newTransaccion);
                            //await _context.SaveChangesAsync();




                            if (caja != null)
                            {
                                string saldoEnMoneda = Math.Round(caja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

                                if (caja.SaldoActual < command.CantidadInicial)
                                {
                                    return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {saldoEnMoneda} ");
                                }
                                var saldoActual = caja.SaldoActual;




                                #region DetalleSocioInversion


                                if (configuracionPrestamo == null) { return AppResult.New(false, "No se encontro ConfiguracionPrestamo"); }     //Es necesario para validar si primero se toma las inversiones de los socios

                                var sociosInversion = await _context.SocioInversion.Where(x => !x.IsSoftDeleted && x.Enabled && x.NoPrestado > 0 && x.PorcetajePrestado != 100)
                                    .Include(x => x.DetallesSocioInversion)
                                    .OrderBy(x => x.CreatedDate)
                                    .ToListAsync();
                                var cantidadSinPrestarInversio = sociosInversion.Sum(s => s.NoPrestado);
                                decimal diferenciaEnCaja = saldoActual - cantidadSinPrestarInversio;

                                var cantidadPrestamo = newPrestamo.CantidadInicial;
                                decimal cantidadATrabajar = 0.0M;

                                if (cantidadPrestamo > diferenciaEnCaja)
                                {
                                    cantidadATrabajar = cantidadPrestamo - diferenciaEnCaja;        //Es por que el prestamo hagarra dinero de las inversiones, xq no se ajusto lo que habia en caja
                                }
                                if (configuracionPrestamo.TomarSocioInversion)
                                {
                                    cantidadATrabajar = cantidadPrestamo;                           //En caso que sea prioridad tomar la inversion del socio
                                }


                                if (configuracionPrestamo != null)
                                {
                                    if (configuracionPrestamo.TomarSocioInversion || cantidadATrabajar > 0) //Si esta en true, es porque tomara de la inversion de un socio (en orden) y no cualquier cantidad de caja...
                                    {
                                        List<DetalleSocioInversion> detalleSocioInversiones = new List<DetalleSocioInversion>();

                                        int centinelaStop = 0;
                                        while (cantidadATrabajar > 0)
                                        {
                                            var sInversion = sociosInversion.Where(x => x.NoPrestado != 0).FirstOrDefault();
                                            if (sInversion != null)
                                            {
                                                //Aqui seria ir viendo como ir tomando la cantidad por socio y si se va a ocupar mas de un socio
                                                decimal habia = 0;
                                                decimal sePresto = 0;
                                                decimal quedan = 0;
                                                habia = sInversion.NoPrestado;

                                                quedan = sInversion.NoPrestado - cantidadATrabajar;
                                                if (quedan < 0)
                                                {
                                                    sePresto = sInversion.NoPrestado;
                                                    quedan = 0;
                                                }
                                                else
                                                {
                                                    sePresto = habia - quedan;
                                                }

                                                var newDetalleSocioInversion = DetalleSocioInversion.New(sInversion.Id, newPrestamo.Id, habia, sePresto, quedan, createdBy);
                                                newDetalleSocioInversion.PorcentajeEnPrestamo = (sePresto / command.CantidadInicial) * 100;     //Que porcentaje ocupo en cantidad dentro del prestamo (Esto se rondea)
                                                detalleSocioInversiones.Add(newDetalleSocioInversion);

                                                sInversion.NoPrestado = quedan;
                                                sInversion.CantidadPrestada += sePresto;
                                                sInversion.Movimientos += 1;
                                                sInversion.PorcetajePrestado = (sInversion.CantidadPrestada / sInversion.CantidadActiva) * 100;
                                                if (sInversion.PorcetajePrestado == 100)
                                                {
                                                    sInversion.Estado = EstadoInversion.Agotado;
                                                    sInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)sInversion.Estado);
                                                }

                                                cantidadATrabajar -= sePresto;
                                            }
                                            else
                                            {
                                                break;          //Es porque ya no habian mas Inversiones de Socios para trabajar... Dinero si hay porque todo tipo de ingreso se guarda en caja
                                            }



                                            centinelaStop++;
                                            if (centinelaStop == 15)
                                            {
                                                return AppResult.New(false, "Error en While");
                                            }

                                        }

                                        if (detalleSocioInversiones.Any())
                                        {
                                            await _context.DetalleSocioInversion.AddRangeAsync(detalleSocioInversiones);
                                        }

                                    }
                                }
                                #endregion



                                var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;


                                caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, createdBy);
                                newTransaccion.SaldoCajaEnElMomento = saldoActual;
                                newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                                cliente.PrestamoActivo = true;
                                cliente.CantidadPrestamos = cliente.CantidadPrestamos + 1;


                                //await _context.Caja.AddAsync(agregarACaja);
                                await _context.SaveChangesAsync();

                                return AppResult.New(true, $"Prestamo: {newPrestamo.CodigoPrestamo}");
                            }
                            else
                            {
                                //throw new ArgumentNullException($"No se encontro Caja activa!");
                                return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                            }








                        }
                        else
                        {
                            //Fue un prestamo directo sin necesidad de aprobacion

                            var newPrestamo = Prestamo.NewPendienteAprobacion(secuencial, command.ResponsableId, codigoPrestamo, command.CantidadInicial, command.InteresPorcentaje, command.CantidadMeses,
                                command.Garantia, command.Observacion, cliente.Id, cliente.Nombre + " " + cliente.Apellido, command.TipoPrestamo, command.CuotaMensual, command.ResponsableId);

                            newPrestamo.RequiereDocumento = command.RequiereDocumento;
                            newPrestamo.CantidadEnDolar = command.CantidadEnDolar;
                            newPrestamo.CantidadEnEuro = command.CantidadEnEuro;
                            newPrestamo.ReferenciaBancaria = command.ReferenciaBancaria;

                            if (command.TipoPrestamo == 2)
                            {
                                //Cuota Fija
                                newPrestamo.CantidadMeses = command.CantidadMeses;
                                newPrestamo.DebeMeses = command.CantidadMeses;
                                newPrestamo.MesesPagados = 0;
                                newPrestamo.CuotasPagadas = 0;
                                newPrestamo.FechaEstimadoFinPrestamo = DateTime.Now.AddMonths(command.CantidadMeses);
                            }


                            newPrestamo.Ganancia = 0;
                            if (command.InteresPorcentaje > 1)
                            {
                                newPrestamo.EstimadoAPagarMes = (command.InteresPorcentaje / 100) * command.CantidadInicial;
                            }
                            else
                            {
                                newPrestamo.EstimadoAPagarMes = command.InteresPorcentaje * command.CantidadInicial;
                            }



                            await _context.Prestamo.AddAsync(newPrestamo);

                            await _context.SaveChangesAsync();

                            return AppResult.New(true, $"Prestamo: {newPrestamo.CodigoPrestamo}");
                        }



                    }
                    catch (Exception e)
                    {
                        return AppResult.New(false, e.Message);
                    }




                }
            }
        }







        public class AtenderPrestamoPendiente
        {
            public class CommandPrestamo : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public EstadoPrestamo Estado { get; set; }
                public DateTime? FechaEntregado { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                public string ReferenciaBancaria { get; set; }
                public IFormFile Archivo { get; set; }   // imagen o pdf
                public string Observacion { get; set; }
                public Guid? UsuarioId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamo, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandPrestamo command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var modifiedBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var prestamo = await _context.Prestamo.Where(x => x.Id == command.Id && !x.IsSoftDeleted && x.Enabled)
                            .Include(x => x.Cliente)
                            .FirstOrDefaultAsync();
                        prestamo.ThrowIfNull($"No existe Prestamo");

                        var cliente = prestamo.Cliente;
                        cliente.ThrowIfNull("No se encontro Cliente");

                        if (prestamo.Estado != EstadoPrestamo.PendienteAprobacion)
                        {
                            throw new Exception($"Prestamo esta en estado {prestamo.Estado_Descripcion} y no en Pendiente Aprobacion");
                        }



                        //Aqui debo controlar FechaEntregado esta fecha seria como fecha de aprobado


                        if(command.Estado == EstadoPrestamo.Vigente)
                        {
                            //La caja sera luego por empresaId
                            //var caja = await _context.Caja.Where(x => !x.IsSoftDeleted == x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                            //caja.ThrowIfNull("No se encontro Caja para Empresa");


                            var configuracionPrestamo = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();


                            if (prestamo.CantidadInicial < 1 || prestamo.InteresPorcentaje < 1)
                            {
                                return AppResult.New(false, $"Cantidad o Interes del prestamo no Valido");

                            }

                            var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaDestinoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            cuentaDestino.ThrowIfNull($"No se encontro cuenta destino");
                            var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == command.CuentaBancariaOrigenId && !x.IsSoftDeleted)
                                .Include(x => x.Caja)
                                .Include(x => x.InstitucionBancaria)
                                .FirstOrDefaultAsync();
                            cuentaOrigen.ThrowIfNull($"No se encontro cuenta Origen");

                            var caja = cuentaOrigen.Caja;
                            caja.ThrowIfNull($"No se encontro Caja relacionada a cuenta Origen");

                            if (caja.SaldoActual < prestamo.CantidadInicial)
                            {
                                return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {caja.SaldoActual:N2} ");
                            }
                            if (cuentaOrigen.SaldoActual < prestamo.CantidadInicial)
                            {
                                throw new Exception("Cuenta Origen no cuenta con fondos solicitados");
                            }

                            #region Validar Fecha de entrega
                            var hoy = DateTime.Now;
                            if (command.FechaEntregado == null || command.FechaEntregado == DateTime.MinValue || command.FechaEntregado < hoy)
                            {
                                if(command.FechaEntregado < hoy)
                                {
                                    throw new Exception("Fecha de entrega no puede ser menor que hoy");
                                }
                                else
                                {
                                    throw new Exception("Fecha de entrega no puede ser vacia");
                                }
                            }

                            #endregion

                            #region Validar congruencia entre Cuentas Bancarias

                            bool origenEsEfectivo =
                                cuentaOrigen.InstitucionBancaria?.Nombre?.Contains("efectivo", StringComparison.OrdinalIgnoreCase) == true ||
                                cuentaOrigen.NumeroCuenta?.Contains("efectivo", StringComparison.OrdinalIgnoreCase) == true ||
                                cuentaOrigen.NumeroTarjeta?.Contains("efectivo", StringComparison.OrdinalIgnoreCase) == true;

                            bool destinoEsEfectivo =
                                cuentaDestino.InstitucionBancaria?.Nombre?.Contains("efectivo", StringComparison.OrdinalIgnoreCase) == true ||
                                cuentaDestino.NumeroCuenta?.Contains("efectivo", StringComparison.OrdinalIgnoreCase) == true ||
                                cuentaDestino.NumeroTarjeta?.Contains("efectivo", StringComparison.OrdinalIgnoreCase) == true;

                            // Regla 1: No permitido banca -> efectivo
                            if (!origenEsEfectivo && destinoEsEfectivo)
                            {
                                throw new Exception("No es permitido una transaccion entre cuenta Origen (Banca) y destino (Efectivo)");
                            }

                            // Regla 2: Referencia obligatoria en cualquier transacción que NO sea efectivo -> efectivo
                            if (!(origenEsEfectivo && destinoEsEfectivo))
                            {
                                if (string.IsNullOrWhiteSpace(command.ReferenciaBancaria))
                                {
                                    throw new Exception("Referencia Bancaria es obligatoria");
                                }
                            }

                            #endregion



                            var newEgreso = Egreso.New(0, prestamo.CodigoPrestamo, prestamo.CantidadInicial, "Nuevo Prestamo", $"Nuevo prestamo aprobado {prestamo.ClienteNombre} por L {Math.Round(prestamo.CantidadInicial, 2)} ", prestamo.Id, modifiedBy);
                            await _context.Egreso.AddAsync(newEgreso);

                            var numTransacciones = await _context.Transaccion.AsNoTracking().ToListAsync();
                            var newTransaccion = Transaccion.New(numTransacciones.Count() + 1, prestamo.CodigoPrestamo, prestamo.CantidadInicial, cuentaOrigen.Id, cuentaDestino.Id, modifiedBy);
                            newTransaccion.EgresoId = newEgreso.Id;
                            newTransaccion.CajaId = caja.Id;

                            cuentaOrigen.RestarMovimiento(prestamo.CantidadInicial);             //Caja
                            cuentaDestino.SumarMovimiento(prestamo.CantidadInicial);             //Cliente

                            await _context.Transaccion.AddAsync(newTransaccion);
                            //await _context.SaveChangesAsync();




                            if (caja != null)
                            {
                                string saldoEnMoneda = Math.Round(caja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

          
                                var saldoActual = caja.SaldoActual;




                                #region DetalleSocioInversion


                                if (configuracionPrestamo == null) { return AppResult.New(false, "No se encontro ConfiguracionPrestamo"); }     //Es necesario para validar si primero se toma las inversiones de los socios

                                var sociosInversion = await _context.SocioInversion.Where(x => !x.IsSoftDeleted && x.Enabled && x.NoPrestado > 0 && x.PorcetajePrestado != 100)
                                    .Include(x => x.DetallesSocioInversion)
                                    .OrderBy(x => x.CreatedDate)
                                    .ToListAsync();
                                var cantidadSinPrestarInversio = sociosInversion.Sum(s => s.NoPrestado);
                                decimal diferenciaEnCaja = saldoActual - cantidadSinPrestarInversio;

                                var cantidadPrestamo = prestamo.CantidadInicial;
                                decimal cantidadATrabajar = 0.0M;

                                if (cantidadPrestamo > diferenciaEnCaja)
                                {
                                    cantidadATrabajar = cantidadPrestamo - diferenciaEnCaja;        //Es por que el prestamo hagarra dinero de las inversiones, xq no se ajusto lo que habia en caja
                                }
                                if (configuracionPrestamo.TomarSocioInversion)
                                {
                                    cantidadATrabajar = cantidadPrestamo;                           //En caso que sea prioridad tomar la inversion del socio
                                }


                                if (configuracionPrestamo != null)
                                {
                                    if (configuracionPrestamo.TomarSocioInversion || cantidadATrabajar > 0) //Si esta en true, es porque tomara de la inversion de un socio (en orden) y no cualquier cantidad de caja...
                                    {
                                        List<DetalleSocioInversion> detalleSocioInversiones = new List<DetalleSocioInversion>();

                                        int centinelaStop = 0;
                                        while (cantidadATrabajar > 0)
                                        {
                                            var sInversion = sociosInversion.Where(x => x.NoPrestado != 0).FirstOrDefault();
                                            if (sInversion != null)
                                            {
                                                //Aqui seria ir viendo como ir tomando la cantidad por socio y si se va a ocupar mas de un socio
                                                decimal habia = 0;
                                                decimal sePresto = 0;
                                                decimal quedan = 0;
                                                habia = sInversion.NoPrestado;

                                                quedan = sInversion.NoPrestado - cantidadATrabajar;
                                                if (quedan < 0)
                                                {
                                                    sePresto = sInversion.NoPrestado;
                                                    quedan = 0;
                                                }
                                                else
                                                {
                                                    sePresto = habia - quedan;
                                                }

                                                var newDetalleSocioInversion = DetalleSocioInversion.New(sInversion.Id, prestamo.Id, habia, sePresto, quedan, modifiedBy);
                                                newDetalleSocioInversion.PorcentajeEnPrestamo = (sePresto / prestamo.CantidadInicial) * 100;     //Que porcentaje ocupo en cantidad dentro del prestamo (Esto se rondea)
                                                detalleSocioInversiones.Add(newDetalleSocioInversion);

                                                sInversion.NoPrestado = quedan;
                                                sInversion.CantidadPrestada += sePresto;
                                                sInversion.Movimientos += 1;
                                                sInversion.PorcetajePrestado = (sInversion.CantidadPrestada / sInversion.CantidadActiva) * 100;
                                                if (sInversion.PorcetajePrestado == 100)
                                                {
                                                    sInversion.Estado = EstadoInversion.Agotado;
                                                    sInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)sInversion.Estado);
                                                }

                                                cantidadATrabajar -= sePresto;
                                            }
                                            else
                                            {
                                                break;          //Es porque ya no habian mas Inversiones de Socios para trabajar... Dinero si hay porque todo tipo de ingreso se guarda en caja
                                            }



                                            centinelaStop++;
                                            if (centinelaStop == 15)
                                            {
                                                return AppResult.New(false, "Error en While");
                                            }

                                        }

                                        if (detalleSocioInversiones.Any())
                                        {
                                            await _context.DetalleSocioInversion.AddRangeAsync(detalleSocioInversiones);
                                        }

                                    }
                                }
                                #endregion



                                var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;


                                caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, modifiedBy);
                                newTransaccion.SaldoCajaEnElMomento = saldoActual;
                                newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                                cliente.PrestamoActivo = true;
                                cliente.CantidadPrestamos = cliente.CantidadPrestamos + 1;

                                prestamo.Estado = command.Estado;
                                prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)command.Estado);
                                prestamo.CuentaBancariaId = cuentaDestino.Id;
                                prestamo.ReferenciaBancaria = command.ReferenciaBancaria;
                                prestamo.ModifiedDate = DateTime.Now;
                                prestamo.ModifiedBy = modifiedBy;
                                prestamo.FechaEntragado = (DateTime)command.FechaEntregado;
                                prestamo.Observacion += " **Prestamo Aprobado";

                                await _context.SaveChangesAsync();

                                return AppResult.New(true, $"Prestamo {prestamo.CodigoPrestamo} aprobado exitosamente");
                            }
                            else
                            {
                                return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                            }


                        }
                        else if(command.Estado == EstadoPrestamo.Rechazado)
                        {
                            prestamo.Estado = command.Estado;
                            prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)command.Estado);
                            prestamo.Observacion += " **Prestamo Rechazado";
                            prestamo.ModifiedBy = modifiedBy;
                            prestamo.ModifiedDate = DateTime.Now;

                            await _context.SaveChangesAsync();

                            return AppResult.New(true, $"Prestamo {prestamo.CodigoPrestamo} rechazado exitosamente");
                        }
                        else
                        {
                            return AppResult.New(false, $"Estado a atender no es valido");
                        }

                    }
                    catch(Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }





        public class GetCuentasBancariasParaPrestamosPendientesAprobacion
        {

            public class CuentasBancariasCajaClienteVm
            {
                public List<CajaVm> Cajas {  get; set; }
                public List<CuentaBancariaVm> CuentasBancariasClientes { get; set; }
            }
            public class Command : IRequest<CuentasBancariasCajaClienteVm>
            {

            }

            public class CommandHandler : IRequestHandler<Command, CuentasBancariasCajaClienteVm>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<CuentasBancariasCajaClienteVm> Handle(Command command, CancellationToken cancellationToken)
                {
                    var modifiedBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                    var prestamos = await _context.Prestamo.Where(x => x.Estado == EstadoPrestamo.PendienteAprobacion && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.Cliente)
                        .ToListAsync();

                    var cajas = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled && x.Modulo == ModuloCaja.Prestamos)
                        .AsNoTracking()
                        .ProjectToType<CajaVm>()
                        .ToListAsync();

                    var clientesIds = prestamos.Select(x => x.ClienteId).Distinct().ToList();

                    var cuentasBancarias = await _context.CuentaBancaria.Where(x => clientesIds.Contains((Guid)x.PersonaId) && !x.IsSoftDeleted && x.Enabled)
                        .ProjectToType<CuentaBancariaVm>()
                        .ToListAsync();


                    var newRespuesta = new CuentasBancariasCajaClienteVm
                    {
                        Cajas = cajas,
                        CuentasBancariasClientes = cuentasBancarias
                    };

                    return newRespuesta;
                }
            }
        }






        public class Delete
        {
            public class CommandPrestamoDelete : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public string Observacion { get; set; }
                //public Guid ModifiedBy { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoDelete, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandPrestamoDelete command, CancellationToken cancellationToken)
                {
                    var modifiedBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                    var prestamo = await _context.Prestamo.Where(x => x.Id == command.Id && x.Estado == EstadoPrestamo.Vigente && !x.IsSoftDeleted && x.Enabled)
                        .Include(x => x.Cliente)
                        .FirstOrDefaultAsync();
                    if (prestamo == null) { return AppResult.New(false, "No se encontro Prestamo Vigente, o ha sido eliminado antes!"); }


                    var egreso = await _context.Egreso.Where(x => x.PrestamoId == prestamo.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                    var transaccion = await _context.Transaccion.Where(x => x.EgresoId == egreso.Id && !x.IsSoftDeleted)
                        .Include(x => x.Caja)
                        .Include(x => x.CuentaBancariaOrigen)
                        .Include(x => x.CuentaBancariaDestino)
                        .FirstOrDefaultAsync();
                    transaccion.ThrowIfNull("No se encontro transaccion");
                    var caja = transaccion.Caja;
                    caja.ThrowIfNull("No se encontro Caja");

                    if (caja.IsSoftDeleted || !caja.Enabled)
                    {
                        throw new Exception("Se esta tomando una Caja eliminada");
                    }

                    var cuentaOrigen = transaccion.CuentaBancariaOrigen;
                    cuentaOrigen.ThrowIfNull("No se encontro CuentaBancariaOrigen");
                    var cuentaDestino = transaccion.CuentaBancariaDestino;
                    cuentaDestino.ThrowIfNull("No se encontro CuentaBancariaDestino");

                    //var cajaPrestamo = await _context.Caja.Where(x => x.Egreso.PrestamoId == prestamo.Id).AsNoTracking().FirstOrDefaultAsync();
                    //if (cajaPrestamo == null)
                    //{
                    //    prestamo.Enabled = false;
                    //    prestamo.ModifiedBy = modifiedBy;
                    //    prestamo.Estado = EstadoPrestamo.Cancelado;
                    //    prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)prestamo.Estado); ;
                    //    await _context.SaveChangesAsync();
                    //    return AppResult.New(false, $"Este Prestamo no se vio afectada en Caja. Se elimino Prestamo {prestamo.CodigoPrestamo}.");
                    //}

                    var transaction = _context.Database.BeginTransaction();

                    try
                    {
                        if (command.Observacion == null || command.Observacion.Count() == 0)
                        {
                            command.Observacion = $"Prestamo Eliminado de {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido} con Cantidad {prestamo.RestaCapital.ToString()} {prestamo.Moneda_Descripcion}. {DateTime.Now.ToString()}";
                        }
                        else
                        {
                            command.Observacion = "Prestamo eliminado. " + command.Observacion;
                        }

                        //Se ingresa dinero a caja
                        var ingresos = await _context.Ingreso.Where(x => !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                        var numeroIngreso = ingresos.Where(x => x.PrestamoId!=null && x.PrestamoId != Guid.Empty).OrderByDescending(x => x.NumeroIngreso).Select(x => x.NumeroIngreso).FirstOrDefault();


                        var newIngreso = Ingreso.NewPorPrestamoEliminado(ingresos.Count()+1, prestamo.CodigoPrestamo, prestamo.RestaCapital, $"Prestamo Eliminado. Se toma en cuenta el RestaCapital", command.Observacion, prestamo.Id, modifiedBy);
                        await _context.Ingreso.AddAsync(newIngreso);


                        var numTransacciones = await _context.Transaccion.AsNoTracking().CountAsync();
                        //var newTransaccion = Transaccion.New(numTransacciones + 1, prestamo.CodigoPrestamo, newIngreso.Monto, prestamo.CuentaBancariaId, modifiedBy);
                        var newTransaccion = Transaccion.New(numTransacciones + 1, prestamo.CodigoPrestamo, newIngreso.Monto, transaccion.CuentaBancariaOrigenId, transaccion.CuentaBancariaDestinoId,  modifiedBy);

                        newTransaccion.IngresoId = newIngreso.Id;
                        newTransaccion.CajaId = caja.Id;

                        cuentaOrigen.SumarMovimiento(newIngreso.Monto);                 //Caja
                        cuentaDestino.RestarMovimiento(newIngreso.Monto);               //Cliente


                        await _context.Transaccion.AddAsync(newTransaccion);





                        #region DetalleSocioInversion

                        var inversionSociosDetalles = await _context.DetalleSocioInversion.Where(x => x.PrestamoId == prestamo.Id && !x.IsSoftDeleted && x.Enabled)
                            .Include(x => x.SocioInversion)
                            .ToListAsync();

                        if (inversionSociosDetalles.Any())
                        {
                            var detallesInversionesIds = inversionSociosDetalles.Select(x => x.Id).ToList();

                            await _context.MovimientoDetalleSocio.Where(x => detallesInversionesIds.Contains(x.DetalleSocioInversionId) && !x.IsSoftDeleted).ToListAsync();


                            foreach (var detalleS in inversionSociosDetalles)
                            {
                                detalleS.IsSoftDeleted = true;
                                detalleS.Enabled = false;
                                detalleS.ModifiedBy = modifiedBy;
                                detalleS.ModifiedDate = DateTime.Now;

                                var socioInversion = detalleS.SocioInversion;

                                var cantidadSumarANoPrestado = detalleS.SePresto - detalleS.CantidadPagadaDePrestamo;

                                //socioInversion.NoPrestado =+ detalleS.SePresto;
                                //socioInversion.CantidadPrestada = socioInversion.CantidadPrestada - detalleS.SePresto;
                                socioInversion.NoPrestado += cantidadSumarANoPrestado;
                                socioInversion.CantidadPrestada = socioInversion.CantidadPrestada - cantidadSumarANoPrestado;
                                socioInversion.PorcetajePrestado = (socioInversion.CantidadPrestada / socioInversion.Cantidad) * 100;
                                socioInversion.Movimientos -= 1;

                                socioInversion.ModifiedBy = modifiedBy;
                                socioInversion.ModifiedDate = DateTime.Now;

                                if(socioInversion.Estado == EstadoInversion.Agotado && socioInversion.PorcetajePrestado < 100)
                                {
                                    socioInversion.Estado = EstadoInversion.EnCaja;
                                    socioInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)socioInversion.Estado);
                                }
                            }
                        }

                        #endregion


                        #region Proceso viejo caja
                        //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();


                        //if (ultimoRegistroCaja != null)
                        //{


                        //    var nuevoSaldo = ultimoRegistroCaja.SaldoActual + newIngreso.Monto;

                        //    var agregarACaja = Caja.NewIngreso(modifiedBy, prestamo.CodigoPrestamo, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newIngreso.Id, modifiedBy);

                        //    ultimoRegistroCaja.Enabled = false;
                        //    ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                        //    ultimoRegistroCaja.ModifiedBy = modifiedBy;

                        //    prestamo.Enabled = false;
                        //    prestamo.ModifiedBy = modifiedBy;
                        //    prestamo.Observacion = prestamo.Observacion + "/ " + command.Observacion.ToString();
                        //    prestamo.Estado = EstadoPrestamo.Cancelado;
                        //    prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)prestamo.Estado);



                        //    await _context.Caja.AddAsync(agregarACaja);
                        //    await _context.SaveChangesAsync();



                        //    var prestamosCliente = await _context.Prestamo.Where(x => x.ClienteId == prestamo.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                        //    if (prestamosCliente.Count == 0)
                        //    {
                        //        prestamo.Cliente.PrestamoActivo = false;
                        //        await _context.SaveChangesAsync();
                        //    }


                        //    transaction.Commit();

                        //    return AppResult.New(true, $"Inversion {prestamo.CodigoPrestamo} cancelado con exito!");
                        //}
                        //else
                        //{

                        //    return AppResult.New(false, "No se encontro Caja activa con saldo!!!");
                        //}
                        #endregion


                        var saldoActual = caja.SaldoActual;
                        var nuevoSaldo = caja.SaldoActual + newIngreso.Monto;


                        //caja.SaldoAnterior = saldoActual;
                        //caja.SaldoActual = nuevoSaldo;
                        //caja.UltimaTransaccionId = newTransaccion.Id;
                        //caja.ModifiedDate = DateTime.Now;
                        //caja.ModifiedBy = modifiedBy;

                        caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, modifiedBy);
                        newTransaccion.SaldoCajaEnElMomento = saldoActual;
                        newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;

                        prestamo.Enabled = false;
                        prestamo.ModifiedBy = modifiedBy;
                        prestamo.Observacion = prestamo.Observacion + "/ " + command.Observacion.ToString();
                        prestamo.Estado = EstadoPrestamo.Cancelado;
                        prestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)prestamo.Estado);

                        await _context.SaveChangesAsync();



                        var prestamosCliente = await _context.Prestamo.Where(x => x.ClienteId == prestamo.ClienteId && !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                        if (prestamosCliente.Count == 0)
                        {
                            prestamo.Cliente.PrestamoActivo = false;
                            await _context.SaveChangesAsync();
                        }


                        transaction.Commit();

                        return AppResult.New(true, $"Inversion {prestamo.CodigoPrestamo} cancelado con exito!");





                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        return AppResult.New(false, $"ERROR: {e.Message}");
                    }


                }


            }


        }



 




        public class RecalcularCantidadCuotas       //Solo lo ocupe para una prueba
        {
            public class CommandPrestamoRe : IRequest<AppResult>
            {
                public string Observacion { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandPrestamoRe, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandPrestamoRe command, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted).ToListAsync();
                    var prestamosIds = prestamos.Select(x => x.Id).ToList();
                    var detallesPrestamos = await _context.PrestamoDetalle.Where(x => prestamosIds.Contains(x.PrestamoId) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();

                    foreach (var prestamo in prestamos)
                    {
                        var cuotas = detallesPrestamos.Where(x => x.PrestamoId == prestamo.Id).ToList();
                        prestamo.CuotasPagadas = cuotas.Count();
                    }

                    await _context.SaveChangesAsync();

                    return AppResult.New(true, "Listo");

                }


            }


        }






        public class GetPrestamoByClienteId
        {
            public class QueryPrestamoByClienteId : IRequest<List<PrestamoVm>>
            {
                public Guid ClienteId { get; set; }
            }

            public class QueryHandler : IRequestHandler<QueryPrestamoByClienteId, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamoByClienteId query, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => x.ClienteId == query.ClienteId && !x.IsSoftDeleted )
                        .AsNoTracking()
                        .OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .OrderBy(x => x.Estado)
                        .ToListAsync();

                    foreach(var prestamo in prestamos)
                    {
                        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled).ToList();

                        var ultimoDetalle = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                        if (ultimoDetalle != null && ultimoDetalle.RestaCapital == prestamo.RestaCapital)
                        {
                            prestamo.DeudaTotal = ultimoDetalle.RestaCapital + ultimoDetalle.ProximoPago;
                        }
                        else
                        {
                            var interes = prestamo.InteresPorcentaje;
                            if (interes > 1)
                            {
                                interes = interes / 100;
                            }
                            prestamo.DeudaTotal = prestamo.CantidadInicial + (Math.Round(prestamo.CantidadInicial * interes, 2));
                        }

                        decimal gananciaEstimada = 0M;
                        prestamo.PrestamoDetalles.ForEach(s => gananciaEstimada += s.MontoInteres);
                        prestamo.GananciaEstimado = gananciaEstimada;
                        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).ToList();
                    }


                    return prestamos;

                }


            }


        }



        public class GetPrestamoByIdV1
        {
            public class QueryPrestamoById : IRequest<PrestamoVm>
            {
                public Guid Id { get; set; }
            }

            public class QueryHandler : IRequestHandler<QueryPrestamoById, PrestamoVm>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<PrestamoVm> Handle(QueryPrestamoById query, CancellationToken cancellationToken)
                {

                    var prestamo = await _context.Prestamo.Where(x => x.Id == query.Id && !x.IsSoftDeleted)
                        .AsNoTracking()
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .FirstOrDefaultAsync();

                    if (prestamo != null)
                    {
                        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled && !x.IsSoftDeleted).ToList();

                        if (prestamo != null && prestamo.RestaCapital > 0)
                        {
                            var ultimoDetalle = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                            #region Calcular el interes por Dia

                            var interes = prestamo.InteresPorcentaje;

                            #region como lo hacia antes


                            //if (interes > 1)
                            //{
                            //    interes = interes / 100;
                            //}

                            //TimeSpan restarFechas = DateTime.Now - prestamo.FechaUltimoPago;                     // Restar las fechas: Desde el ultimo pago hasta la actualida
                            //int cantidadDias = restarFechas.Days;                                                // Obtener la cantidad de días que se generaron interes


                            //if (prestamo.FechaUltimoPago == DateTime.MinValue || prestamo.FechaUltimoPago == null)  //Si no han pagado ningun cuota
                            //{
                            //    restarFechas = DateTime.Now - prestamo.FechaEntragado;
                            //    cantidadDias = restarFechas.Days;

                            //    prestamo.InteresMes = Math.Round(prestamo.CantidadInicial * interes, 2);

                            //    if (configuracion.PrimerMesInteresObligatorio && cantidadDias <= 30)
                            //    {
                            //        prestamo.InteresGenerado = prestamo.InteresMes;
                            //    }
                            //    else
                            //    {
                            //        var interesAlDia = prestamo.InteresMes / 30;     //30 dias
                            //        prestamo.InteresGenerado = interesAlDia * cantidadDias;
                            //    }


                            //    prestamo.HaceCuantosDiasUltimoPago = "Ningun pago realizado";
                            //}
                            //else if (ultimoDetalle != null)
                            //{

                            //    prestamo.InteresMes = ultimoDetalle.ProximoPago;

                            //    //Calcular el interes a pagar por dias

                            //    var interesBaseMes = ultimoDetalle.ProximoPago;
                            //    var interesAlDia = interesBaseMes / 30;     //30 dias

                            //    prestamo.InteresGenerado = interesAlDia * cantidadDias;

                            //    prestamo.HaceCuantosDiasUltimoPago = "Hace " + cantidadDias + " dias";
                            //    prestamo.PagoHasta = ultimoDetalle.FechaPago;
                            //}
                            #endregion


                            var ultimoPago = prestamo.FechaUltimoPago;
                            if (ultimoPago == null || ultimoPago == DateTime.MinValue)
                            {
                                ultimoPago = prestamo.FechaEntragado;
                                prestamo.HaceCuantosDiasUltimoPago = "Ningun pago realizado";
                            }

                            var proximoMes = ultimoPago.AddMonths(1);
                            var diasAContar = (proximoMes - ultimoPago).Days;


                            if (interes > 1)
                            {
                                interes = interes / 100;
                            }

                            var interesMes = Math.Round(prestamo.RestaCapital * interes, 2);
                            var interesDiario = interesMes / diasAContar;


                            var hoy = DateTime.Now;
                            var diasTranscurridos = (hoy - ultimoPago).Days;

                            var interesGenerado = diasTranscurridos * interesDiario;

                            prestamo.InteresGenerado = interesGenerado;

                            //Si es el primer pago y esta configurado que el primer mes es obligatorio
                            if (configuracion.PrimerMesInteresObligatorio && (prestamo.FechaUltimoPago == null || prestamo.FechaUltimoPago == DateTime.MinValue))
                            {
                                prestamo.InteresGenerado = prestamo.InteresMes;
                            }

                            if (prestamo.HaceCuantosDiasUltimoPago == null)
                            {
                                prestamo.HaceCuantosDiasUltimoPago = "Hace " + diasTranscurridos + " dias";
                            }

                            #endregion







                            if (ultimoDetalle != null && ultimoDetalle.RestaCapital == prestamo.RestaCapital)
                            {
                                prestamo.DeudaTotal = ultimoDetalle.RestaCapital + prestamo.InteresGenerado;
                            }
                            else
                            {
                                prestamo.DeudaTotal = prestamo.CantidadInicial + prestamo.InteresGenerado;
                            }

                            decimal gananciaEstimada = 0M;
                            prestamo.PrestamoDetalles.ForEach(s => gananciaEstimada += s.MontoInteres);
                            prestamo.GananciaEstimado = gananciaEstimada;



                            prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.OrderByDescending(s => s.CreatedDate).ToList();

                        }

                    }

                    return prestamo;

                }


            }


        }



        public class GetPrestamoById
        {
            public class PrestamoRes : PrestamoVm
            {
                public CuentaBancariaVm CuentaBancaria { get; set; }
            }
            public class QueryPrestamoById : IRequest<PrestamoRes>
            {
                public Guid Id { get; set; }
            }

            public class QueryHandler : IRequestHandler<QueryPrestamoById, PrestamoRes>
            {
                private readonly CooperativaDbContext _context;
                private readonly ICalculationService _calculationService;

                public QueryHandler(CooperativaDbContext context, ICalculationService calculationService)
                {
                    _context = context;
                    _calculationService = calculationService;
                }

                public async Task<PrestamoRes> Handle(QueryPrestamoById query, CancellationToken cancellationToken)
                {

                    var prestamoEntidad = await _context.Prestamo.Where(x => x.Id == query.Id && !x.IsSoftDeleted)
                        .Include(x => x.PrestamoDetalles)
                        .Include(x => x.PrestamoDetalles).ThenInclude(s => s.CuentaBancaria)
                        .Include(x => x.Cliente)
                        .Include(s => s.CuentaBancaria).ThenInclude(s => s.InstitucionBancaria)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    var prestamo = prestamoEntidad.Adapt<PrestamoRes>();

                    if (prestamo != null)
                    {
                        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled && !x.IsSoftDeleted).ToList();

                        if (prestamo != null && prestamo.RestaCapital > 0)
                        {
                            var ultimoDetalle = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                            var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();


                            var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamoEntidad, configuracion, DateTime.Now, 0);


                            prestamo.InteresGenerado = calculo.InteresGenerado;
                            var diasTranscurridos = calculo.DiasGenerados;
                            if(diasTranscurridos < 0)
                            {
                                diasTranscurridos = 0;
                            }


                            //Si es el primer pago y esta configurado que el primer mes es obligatorio
                            if (configuracion.PrimerMesInteresObligatorio && (prestamo.FechaUltimoPago == null || prestamo.FechaUltimoPago == DateTime.MinValue))
                            {
                                prestamo.InteresGenerado = prestamo.InteresMes;
                            }

                            if (prestamo.HaceCuantosDiasUltimoPago == null)
                            {
                                prestamo.HaceCuantosDiasUltimoPago = "Hace " + diasTranscurridos + " dias";
                            }

                            if (prestamo.InteresGenerado < 0)
                            {
                                prestamo.InteresGenerado = 0;
                            }


                            if (ultimoDetalle != null && ((ultimoDetalle.RestaCapital == prestamo.RestaCapital || Math.Round(ultimoDetalle.RestaCapital, 2) == Math.Round(prestamo.RestaCapital, 2))  ))
                            {
                                prestamo.DeudaTotal = ultimoDetalle.RestaCapital + prestamo.InteresGenerado;
                            }
                            else
                            {
                                prestamo.DeudaTotal = prestamo.CantidadInicial + prestamo.InteresGenerado;
                            }

                            decimal gananciaEstimada = 0M;
                            prestamo.PrestamoDetalles.ForEach(s => gananciaEstimada += s.MontoInteres);
                            prestamo.GananciaEstimado = gananciaEstimada;



                            prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.OrderByDescending(s => s.CreatedDate).ToList();

                        }

                    }

                    return prestamo;

                }


            }


        }






        public class GetPrestamosActivos
        {
            public class QueryPrestamosActivos : IRequest<List<PrestamoVm>>
            {

            }

            public class QueryPrestamosActivosHandler : IRequestHandler<QueryPrestamosActivos, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosActivosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamosActivos query, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => x.Estado == EstadoPrestamo.Vigente && !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .OrderByDescending(x => x.CreatedDate)
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .ToListAsync();

                    foreach(var prestamo in prestamos)
                    {
                        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled).ToList();
                    }

                    return prestamos;

                }
            }
        }



        public class GetPrestamosActivosProSolicitudPrestamo
        {

            public class QueryPrestamosActivos : IRequest<List<PrestamoVm>>
            {

            }

            public class QueryPrestamosActivosHandler : IRequestHandler<QueryPrestamosActivos, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosActivosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamosActivos query, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => x.Estado == EstadoPrestamo.Vigente && !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .OrderByDescending(x => x.CreatedDate)
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .ToListAsync();

                    foreach (var prestamo in prestamos)
                    {
                        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled).ToList();
                    }

                    return prestamos;

                }
            }
        }




        public class GetPrestamosPagados
        {
            public class QueryPrestamosPagados : IRequest<List<PrestamoVm>>
            {

            }

            public class QueryPrestamosPagadosHandler : IRequestHandler<QueryPrestamosPagados, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosPagadosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamosPagados query, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => x.Estado == EstadoPrestamo.Pagado && !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .OrderByDescending(x => x.CreatedDate)
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .ToListAsync();


                    return prestamos;

                }


            }


        }





        public class GetPrestamosCancelados
        {
            public class QueryPrestamosCancelados : IRequest<List<PrestamoVm>>
            {

            }

            public class QueryPrestamosCanceladosHandler : IRequestHandler<QueryPrestamosCancelados, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosCanceladosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamosCancelados query, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => x.Estado == EstadoPrestamo.Cancelado && !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .OrderByDescending(x => x.CreatedDate)
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .ToListAsync();


                    return prestamos;

                }


            }


        }


        public class Index
        {
            public class QueryPrestamosIndex : IRequest<List<PrestamoVm>>
            {

            }

            public class QueryPrestamosIndexHandler : IRequestHandler<QueryPrestamosIndex, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosIndexHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamosIndex query, CancellationToken cancellationToken)
                {

                    var prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted)
                        .OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                        .AsNoTracking()
                        .Include(x => x.PrestamoDetalles)
                        .ProjectToType<PrestamoVm>()
                        .ToListAsync();
                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                    foreach (var prestamo in prestamos)
                    {

                        var ultimoPago = prestamo.FechaUltimoPago;
                        if(ultimoPago == null || ultimoPago == DateTime.MinValue)
                        {
                            ultimoPago = prestamo.FechaEntragado;
                        }

                        var proximoMes = ultimoPago.AddMonths(1);
                        var diasAContar = (proximoMes - ultimoPago).Days;
                        //var cantidadPorDia = prestamo.PrestamoDetalles.Where(x => ).OrderByDescending(x => x.CreatedDate).FirstOrDefault();


                        var interes = prestamo.InteresPorcentaje;
                        if (interes > 1)
                        {
                            interes = interes / 100;
                        }

                        var interesMes = Math.Round(prestamo.RestaCapital * interes, 2);
                        var interesDiario = interesMes/diasAContar;


                        var hoy = DateTime.Now;
                        var diasTranscurridos = (hoy - ultimoPago).Days;

                        var interesGenerado = diasTranscurridos * interesDiario;


                        prestamo.InteresGenerado = interesGenerado;
                        if (prestamo.InteresGenerado < 0)
                        {
                            prestamo.InteresGenerado = 0;
                        }

                        //Si es el primer pago y esta configurado que el primer mes es obligatorio
                        if (configuracion.PrimerMesInteresObligatorio && (prestamo.FechaUltimoPago == null || prestamo.FechaUltimoPago == DateTime.MinValue))
                        {
                            prestamo.InteresGenerado = prestamo.InteresMes;
                        }



                        prestamo.Mora = prestamo.InteresGenerado - prestamo.EstimadoAPagarMes;
                        if(prestamo.Mora < 0) { prestamo.Mora = 0; }

                    }


                    return prestamos;

                }


            }


        }




        public class IndexSoloPrestamos
        {
            public class QueryPrestamosIndex : IRequest<List<PrestamoDto>>
            {

            }
            public class QueryPrestamosIndexHandler : IRequestHandler<QueryPrestamosIndex, List<PrestamoDto>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosIndexHandler(CooperativaDbContext context)
                {
                    _context = context;
                }
                public async Task<List<PrestamoDto>> Handle(QueryPrestamosIndex query, CancellationToken cancellationToken)
                {
                    var prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ProjectToType<PrestamoDto>()
                        .ToListAsync();

                    return prestamos;
                }
            }
        }






        public class GetByFiltros
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
            public class EstadoInputVm
            {
                public EstadoPrestamo Estado { get; set; }
            }
            public class QueryPrestamosByFiltros : IRequest<List<PrestamoVm>>
            {
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
                public EstadoInputVm Estado { get; set; }
            }

            public class QueryPrestamosByFiltrosHandler : IRequestHandler<QueryPrestamosByFiltros, List<PrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosByFiltrosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoVm>> Handle(QueryPrestamosByFiltros query, CancellationToken cancellationToken)
                {
                    List<PrestamoVm> prestamos = new List<PrestamoVm>();


                    //Aqui no he ido a la base de datos, solo tengo armada la consulta: Select * from Prestamo where IsSoftDeleted = false
                    var prestamosQuery = _context.Prestamo.Where(x => !x.IsSoftDeleted)
                        .Include(x => x.PrestamoDetalles)
                        .AsNoTracking();

                    if (query.Estado != null)
                    {
                        //Aqui agregi un filtro mas: Select * from Prestamo where IsSoftDeleted = false && Estado == query.Estado
                        prestamosQuery = prestamosQuery.Where(x => x.Estado == query.Estado.Estado);
                    }
                    else
                    {
                        prestamosQuery = prestamosQuery.Where(x => x.Estado != EstadoPrestamo.Cancelado);
                    }

                    if (query.Index)
                    {
                        //Aqui oficialmente voy a la base da datos
                        //prestamos = await prestamosQuery
                        //    .ProjectToType<PrestamoVm>()
                        //    .ToListAsync();

                        //return prestamos;
                    }
                    else if (query.Anio != 0)
                    {
                        prestamosQuery = prestamosQuery.Where(x => x.CreatedDate.Year == query.Anio);
                    }
                    else if (query.AnioMes != null)
                    {
                        prestamosQuery = prestamosQuery.Where(x => x.CreatedDate.Year == query.AnioMes.Anio && x.CreatedDate.Month == query.AnioMes.Mes);
                    }
                    else if (query.RangoFechas != null)
                    {
                        var inicio = query.RangoFechas.FechaInicio.Date;
                        var fin = query.RangoFechas.FechaFin.Date;
                        prestamosQuery = prestamosQuery.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin);
                    }


                    //Aqui oficialmente voy a la base da datos
                    prestamos = await prestamosQuery
                        .ProjectToType<PrestamoVm>()
                        .ToListAsync();





                    #region Forma que normalmente trabajo en vesta
                    //if (query.Index)
                    //{
                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && (query.Estado != null || query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    //else if (query.Anio != 0)
                    //{
                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.Anio && (query.Estado != null && query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    //else if (query.AnioMes != null)
                    //{
                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.AnioMes.Anio && x.CreatedDate.Month == query.AnioMes.Mes && (query.Estado != null && query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    //else if (query.RangoFechas != null)
                    //{
                    //    var inicio = query.RangoFechas.FechaInicio.Date;
                    //    var fin = query.RangoFechas.FechaFin.Date;

                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.CreatedDate >= inicio && x.CreatedDate <= fin && !x.IsSoftDeleted && (query.Estado != null && query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    #endregion



                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                    foreach (var prestamo in prestamos)
                    {

                        var ultimoPago = prestamo.FechaUltimoPago;
                        if (ultimoPago == null || ultimoPago == DateTime.MinValue)
                        {
                            ultimoPago = prestamo.FechaEntragado;
                        }

                        var proximoMes = ultimoPago.AddMonths(1);
                        var diasAContar = (proximoMes - ultimoPago).Days;
                        //var cantidadPorDia = prestamo.PrestamoDetalles.Where(x => ).OrderByDescending(x => x.CreatedDate).FirstOrDefault();


                        var interes = prestamo.InteresPorcentaje;
                        if (interes > 1)
                        {
                            interes = interes / 100;
                        }

                        var interesMes = Math.Round(prestamo.RestaCapital * interes, 2);
                        var interesDiario = interesMes / diasAContar;


                        var hoy = DateTime.Now;
                        var diasTranscurridos = (hoy - ultimoPago).Days;

                        var interesGenerado = diasTranscurridos * interesDiario;


                        prestamo.InteresGenerado = interesGenerado;
                        if (prestamo.InteresGenerado < 0)
                        {
                            prestamo.InteresGenerado = 0;
                        }

                        //Si es el primer pago y esta configurado que el primer mes es obligatorio
                        if (configuracion.PrimerMesInteresObligatorio && (prestamo.FechaUltimoPago == null || prestamo.FechaUltimoPago == DateTime.MinValue))
                        {
                            prestamo.InteresGenerado = prestamo.InteresMes;
                        }



                        prestamo.Mora = prestamo.InteresGenerado - prestamo.EstimadoAPagarMes;
                        if (prestamo.Mora < 0) { prestamo.Mora = 0; }

                    }


                    return prestamos;

                }


            }


        }





        public class GetByFiltrosSinDetalles
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
            public class EstadoInputVm
            {
                public EstadoPrestamo Estado { get; set; }
            }
            public class QueryPrestamosByFiltros : IRequest<List<PrestamoDto>>
            {
                public bool Index { get; set; }                
                public bool IndexSinEstado { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
                public EstadoInputVm Estado { get; set; }
            }

            public class QueryPrestamosByFiltrosHandler : IRequestHandler<QueryPrestamosByFiltros, List<PrestamoDto>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosByFiltrosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<PrestamoDto>> Handle(QueryPrestamosByFiltros query, CancellationToken cancellationToken)
                {
                    List<PrestamoDto> prestamos = new List<PrestamoDto>();


                    //Aqui no he ido a la base de datos, solo tengo armada la consulta: Select * from Prestamo where IsSoftDeleted = false
                    var prestamosQuery = _context.Prestamo.Where(x => !x.IsSoftDeleted)
                        .Include(x => x.PrestamoDetalles)
                        .AsNoTracking();





                    if (!query.IndexSinEstado)
                    {
                        if (query.Estado != null)
                        {
                            //Aqui agregi un filtro mas: Select * from Prestamo where IsSoftDeleted = false && Estado == query.Estado
                            prestamosQuery = prestamosQuery.Where(x => x.Estado == query.Estado.Estado);
                        }
                        else
                        {
                            prestamosQuery = prestamosQuery.Where(x => x.Estado == EstadoPrestamo.Vigente || x.Estado == EstadoPrestamo.Pagado);
                        }

                        if (query.Index)
                        {

                        }
                        else if (query.Anio != 0)
                        {
                            prestamosQuery = prestamosQuery.Where(x => x.CreatedDate.Year == query.Anio);
                        }
                        else if (query.AnioMes != null)
                        {
                            prestamosQuery = prestamosQuery.Where(x => x.CreatedDate.Year == query.AnioMes.Anio && x.CreatedDate.Month == query.AnioMes.Mes);
                        }
                        else if (query.RangoFechas != null)
                        {
                            var inicio = query.RangoFechas.FechaInicio.Date;
                            var fin = query.RangoFechas.FechaFin.Date;
                            prestamosQuery = prestamosQuery.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin);
                        }
                    }
                    else
                    {
                        //Si no se mapea todo
                    }



                    //Aqui oficialmente voy a la base da datos
                    prestamos = await prestamosQuery
                        .ProjectToType<PrestamoDto>()
                        .ToListAsync();





                    #region Forma que normalmente trabajo en vesta
                    //if (query.Index)
                    //{
                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && (query.Estado != null || query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    //else if (query.Anio != 0)
                    //{
                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.Anio && (query.Estado != null && query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    //else if (query.AnioMes != null)
                    //{
                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.AnioMes.Anio && x.CreatedDate.Month == query.AnioMes.Mes && (query.Estado != null && query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    //else if (query.RangoFechas != null)
                    //{
                    //    var inicio = query.RangoFechas.FechaInicio.Date;
                    //    var fin = query.RangoFechas.FechaFin.Date;

                    //    prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.CreatedDate >= inicio && x.CreatedDate <= fin && !x.IsSoftDeleted && (query.Estado != null && query.Estado.Estado == x.Estado))
                    //        .Include(x => x.PrestamoDetalles)
                    //        //.OrderByDescending(x => x.Estado_Descripcion).ThenByDescending(x => x.CreatedDate)
                    //        .AsNoTracking()
                    //        .ProjectToType<PrestamoVm>()
                    //        .ToListAsync();
                    //}
                    #endregion



                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();

                    foreach (var prestamo in prestamos)
                    {

                        var ultimoPago = prestamo.FechaUltimoPago;
                        if (ultimoPago == null || ultimoPago == DateTime.MinValue)
                        {
                            ultimoPago = prestamo.FechaEntragado;
                        }

                        var proximoMes = ultimoPago.AddMonths(1);
                        var diasAContar = (proximoMes - ultimoPago).Days;
                        //var cantidadPorDia = prestamo.PrestamoDetalles.Where(x => ).OrderByDescending(x => x.CreatedDate).FirstOrDefault();


                        var interes = prestamo.InteresPorcentaje;
                        if (interes > 1)
                        {
                            interes = interes / 100;
                        }

                        var interesMes = Math.Round(prestamo.RestaCapital * interes, 2);
                        var interesDiario = interesMes / diasAContar;


                        var hoy = DateTime.Now;
                        var diasTranscurridos = (hoy - ultimoPago).Days;

                        var interesGenerado = diasTranscurridos * interesDiario;


                        prestamo.InteresGenerado = interesGenerado;
                        if (prestamo.InteresGenerado < 0)
                        {
                            prestamo.InteresGenerado = 0;
                        }

                        //Si es el primer pago y esta configurado que el primer mes es obligatorio
                        if (configuracion.PrimerMesInteresObligatorio && (prestamo.FechaUltimoPago == null || prestamo.FechaUltimoPago == DateTime.MinValue))
                        {
                            prestamo.InteresGenerado = prestamo.InteresMes;
                        }



                        prestamo.Mora = prestamo.InteresGenerado - prestamo.EstimadoAPagarMes;
                        if (prestamo.Mora < 0) { prestamo.Mora = 0; }

                    }


                    return prestamos;

                }


            }


        }




        public class GetEstadosPrestamo
        {
            public class EstadoRes
            {
                public int Id { get; set; }
                public string Descripcion { get; set; }
            }
            public class QueryGTR : IRequest<List<EstadoRes>>
            {
            }

            public class Handler : IRequestHandler<QueryGTR, List<EstadoRes>>
            {
                public Handler()
                {
                }

                public async Task<List<EstadoRes>> Handle(QueryGTR request, CancellationToken cancellationToken)
                {
                    // Simulamos async por convención, aunque no hay query a BD
                    var estadosFiltrados = Enum.GetValues(typeof(EstadoPrestamo))
                        .Cast<EstadoPrestamo>()
                        .Select(t => new EstadoRes
                        {
                            Id = (int)t,
                            Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)t)
                        })
                        .ToList();

                    return estadosFiltrados;
                }
            }
        }




        public class GetEstadosParaAtenderPrestamo
        {
            public class EstadoRes
            {
                public int Id { get; set; }
                public string Descripcion { get; set; }
            }
            public class QueryGTR : IRequest<List<EstadoRes>>
            {
            }

            public class Handler : IRequestHandler<QueryGTR, List<EstadoRes>>
            {
                public Handler()
                {
                }

                public async Task<List<EstadoRes>> Handle(QueryGTR request, CancellationToken cancellationToken)
                {
                    // Simulamos async por convención, aunque no hay query a BD
                    // Filtramos solo los estados que necesitamos
                    var estadosFiltrados = Enum.GetValues(typeof(EstadoPrestamo))
                        .Cast<EstadoPrestamo>()
                        .Where(e => e == EstadoPrestamo.Vigente || e == EstadoPrestamo.Rechazado) // <-- filtramos aquí
                        .Select(t => new EstadoRes
                        {
                            Id = (int)t,
                            Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)t)
                        })
                        .ToList();

                    return estadosFiltrados;
                }
            }
        }





        public class DashBoardPIM
        {
            public class RespuestaVm
            {
                public List<ResumenGlobalPorEstadoVm> ResumenGlobalPorEstados { get; set; }
                public List<PrestamoMontoAnioVm> PrestamosEstadosAnios { get; set; }
            }
            public class ResumenGlobalPorEstadoVm
            {
                public string Estado_Descripcion { get; set; }
                public decimal CantidadPrestadaTotalGlobal { get; set; }
                public decimal GananciaTotalGlobal { get; set; }
                public int CantidadPrestamosTotalGlobal { get; set; }
            }
            public class PrestamoMontoAnioVm
            {
                public int Anio { get; set; }
                public string Estado_Descripcion { get; set; }
                public decimal CantidadPrestadaTotalAnio { get; set; }
                public decimal GananciaTotalAnio { get; set; }
                public int CantidadPrestamosTotalAnio { get; set; }
                public List<MontoMesVm> Meses { get; set; }
            }
            public class MontoMesVm
            {
                public string Estado_Descripcion { get; set; }
                public DateTime Fecha { get; set; }
                public decimal CantidadPrestada { get; set; }
                public decimal Ganancia { get; set; }
                public int CantidadPrestamos { get; set; }
                public int CantidadPrestamosDetalles { get; set; }
            }
            public class QueryPrestamosIndex : IRequest<RespuestaVm>
            {

            }
            public class QueryPrestamosIndexHandler : IRequestHandler<QueryPrestamosIndex, RespuestaVm>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosIndexHandler(CooperativaDbContext context)
                {
                    _context = context;
                }
                public async Task<RespuestaVm> Handle(QueryPrestamosIndex query, CancellationToken cancellationToken)
                {
                    var prestamos = await _context.Prestamo.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking()
                        .ToListAsync();

                    var prestamosIds = prestamos.Select(x => x.Id).ToList();
                    var prestamosDetalles = await _context.PrestamoDetalle.Where(x => !x.IsSoftDeleted && prestamosIds.Contains(x.PrestamoId))
                        .AsNoTracking()
                        .ToListAsync();

                    var anios = prestamos.Select(x => x.CreatedDate.Year).Distinct().ToList();
                    var estados = prestamos.Select(x => x.Estado).Distinct().ToList();

                    List<PrestamoMontoAnioVm> prestamoMontoAnio = new List<PrestamoMontoAnioVm>();
                    List<ResumenGlobalPorEstadoVm> resumenesGlobalPorEstados = new List<ResumenGlobalPorEstadoVm>();



                    //var prestamosValidos = prestamos.Where(x => x.Estado != EstadoPrestamo.Cancelado ).ToList();
                    var prestamosValidos = prestamos.Where(x => x.Estado == EstadoPrestamo.Vigente || x.Estado == EstadoPrestamo.Pagado).ToList();
                    for (int i = 0; i < anios.Count(); i++)
                    {
                        var anio = anios.ElementAt(i);

                        var cantidadMeses = prestamosValidos.Where(x => x.CreatedDate.Year == anio).Select(x => x.CreatedDate.Month).Distinct().ToList();

                        var newPresamoEstadoAnio = new PrestamoMontoAnioVm
                        {
                            Anio = anio,
                            Meses = new List<MontoMesVm>(),
                        };

                        decimal cantidadTotalPrestadaAnio = 0.0M;
                        decimal gananciaTotalAnio = 0.0M;
                        int cantidadPrestamosTotalAnio = 0;

                        for (int m = 0; m < cantidadMeses.Count(); m++)
                        {
                            var mes = cantidadMeses.ElementAt(m);

                            var prestamosMesAnio = prestamosValidos.Where(x => x.CreatedDate.Year == anio && x.CreatedDate.Month == mes).ToList();
                            var prestamosMesAnioIds = prestamosMesAnio.Select(x => x.Id).ToList();
                            var prestamosDetallesMesAnio = prestamosDetalles.Where(x => x.CreatedDate.Year == anio && x.CreatedDate.Month == mes).ToList();

                            var montoPrestadoMes = prestamosMesAnio.Sum(x => x.CantidadInicial);
                            var ganancias = prestamosDetallesMesAnio.Sum(x => x.MontoInteres);

                            var newMes = new MontoMesVm
                            {
                                Estado_Descripcion = prestamosMesAnio.FirstOrDefault().Estado_Descripcion,
                                CantidadPrestamos = prestamosMesAnio.Count(),
                                CantidadPrestamosDetalles = prestamosDetallesMesAnio.Count(),
                                Fecha = new DateTime(anio, mes, 1),
                                CantidadPrestada = montoPrestadoMes,
                                Ganancia = ganancias
                            };

                            newPresamoEstadoAnio.Meses.Add(newMes);
                            cantidadTotalPrestadaAnio += newMes.CantidadPrestada;
                            gananciaTotalAnio += newMes.Ganancia;
                            cantidadPrestamosTotalAnio += newMes.CantidadPrestamos;

                        }

                        //newPresamoEstadoAnio.Estado_Descripcion = estadoDescripcion;
                        newPresamoEstadoAnio.CantidadPrestadaTotalAnio = cantidadTotalPrestadaAnio;
                        newPresamoEstadoAnio.GananciaTotalAnio = gananciaTotalAnio;

                        prestamoMontoAnio.Add(newPresamoEstadoAnio);
                    }




                    for (int e = 0; e < estados.Count; e++)
                    {
                        var estado = estados.ElementAt(e);

                        var prestamosEstados = prestamos.Where(x => x.Estado == estado).ToList();

                        var estadoDescripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)estado);

                        var prestamosEstado = prestamos.Where(x => x.Estado == estado).ToList();

                        var cantidadTotalPrestadaGlobal = prestamosEstado.Sum(x => x.CantidadInicial);
                        var gananciaTotalGlobal = prestamosEstado.Sum(x => x.Ganancia); ;
                        int cantidadPrestamosGlobal = prestamosEstado.Count();

                        if (estado == EstadoPrestamo.Vigente)
                        {
                            cantidadTotalPrestadaGlobal = prestamosEstado.Sum(x => x.RestaCapital);
                        }
                        if (estado == EstadoPrestamo.Cancelado)
                        {
                            gananciaTotalGlobal = 0;
                        }


                        var newResumenGeneralPorEstado = new ResumenGlobalPorEstadoVm
                        {
                            Estado_Descripcion = estadoDescripcion,
                            CantidadPrestadaTotalGlobal = cantidadTotalPrestadaGlobal,
                            GananciaTotalGlobal = (decimal)gananciaTotalGlobal,
                            CantidadPrestamosTotalGlobal = cantidadPrestamosGlobal
                        };

                        resumenesGlobalPorEstados.Add(newResumenGeneralPorEstado);
                    }



                    var respuesta = new RespuestaVm
                    {
                        PrestamosEstadosAnios = prestamoMontoAnio,
                        ResumenGlobalPorEstados = resumenesGlobalPorEstados
                    };

                    return respuesta;
                }
            }
        }




























        public class PrestamoDto
        {
            public Guid Id { get;set; }
            public int NumeroPrestamo { get; set; }
            public Guid Responsable { get; set; }
            public string CodigoPrestamo { get; set; }
            public decimal CantidadInicial { get; set; }
            public Moneda Moneda { get; set; }
            public string Moneda_Descripcion { get; set; }
            public string Estado_Descripcion { get; set; }
            public decimal InteresPorcentaje { get; set; }
            public int CantidadMeses { get; set; }
            public int CuotasPagadas { get; set; }
            public decimal EstimadoAPagarMes { get; set; }  //Hacer un calculo rapido por: Cantidad - %Interes - cantidad de meses
            public string Garantia { get; set; }
            public DateTime FechaEntragado { get; set; }
            public DateTime FechaEstimadoFinPrestamo { get; set; }
            public EstadoPrestamo Estado { get; set; }
            public DateTime FechaUltimoPago { get; set; }
            public DateTime PagoHasta { get; set; }
            public int MesesPagados { get; set; }
            public decimal MontoPagado { get; set; }
            public decimal RestaCapital { get; set; }
            public decimal InteresMes { get; set; }
            public string HaceCuantosDiasUltimoPago { get; set; }
            public decimal InteresGenerado { get; set; }
            public decimal? Ganancia { get; set; }
            public int DebeMeses { get; set; }          //mediante un metodo que calcule cuantos meses no ha pagado el cliente
            public decimal Mora { get; set; }
            public string Observacion { get; set; }
            public decimal DeudaTotal { get; set; }
            public decimal GananciaEstimado { get; set; }
            public Guid ClienteId { get; set; }
            public string ClienteNombre { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public TipoPrestamo TipoPrestamo { get; set; }
            public string TipoPrestamo_Descripcion { get; set; }
            //public decimal InteresGenerado { get; set; }
            public string ReferenciaBancaria { get; set; }
            public bool? RequiereDocumento { get; set; }
            public string LinkDocumento { get; set; }
            public Guid? CuentaBancariaId { get; set; }
        }

        public class PrestamoVm : PrestamoDto
        {
            public ClienteVm Cliente { get; set; }
            public List<PrestamoDetalleDto> PrestamoDetalles { get; set; }
        }
        public class PrestamoDetalleDto : PrestamoDetalleVm
        {
            public CuentaBancariaVm CuentaBancaria { get; set; }
        }




    }
}
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
using Cooperativa.App.Domain.Model.Caja;
using static Cooperativa.App.Domain.Model.Caja.Retiro;
using Mapster;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Utilidades;
using Microsoft.VisualBasic;
using static Cooperativa.App.CRUD.PrestamoCrud;

namespace Cooperativa.App.CRUD
{
    public class RetiroCrud
    {
        public class CrearParaCaja
        {
            public class CommandRec : IRequest<AppResult>
            {
                public TipoRetiro TipoRetiro { get; set; }
                public Guid? CajaId { get; set; }    //Falta trabajar, para cuando ya haga de muchas cajas para un Cliente
                public Guid SolicitanteId { get; set; }
                public Guid CuentaBancariaId { get; set; }
                public decimal Monto { get; set; }
                public string Moneda_Descripcion { get; set; }
                public string Motivo { get; set; }
                public string Observacion { get; set; }

            }

            public class CommandHandler : IRequestHandler<CommandRec, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandRec cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var usuario = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        //var caja = await _context.Caja.Where(x => x.Id == cmd.CajaId).FirstOrDefaultAsync();
                        //if (caja == null) { throw new Exception("caja no existe"); }

                        var caja = await _context.Caja.Where(x => x.Id == cmd.CajaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync(); //Traera la unica habilitada
                        if (caja == null) { throw new Exception("No se encontro Caja!"); }

                        var solicitante = await _context.Socio.Where(x => x.Id == cmd.SolicitanteId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if(solicitante == null) { throw new Exception("Socio solicitante no existe"); }

                        var retirosPendientes = await _context.Retiro.Where(x => /*x.CreatedBy == cmd.CreatedBy*/ x.Estado == EstadoRetiro.Pendiente && !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                        if(retirosPendientes.Count >= 20) { throw new Exception("Nose puedo realizar Solicitud de Retiro, hay 20 o mas Solicitudes pendientes."); }

                        //var cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        //if(cuentaBancaria == null) { throw new Exception("No se encontro Cuenta Bancaria a creditar."); }
                        //if(cuentaBancaria.CajaId == null || cuentaBancaria.CajaId != caja.Id) { throw new Exception("Cuenta bancaria no pertenece a Caja"); }

                        //Descomentar lo de arriba;             Tengo que hacer la reparacion de Caja ya que se crea un Registro de Caja por cada movimiento, me parece que todo lo que hago en caja puedo llevarlo a Transacciones
                        var cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (cuentaBancaria == null) { throw new Exception("Cuenta Bancaria no existe"); }


                        var historialMoneda = await _context.HistorialCambioMoneda.Where(x => !x.IsSoftDeleted && x.Enabled)
                            .OrderByDescending(x => x.CreatedDate)
                            .FirstOrDefaultAsync();



                        #region Correlativo de NumeroRetiro
                        
                        int numRetiros = await _context.Retiro.CountAsync(); // Obtener el total de retiros existentes
                        numRetiros++; // Sumar 1 para el nuevo correlativo
                        int anio = DateTime.Now.Year % 100; // Obtener los últimos dos dígitos del año

                        var numeroRetiro = $"RTR{(int)cmd.TipoRetiro}-{numRetiros:D4}-{anio}"; // Formatear el número de retiro con ceros a la izquierda

                        var existe = await _context.Retiro.Where(x => x.NumeroRetiro == numeroRetiro).AsNoTracking().FirstOrDefaultAsync();
                        if(existe != null) { throw new Exception("Ocurrio un error al generar NumeroRetiro"); }
                        #endregion


                        var newRetiro = Retiro.New(numeroRetiro, caja.Id, solicitante.Id, historialMoneda != null ? historialMoneda.Id : null, cuentaBancaria.Id, cmd.TipoRetiro, cmd.Monto, cmd.Motivo, cmd.Observacion, cmd.Moneda_Descripcion, cmd.SolicitanteId);

                        await _context.Retiro.AddAsync(newRetiro);
                        await _context.SaveChangesAsync();




                        //Si manda socio



                        /*OJO 24 mayo 2025
                        
                        Voy a agregar una propiedad en Socio llamada, TotalInversion y TotalGanancia;
                         * Cada movimiento de inversiones se lo sumo o resto (Creo que ya lo hago)   
                         * Cada movimiento se lo suma o resto a Ganancia

                            * Cuando llegue el momento de retirar dinero entonces tomo esas dos propiedades y de ahi saco dependiendo.
                            * La Tabla de retiro deberia de guardar cuanto dinero tenia el Socio en esas propiedades de TotalInversion y TotalGanancia

                        */




                        //QUE FALTA?
                        //Falta pedir una lista de SocioInversion, que sera cuando un Socio quiera retirar.... o podria ser que cuando manden Socio

                        //OJO ANGEL: tengo que analizar bien como hacer estos retiros... porque los Socio si tienen 15k y solo quieren retirar 7k por ejemplo?
                        //          Entonces el socio se queda se queda con 8k mas las ganancias.
                        //Otro esecenario esque el Socio diga que solo quiere sacar las ganacias




                        return AppResult.New(true, "Solicitud de Retiro creado exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }






        public class CrearParaSocioInversion
        {
            public class SocioInversionCantidadVm
            {
                public Guid SocioInversionId { get; set; }
                public decimal CantidadARetirar { get; set; }
            }

            public class CommandRsi : IRequest<AppResult>
            {
                public TipoRetiro TipoRetiro { get; set; }
                public Guid? CajaId { get; set; }    //Falta trabajar, para cuando ya haga de muchas cajas para un Cliente
                public Guid SolicitanteId { get; set; }
                public Guid CuentaBancariaId { get; set; }
                //public decimal Monto { get; set; }
                public string Moneda_Descripcion { get; set; }
                public string Motivo { get; set; }
                public List<SocioInversionCantidadVm> SocioInversiones { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandRsi, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandRsi cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var usuario = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var sociosInversionesIds = cmd.SocioInversiones.Select(x => x.SocioInversionId).Distinct().ToList();
                        var cantidadTotalRetirar = cmd.SocioInversiones.Sum(x => x.CantidadARetirar);


                        //var caja = await _context.Caja.Where(x => x.Id == cmd.CajaId).FirstOrDefaultAsync();
                        //if (caja == null) { throw new Exception("caja no existe"); }

                        var caja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync(); //Traera la unica habilitada
                        if (caja == null) { throw new Exception("No se encontro Caja!"); }

                        if (caja.SaldoActual < cantidadTotalRetirar)
                        {
                            throw new Exception("No hay saldo suficiente en caja para realizar retiro");
                        }


                        var solicitante = await _context.Socio.Where(x => x.Id == cmd.SolicitanteId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (solicitante == null) { throw new Exception("Socio solicitante no existe"); }

                        var retirosPendientes = await _context.Retiro.Where(x => /*x.CreatedBy == cmd.CreatedBy*/ x.Estado == EstadoRetiro.Pendiente && !x.IsSoftDeleted).AsNoTracking().ToListAsync();
                        if (retirosPendientes.Count >= 20) { throw new Exception("Nose puedo realizar Solicitud de Retiro, hay 20 o mas Solicitudes pendientes."); }

                        //var cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        //if(cuentaBancaria == null) { throw new Exception("No se encontro Cuenta Bancaria a creditar."); }
                        //if(cuentaBancaria.CajaId == null || cuentaBancaria.CajaId != caja.Id) { throw new Exception("Cuenta bancaria no pertenece a Caja"); }

                        //Descomentar lo de arriba;             Tengo que hacer la reparacion de Caja ya que se crea un Registro de Caja por cada movimiento, me parece que todo lo que hago en caja puedo llevarlo a Transacciones
                        var cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        if (cuentaBancaria == null) { throw new Exception("Cuenta Bancaria no existe"); }


                        var historialMoneda = await _context.HistorialCambioMoneda.Where(x => !x.IsSoftDeleted && x.Enabled)
                            .OrderByDescending(x => x.CreatedDate)
                            .FirstOrDefaultAsync();



                        #region Correlativo de NumeroRetiro

                        int numRetiros = await _context.Retiro.CountAsync(); // Obtener el total de retiros existentes
                        numRetiros++; // Sumar 1 para el nuevo correlativo
                        int anio = DateTime.Now.Year % 100; // Obtener los últimos dos dígitos del año

                        var numeroRetiro = $"RTR{(int)cmd.TipoRetiro}-{numRetiros:D4}-{anio}"; // Formatear el número de retiro con ceros a la izquierda

                        var existe = await _context.Retiro.Where(x => x.NumeroRetiro == numeroRetiro).AsNoTracking().FirstOrDefaultAsync();
                        if (existe != null) { throw new Exception("Ocurrio un error al generar NumeroRetiro"); }
                        #endregion



                        var newRetiro = Retiro.New(numeroRetiro, caja.Id, solicitante.Id, historialMoneda != null ? historialMoneda.Id : null, cuentaBancaria.Id, cmd.TipoRetiro, cantidadTotalRetirar, cmd.Motivo, "", cmd.Moneda_Descripcion, cmd.SolicitanteId);
                        await _context.Retiro.AddAsync(newRetiro);

                        var sociosInversion = await _context.SocioInversion.Where(x => sociosInversionesIds.Contains(x.Id) && !x.IsSoftDeleted).ToListAsync();

                        List<SocioInversionRetiro> newSocioInversionRetiros = new List<SocioInversionRetiro>();
                        for(int i=0; i<cmd.SocioInversiones.Count(); i++)
                        {
                            var inversionCmd = cmd.SocioInversiones.ElementAt(i);
                            var inversion = sociosInversion.Where(x => x.Id == inversionCmd.SocioInversionId).FirstOrDefault();
                            if (inversion == null)
                            {
                                throw new Exception("SocioInversion no existe");
                            }


                            if(inversion.CantidadDisponibleRetirar < inversionCmd.CantidadARetirar)
                            {
                                throw new Exception($"Para la Socio Inversion {inversion.CodigoInversion} no es posible retirar cantidad porque la maxima cantidad disponible a retirar es {inversion.CantidadDisponibleRetirar}");
                            }


                            var newSocioRetiro = SocioInversionRetiro.New(inversion.Id, newRetiro.Id, inversionCmd.CantidadARetirar, usuario);
                            newSocioInversionRetiros.Add(newSocioRetiro);
                        }


                        await _context.SocioInversionRetiro.AddRangeAsync(newSocioInversionRetiros);


                        await _context.SaveChangesAsync();









                        //Si manda socio



                        /*OJO 24 mayo 2025
                        
                        Voy a agregar una propiedad en Socio llamada, TotalInversion y TotalGanancia;
                         * Cada movimiento de inversiones se lo sumo o resto (Creo que ya lo hago)   
                         * Cada movimiento se lo suma o resto a Ganancia

                            * Cuando llegue el momento de retirar dinero entonces tomo esas dos propiedades y de ahi saco dependiendo.
                            * La Tabla de retiro deberia de guardar cuanto dinero tenia el Socio en esas propiedades de TotalInversion y TotalGanancia

                        */




                        //QUE FALTA?
                        //Falta pedir una lista de SocioInversion, que sera cuando un Socio quiera retirar.... o podria ser que cuando manden Socio

                        //OJO ANGEL: tengo que analizar bien como hacer estos retiros... porque los Socio si tienen 15k y solo quieren retirar 7k por ejemplo?
                        //          Entonces el socio se queda se queda con 8k mas las ganancias.
                        //Otro esecenario esque el Socio diga que solo quiere sacar las ganacias




                        return AppResult.New(true, "Solicitud de Retiro creado exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }

















        public class AtenderRetiro
        {
            public class CommandAR : IRequest<AppResult>
            {
                public Guid RetiroId { get; set; }
                public EstadoRetiro Estado { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }   //Cuenta de la CajaId
                public string Observacion { get; set; }
                //public Guid UsuarioId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandAR, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandAR cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        //var usuario = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var primerSocio = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        var usuario = primerSocio.Id;

                        var retiro = await _context.Retiro.Where(x => x.Id == cmd.RetiroId && !x.IsSoftDeleted)
                            .Include(x => x.CuentaBancaria)
                            .Include(x => x.SocioInversionRetiros)
                            .FirstOrDefaultAsync();
                        retiro.ThrowIfNull("No existe Retiro");

                        if (retiro.Estado != EstadoRetiro.Pendiente) { throw new Exception($"Retiro ya fue atendido a {retiro.Estado_Descripcion}"); }



                        if (cmd.Estado == EstadoRetiro.Rechazado)
                        {
                            //retiro.Estado = EstadoRetiro.Rechazado;

                            retiro.ActualizarEstado(cmd.Estado, usuario, cmd.Observacion);
                            //return AppResult.New(true, "Retiro rechazado exitosamente");
                        }
                        else if(cmd.Estado == EstadoRetiro.Aprobado)
                        {

                            //La caja sera luego por empresaId
                            var caja = await _context.Caja.Where(x => x.Id == retiro.CajaId && !x.IsSoftDeleted == x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();
                            caja.ThrowIfNull("No se encontro Caja para Empresa");


                            var cuentaDestino = retiro.CuentaBancaria;
                            cuentaDestino.ThrowIfNull("No se encontro Cuenta Destino");
                            var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaOrigenId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                            cuentaOrigen.ThrowIfNull("Cuenta Origen no existe");


                            if (cuentaOrigen.SaldoActual < retiro.Monto)
                            {
                                throw new Exception("Cuenta Origen no cuenta con saldo necesario para atender retiro");
                            }

                            if (retiro.SocioInversionRetiros.Any())
                            {
                                var socioInversionRetiros = retiro.SocioInversionRetiros.ToList();

                                var socioInversionIds = socioInversionRetiros.Select(x => x.SocioInversionId).Distinct().ToList();
                                var socioInversiones = await _context.SocioInversion.Where(x => socioInversionIds.Contains(x.Id) && !x.IsSoftDeleted).ToListAsync();

                                await _context.DetalleSocioInversion.Where(x => socioInversionIds.Contains(x.SocioInversionId) && !x.IsSoftDeleted)
                                    .Include(x => x.Prestamo)
                                    .ToListAsync();


                                var socioId = socioInversiones.Select(x => x.SocioId).Distinct().ToList();
                                if(socioId.Count() > 1)
                                {
                                    throw new Exception("Hay mas de un Socio en este Retiro, favir revise Socio Inversiones adjuntas al Retiro");
                                }
                                var socio = await _context.Socio.Where(x => socioId.Contains(x.Id) && !x.IsSoftDeleted).FirstOrDefaultAsync();
                                socio.ThrowIfNull("Socio no existe");


                                for (int i =0; i< socioInversionIds.Count(); i++)
                                {
                                    var socioInversionId = socioInversionIds.ElementAt(i);

                                    var retirosSocioInversion = socioInversionRetiros.Where(x => x.SocioInversionId == socioInversionId).ToList();
                                    var totalRetiro = retirosSocioInversion.Sum(x => x.Cantidad);


                                    var socioInversion = socioInversiones.Where(x => x.Id == socioInversionId).FirstOrDefault();
                                    if(socioInversion == null)
                                    {
                                        throw new Exception("No se encontro SocioInversion con Id: " + socioInversionId);
                                    }


                                    if(socioInversion.CantidadDisponibleRetirar < totalRetiro)
                                    {
                                        throw new Exception($"No es posible Retirar {totalRetiro:N2} de Inversion solicitada porque la cantidad disponible es menor en este momento");
                                    }


                                    socioInversion.CantidadDisponibleRetirar -= totalRetiro;


                                    var gananciaDisponile = socioInversion.GananciaDisponible;
                                    var cantidadActiva = socioInversion.CantidadActiva;

                                    var restarAcantidadActiva = 0M;

                                    //Ocupo saber cuanto le quitare a GananciaDisponible y a CantidadActiva
                                    if (totalRetiro <= gananciaDisponile)
                                    {
                                        socioInversion.GananciaDisponible -= totalRetiro;
                                    }
                                    else
                                    {
                                        var residuoGananciaDisponible = totalRetiro - gananciaDisponile;
                                        socioInversion.GananciaDisponible = 0;


                                        socioInversion.CantidadActiva -= residuoGananciaDisponible;

                                        if (socioInversion.CantidadActiva < 0.5M)
                                        {
                                            socioInversion.CantidadActiva = 0;
                                            socioInversion.CantidadDisponibleRetirar = 0;
                                        }

                                        restarAcantidadActiva = residuoGananciaDisponible;
                                    }

                                    socioInversion.Retirado = totalRetiro;



                                    //if (residuoGananciaDisponible < 0.5M)
                                    //{
                                    //    socioInversion.CantidadDisponibleRetirar = 0;

                                    //    socioInversion.CantidadActiva = 0;

                                    //    residuoGananciaDisponible = 0;
                                    //}






                                    #region Parte de dinero trabajando
                                    var detallesSocioInversion = socioInversion.DetallesSocioInversion.Where(x => x.Enabled && !x.IsSoftDeleted && x.Prestamo.Estado == EstadoPrestamo.Vigente).ToList();

                                    if (socioInversion.CantidadActiva == 0)
                                    {
                                        socioInversion.Estado = EstadoInversion.Retirado;
                                        socioInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)socioInversion.Estado);

                                        socioInversion.NoPrestado = 0;
                                        socioInversion.CantidadPrestada = 0;

                                        detallesSocioInversion.ForEach(x =>
                                        {
                                            x.Enabled = false;
                                            x.ModifiedBy = usuario;
                                            x.ModifiedDate = DateTime.Now;
                                        });
                                    }
                                    else
                                    {

                                        if(restarAcantidadActiva > 0)   //Si Cantidad Activa se vio afectada
                                        {

                                            //  QUE TENGO QUE HACER???
                                            /*
                                             SocioInversion:        NoPrestado y CantidadPrestada
                                             DetalleSocioInversion: Crear uno nuevo con los nuevos calculos de cantidad trabajando, para que no pierda trabajo
                                             */

                                            var A = restarAcantidadActiva;                  // Cantidad que se bajo a la inversion
                                            var B = socioInversion.NoPrestado;              // No Prestado
                                            var C = socioInversion.CantidadPrestada;        // Cantidad Prestada

                                            var quedaDeNoPrestado = 0M;

                                            if(A > B)
                                            {
                                                socioInversion.NoPrestado = A - socioInversion.NoPrestado;

                                                quedaDeNoPrestado = A - B;


                                                if(quedaDeNoPrestado > 0)   //Afecta CantidadPrestada
                                                {
                                                    if(quedaDeNoPrestado > C)
                                                    {
                                                        throw new Exception($"Discrepancia se le queria Restar a Inversion {quedaDeNoPrestado:N2} de {C:N2} disponibles de cantidad prestada (o cantidad trabajando)");
                                                    }

                                                    var restante = C - quedaDeNoPrestado;

                                                    socioInversion.CantidadPrestada = socioInversion.CantidadPrestada - quedaDeNoPrestado;

                                                    if(restante > 0)
                                                    {


                                                        decimal quitarAtrabajando = restante;

                                                        foreach(var detalle in detallesSocioInversion)
                                                        {
                                                            if(detalle.SePresto >= quitarAtrabajando)
                                                            {
                                                                detalle.Enabled = false;
                                                                detalle.Observacion = $"Se deshbilito porque el Socio realizo retiro: {retiro.Id}";
                                                                detalle.ModifiedBy = usuario;
                                                                detalle.ModifiedDate = DateTime.Now;


                                                                quitarAtrabajando -= detalle.SePresto;
                                                            }
                                                        }


                                                        if(quitarAtrabajando> 0)
                                                        {
                                                            var detallesSocioInversionDeMenorAMayor = detallesSocioInversion.Where(x => x.Enabled == true).OrderBy(x => x.SePresto).ToList();

                                                            var detalleDeshabilitar = detallesSocioInversionDeMenorAMayor.FirstOrDefault();
                                                            detalleDeshabilitar.Enabled = false;
                                                            detalleDeshabilitar.Observacion = $"Se deshbilito porque el Socio realizo retiro: {retiro.Id} *** Este se deshabilito por necesidad de abarcar catidad retirada";
                                                            detalleDeshabilitar.ModifiedBy = usuario;
                                                            detalleDeshabilitar.ModifiedDate = DateTime.Now;
                                                        }
                                                        else if(quitarAtrabajando < 0)
                                                        {
                                                            throw new Exception($"Discrepancia al gestionar DetalleSocioInversion de prestamos activos");
                                                        }

                                                    }

                                                    //Entonces debo crear nuevos detalleSocioInversion para que trabajen a parte de la nueva cantidad
                                                    //O me sale mejor dejar solo aquello que estan dentro del rango


                                                }

                                            }
                                            else
                                            {
                                                socioInversion.NoPrestado = socioInversion.NoPrestado - A;
                                            }



                                        }


                                    }
                                    #endregion

                                    if(socioInversion.CantidadActiva > 0)
                                    {
                                        var nuevoPorcentajePrestado = (socioInversion.CantidadPrestada / socioInversion.CantidadActiva) * 100;
                                        socioInversion.PorcetajePrestado = nuevoPorcentajePrestado;
                                    }
                                    else
                                    {
                                        socioInversion.PorcetajePrestado = 0;
                                    }


                                    socioInversion.ModifiedBy = usuario;
                                    socioInversion.ModifiedDate = DateTime.Now;

                                }
                                socio.CantidadRetiros++;
                                socio.TotalRetirado += retiro.Monto;
                                socio.SaldoDisponibleARetirar -= retiro.Monto;
                                
                            }



                            var egresosCantidad = await _context.Egreso.CountAsync(); //ToListAsync();
                            var newEgreso = Egreso.NewPorRetiro(egresosCantidad + 1, retiro.NumeroRetiro, retiro.Monto, retiro.Motivo, retiro.Observacion, retiro.Id, usuario);
                            await _context.Egreso.AddAsync(newEgreso);

                            var numTransacciones = await _context.Transaccion.CountAsync();
                            var newTransaccion = Transaccion.New(numTransacciones + 1, retiro.NumeroRetiro, retiro.Monto, cuentaOrigen.Id, retiro.CuentaBancariaId, usuario);
                            newTransaccion.EgresoId = newEgreso.Id;
                            newTransaccion.CajaId = caja.Id;


                            cuentaOrigen.RestarMovimiento(retiro.Monto);                 //Caja
                            cuentaDestino.SumarMovimiento(retiro.Monto);                 //Cliente/Socio

                            await _context.Transaccion.AddAsync(newTransaccion);


                            #region Proceso viejo caja
                            //var ultimoRegistroCaja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();


                            //string saldoEnMoneda = Math.Round(ultimoRegistroCaja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

                            //if (ultimoRegistroCaja.SaldoActual < retiro.Monto)
                            //{
                            //    return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {saldoEnMoneda} ");
                            //}

                            //var nuevoSaldo = ultimoRegistroCaja.SaldoActual - newEgreso.Monto;

                            //var agregarACaja = Caja.NewEgreso(usuario, retiro.NumeroRetiro, ultimoRegistroCaja.SaldoActual, nuevoSaldo, newEgreso.Id, usuario);
                            //await _context.Caja.AddAsync(agregarACaja);

                            //ultimoRegistroCaja.Enabled = false;
                            //ultimoRegistroCaja.ModifiedDate = DateTime.Now;
                            //ultimoRegistroCaja.ModifiedBy = usuario;

                            #endregion



                            string saldoEnMoneda = Math.Round(caja.SaldoActual, 2).ToString("C", System.Globalization.CultureInfo.GetCultureInfo("es-HN")); // "C" indica formato de moneda

                            if (caja.SaldoActual < retiro.Monto)
                            {
                                return AppResult.New(false, $"No hay Dinero suficiente en Caja. Actual: {saldoEnMoneda} ");
                            }

                            var saldoActual = caja.SaldoActual;
                            var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;


                            //caja.SaldoAnterior = saldoActual;
                            //caja.SaldoActual = nuevoSaldo;
                            //caja.UltimaTransaccionId = newTransaccion.Id;
                            //caja.ModifiedDate = DateTime.Now;
                            //caja.ModifiedBy = usuario;


                            caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, usuario);
                            newTransaccion.SaldoCajaEnElMomento = saldoActual;
                            newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;




                            //retiro.Estado = EstadoRetiro.Aprobado;
                            retiro.ActualizarEstado(cmd.Estado, usuario, cmd.Observacion);
                            retiro.ResponsableAtendioId = usuario;
                            retiro.EgresoId = newEgreso.Id;
                        }
                        else
                        {
                            return AppResult.New(true, "Desicion de Retiro no valido");
                        }


                        await _context.SaveChangesAsync();


                        return AppResult.New(true, $"Retiro {retiro.Estado_Descripcion} exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }














        public class GetPendientesAtender
        {
            public class CommandGPA : IRequest<List<RetiroRes>>
            {

            }

            public class CommandHandler : IRequestHandler<CommandGPA, List<RetiroRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<RetiroRes>> Handle(CommandGPA cmd, CancellationToken cancellationToken)
                {

                    var retiros = await _context.Retiro.Where(x => x.Estado == EstadoRetiro.Pendiente && !x.IsSoftDeleted)
                        .ProjectToType<RetiroRes>()
                        .ToListAsync();

                    var cuentasBancariasIds = retiros.Select(x => x.CuentaBancariaId).Distinct().ToList();
                    var cuentasBancarias = await _context.CuentaBancaria.Where(x => cuentasBancariasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();

                    retiros.ForEach(x =>
                    {
                        var cuentaBancaria = cuentasBancarias.Where(s => s.Id == x.CuentaBancariaId).FirstOrDefault();
                        if(cuentaBancaria != null)
                        {
                            //x.CuentaBancariaDescripcion = cuentaBancaria.InstitucionBancaria.Nombre + " " + cuentaBancaria.NumeroTarjeta;
                            x.CuentaBancariaDescripcion = cuentaBancaria.InstitucionBancaria != null ? cuentaBancaria.InstitucionBancaria.Nombre + " " + cuentaBancaria.NumeroTarjeta : cuentaBancaria.NumeroTarjeta;
                        }
                        else
                        {
                            x.CuentaBancariaDescripcion = "Efectivo";
                        }
                    });


                    return retiros;
                }
            }
        }





        public class GetRetiros
        {
            public class CommandGR : IRequest<List<RetiroRes>>
            {

            }

            public class CommandHandler : IRequestHandler<CommandGR, List<RetiroRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<RetiroRes>> Handle(CommandGR cmd, CancellationToken cancellationToken)
                {

                    var retiros = await _context.Retiro.Where(x => x.Estado != EstadoRetiro.Pendiente && !x.IsSoftDeleted)
                        .ProjectToType<RetiroRes>()
                        .ToListAsync();


                    var cuentasBancariasIds = retiros.Select(x => x.CuentaBancariaId).Distinct().ToList();
                    var cuentasBancarias = await _context.CuentaBancaria.Where(x => cuentasBancariasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();

                    retiros.ForEach(x =>
                    {
                        var cuentaBancaria = cuentasBancarias.Where(s => s.Id == x.CuentaBancariaId).FirstOrDefault();
                        if (cuentaBancaria != null)
                        {
                            //x.CuentaBancariaDescripcion = cuentaBancaria.InstitucionBancaria.Nombre + " " + cuentaBancaria.NumeroTarjeta;
                            x.CuentaBancariaDescripcion = cuentaBancaria.InstitucionBancaria != null ? cuentaBancaria.InstitucionBancaria.Nombre + " " + cuentaBancaria.NumeroTarjeta : cuentaBancaria.NumeroTarjeta;
                        }
                        else
                        {
                            x.CuentaBancariaDescripcion = "Efectivo";
                        }
                    });


                    return retiros;
                }
            }
        }




        public class GetRetirosBySocioId
        {
            public class CommandGRBS : IRequest<List<RetiroRes>>
            {
                public Guid SocioId { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandGRBS, List<RetiroRes>>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<RetiroRes>> Handle(CommandGRBS cmd, CancellationToken cancellationToken)
                {

                    var sociosRetiros = await _context.SocioInversionRetiro.Where(x => x.SocioInversion.SocioId == cmd.SocioId && !x.IsSoftDeleted).ToListAsync();
                    var retirosIds = sociosRetiros.Select(x => x.RetiroId).Distinct().ToList();

                    var retiros = await _context.Retiro.Where(x => retirosIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .ProjectToType<RetiroRes>()
                        .ToListAsync();


                    var cuentasBancariasIds = retiros.Select(x => x.CuentaBancariaId).Distinct().ToList();
                    var cuentasBancarias = await _context.CuentaBancaria.Where(x => cuentasBancariasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();

                    retiros.ForEach(x =>
                    {
                        var cuentaBancaria = cuentasBancarias.Where(s => s.Id == x.CuentaBancariaId).FirstOrDefault();
                        if (cuentaBancaria != null)
                        {
                            x.CuentaBancariaDescripcion = cuentaBancaria.InstitucionBancaria != null ? cuentaBancaria.InstitucionBancaria.Nombre + " " + cuentaBancaria.NumeroTarjeta : cuentaBancaria.NumeroTarjeta;
                        }
                        else
                        {
                            x.CuentaBancariaDescripcion = "Efectivo";
                        }
                    });


                    return retiros;
                }
            }
        }






        public class GetReporteRapido
        {
            public class Respuesta
            {
                public decimal CantidadTotalPendiente { get; set; }
                public decimal CantidadTotalAprobadoMes { get; set; }

                public int TotalPendientes { get; set; }
                public int TotalRechazados { get; set; }
                public int TotalAprobados { get; set; }

                public decimal CantidadTotalAprobado { get; set; }
            }
            public class CommandRR : IRequest<Respuesta>
            {

            }

            public class CommandHandler : IRequestHandler<CommandRR, Respuesta>
            {
                private readonly CooperativaDbContext _context;

                public CommandHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<Respuesta> Handle(CommandRR cmd, CancellationToken cancellationToken)
                {
                    #region Esta fue mi idea pero ChatGpt dijo que era mas optimizada la actual
                    //var cantidadTotalPendiente = await _context.Retiro
                    // .Where(x => x.Estado != EstadoRetiro.Pendiente && !x.IsSoftDeleted)
                    // .SumAsync(s => s.Monto);

                    //var cantidadTotalAprobadoMes = await _context.Retiro
                    // .Where(x => x.Estado != EstadoRetiro.Aprobado && !x.IsSoftDeleted)
                    // .SumAsync(s => s.Monto);

                    //var totalPendientes = await _context.Retiro
                    // .Where(x => x.Estado != EstadoRetiro.Pendiente && !x.IsSoftDeleted)
                    // .CountAsync();

                    //var totalRechazados = await _context.Retiro
                    // .Where(x => x.Estado != EstadoRetiro.Pendiente && !x.IsSoftDeleted)
                    // .CountAsync();

                    //var cantidadTotalAprobado = await _context.Retiro
                    // .Where(x => x.Estado != EstadoRetiro.Pendiente && !x.IsSoftDeleted)
                    // .CountAsync();


                    //var newRes = new Respuesta
                    //{
                    //    CantidadTotalPendiente = cantidadTotalPendiente,
                    //    CantidadTotalAprobadoMes = cantidadTotalAprobadoMes,
                    //    TotalPendientes = totalPendientes,
                    //    TotalRechazados = totalRechazados,
                    //    CantidadTotalAprobado = cantidadTotalAprobado
                    //};

                    #endregion

                    var fechaActual = DateTime.Now;

                    var reporte = await _context.Retiro
                        .Where(x => !x.IsSoftDeleted )
                        .GroupBy(r => 1)
                        .Select(g => new
                        {
                            TotalPendientes = g.Count(x => x.Estado == EstadoRetiro.Pendiente && !x.IsSoftDeleted),
                            TotalAprobados = g.Count(x => x.Estado == EstadoRetiro.Aprobado && !x.IsSoftDeleted),
                            TotalRechazados = g.Count(x => x.Estado == EstadoRetiro.Rechazado && !x.IsSoftDeleted),
                            CantidadTotalPendiente = g.Where(x => x.Estado == EstadoRetiro.Pendiente && !x.IsSoftDeleted).Sum(x => x.Monto),
                            CantidadTotalAprobadoMes = g.Where(x => x.Estado == EstadoRetiro.Aprobado && x.CreatedDate.Month == fechaActual.Month && !x.IsSoftDeleted).Sum(x => x.Monto),
                            CantidadTotalAprobado = g.Where(x => x.Estado == EstadoRetiro.Aprobado && !x.IsSoftDeleted).Sum(x => x.Monto)
                        })
                        .FirstOrDefaultAsync();

                    var newRes = new Respuesta
                    {
                        CantidadTotalPendiente = reporte.CantidadTotalPendiente,
                        CantidadTotalAprobadoMes = reporte.CantidadTotalAprobadoMes,
                        TotalPendientes = reporte.TotalPendientes,
                        TotalRechazados = reporte.TotalRechazados,
                        TotalAprobados = reporte.TotalAprobados,
                        CantidadTotalAprobado = reporte.CantidadTotalAprobado,

                    };


                    return newRes;
                }
            }
        }








        public class GetTiposRetiros
        {
            public class QueryGTR : IRequest<List<TipoRetiroRes>>
            {
            }

            public class Handler : IRequestHandler<QueryGTR, List<TipoRetiroRes>>
            {
                public Handler()
                {
                }

                public async Task<List<TipoRetiroRes>> Handle(QueryGTR request, CancellationToken cancellationToken)
                {
                    // Simulamos async por convención, aunque no hay query a BD
                    return await Task.FromResult(
                        Enum.GetValues(typeof(TipoRetiro))
                            .Cast<TipoRetiro>()
                            .Select(t => new TipoRetiroRes
                            {
                                Id = (int)t,
                                Descripcion = TipoRetiroDescripcion.GetEstadoTexto((int)t)
                            })
                            .ToList()
                    );
                }
            }
        }


        public class GetEstadosRetiro
        {
            public class QueryGTR : IRequest<List<TipoRetiroRes>>
            {
            }

            public class Handler : IRequestHandler<QueryGTR, List<TipoRetiroRes>>
            {
                public Handler()
                {
                }

                public async Task<List<TipoRetiroRes>> Handle(QueryGTR request, CancellationToken cancellationToken)
                {
                    // Simulamos async por convención, aunque no hay query a BD
                    return await Task.FromResult(
                        Enum.GetValues(typeof(EstadoRetiro))
                            .Cast<EstadoRetiro>()
                            .Select(t => new TipoRetiroRes
                            {
                                Id = (int)t,
                                Descripcion = EstadoRetiroDescripcion.GetEstadoTexto((int)t)
                            })
                            .ToList()
                    );
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
                public EstadoRetiro Estado { get; set; }
            }
            public class TipoInputVm
            {
                public TipoRetiro Tipo { get; set; }
            }
            public class Query : IRequest<List<RetiroRes>>
            {
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
                public EstadoInputVm Estado { get; set; }
                public TipoInputVm Tipo { get; set; }
            }

            public class QueryHandler : IRequestHandler<Query, List<RetiroRes>>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<RetiroRes>> Handle(Query query, CancellationToken cancellationToken)
                {
                    List<RetiroRes> retiros = new List<RetiroRes>();

                    //Aqui no he ido a la base de datos, solo tengo armada la consulta: Select * from Prestamo where IsSoftDeleted = false
                    var retirosQuery = _context.Retiro.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking();

                    if (query.Estado != null)
                    {
                        retirosQuery = retirosQuery.Where(x => x.Estado == query.Estado.Estado);
                    }
   
                    if (query.Tipo != null)
                    {
                        retirosQuery = retirosQuery.Where(x => x.TipoRetiro == query.Tipo.Tipo);
                    }
      

                    if (query.Index)
                    {

                    }
                    if (query.Anio != 0)
                    {
                        retirosQuery = retirosQuery.Where(x => x.CreatedDate.Year == query.Anio);
                    }
                    if (query.AnioMes != null)
                    {
                        retirosQuery = retirosQuery.Where(x => x.CreatedDate.Year == query.AnioMes.Anio && x.CreatedDate.Month == query.AnioMes.Mes);
                    }
                    if (query.RangoFechas != null)
                    {
                        var inicio = query.RangoFechas.FechaInicio.Date;
                        var fin = query.RangoFechas.FechaFin.Date;
                        retirosQuery = retirosQuery.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin);
                    }


                    //Aqui oficialmente voy a la base da datos
                    retiros = await retirosQuery
                        .ProjectToType<RetiroRes>()
                        .ToListAsync();


                    var cuentasBancariasIds = retiros.Select(x => x.CuentaBancariaId).Distinct().ToList();
                    var cuentasBancarias = await _context.CuentaBancaria.Where(x => cuentasBancariasIds.Contains(x.Id) && !x.IsSoftDeleted)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();

                    retiros.ForEach(x =>
                    {
                        var cuentaBancaria = cuentasBancarias.Where(s => s.Id == x.CuentaBancariaId).FirstOrDefault();
                        if (cuentaBancaria != null)
                        {
                            x.CuentaBancariaDescripcion = cuentaBancaria.InstitucionBancaria != null ? cuentaBancaria.InstitucionBancaria.Nombre + " " + cuentaBancaria.NumeroTarjeta : cuentaBancaria.NumeroTarjeta;
                        }
                        else
                        {
                            x.CuentaBancariaDescripcion = "Efectivo";
                        }
                    });


                    return retiros;
                }
            }
        }




















        public class RetiroRes : RetiroVm
        {
            public PersonaRes Solicitante { get; set; }
            public PersonaRes ResponsableAtendio { get; set; }
            public string CuentaBancariaDescripcion { get; set; }
            public List<SocioRetiroRes> SocioInversionRetiros { get; set; }
        }
        public class PersonaRes
        {
            public Guid? Id { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
        }

        public class TipoRetiroRes
        {
            public int Id { get; set; }
            public string Descripcion { get; set; }
        }
        public class CuentaBancariaVm
        {
            public Guid? Id { get; set; }
        }
        public class SocioRetiroRes
        {
            public Guid? Id { get; set; }
            public Guid SocioInversionId { get; set; }
            public string SocioInversionCodigoInversion { get; set; }
            public decimal Cantidad { get; set; }
        }





    }
}

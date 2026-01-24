using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Utilidades;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.PrestamoCrud;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;

namespace Cooperativa.App.CRUD
{
    public class CajaCrud
    {

        public class Crear
        {
            public class Command : IRequest<AppResult>
            {
                //public Guid ResponsableId { get; set; }
                public Guid? EmpresaId { get; set; }
                public ModuloCaja Modulo { get; set; }
                //public Guid CreatedBy { get; set; }
            }

            public class CommandHandlerSocioInversion : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;

                public CommandHandlerSocioInversion(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase; 
                }

                public async Task<AppResult> Handle(Command command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
                        var responsable = await _context.Socio.Where(x => !x.IsSoftDeleted).FirstOrDefaultAsync();


                        //Codigo: [CLIENTE-EMPRESA]-[SUCURSAL]-[MODULO]-CAJ[NUMERO]
                        var hoy = DateTime.Now;
                        var codigoModulo = "";
                        if (command.Modulo == ModuloCaja.Prestamos)
                        {
                            codigoModulo = "PRE";
                        }
                        if(codigoModulo.Length < 1) { throw new Exception("Codigo de Modulo no encontrado"); }

                        var codigoEmpresa = "COOPAZ";
                        var correlativoBase = $"{codigoEmpresa}-{codigoModulo}-{hoy.Year}";

                        var secuencial = await _iUtilidadesBase.GenerarSecuencial("Caja", correlativoBase);

                        var correlativoCaja = $"{correlativoBase}-CAJ{secuencial:D3}";                  //"COOPAZ-PRE-2025-CAJ001"


                        var caja = await _context.Caja.Where(x => x.Enabled && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        caja.Referencia = correlativoCaja;


                        //var newCaja = Caja.New(responsable.Id, Guid.Empty, correlativoCaja, command.Modulo, createdBy);

                        //await _context.Caja.AddAsync(newCaja);

                        await _context.SaveChangesAsync();


                        return AppResult.New(true, $"Caja creada existosamente: {correlativoCaja}");

                    }
                    catch (Exception e)
                    {
                        return AppResult.New(false, e.Message);

                    }

                }
            }
        }





        public class GetCajasByEmpresaId
        {
            public class Query : IRequest<List<CajaVm>>
            {
                public Guid? EmpresaId { get; set; }
            }

            public class QueryCajaHandler : IRequestHandler<Query, List<CajaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryCajaHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CajaVm>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var cajas = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .ProjectToType<CajaVm>()
                        .ToListAsync();

                    return cajas;
                }
            }
        }



        public class GetCajasConTransaccionesByEmpresaId
        {
            public class CajaRes : CajaVm
            {
                public List<TransaccionVm> Transacciones { get; set; }
            }
            public class Query : IRequest<List<CajaRes>>
            {
                public Guid? EmpresaId { get; set; }
            }

            public class QueryCajaHandler : IRequestHandler<Query, List<CajaRes>>
            {
                private readonly CooperativaDbContext _context;

                public QueryCajaHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CajaRes>> Handle(Query query, CancellationToken cancellationToken)
                {
                    var cajas = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).ToListAsync();

                    var cajasIds = cajas.Select(X => X.Id).ToList();
                    await _context.CuentaBancaria.Where(x => cajasIds.Contains((Guid)x.CajaId) && !x.IsSoftDeleted)
                        .Include(x => x.InstitucionBancaria)
                        .ToListAsync();
                    await _context.Transaccion.Where(x => cajasIds.Contains((Guid)x.CajaId) && !x.IsSoftDeleted).ToListAsync();


                    List< CajaRes > cajasVm = new List< CajaRes >();

                    foreach(var ca in cajas)
                    {
                        var cajaVm = ca.Adapt<CajaRes>();
                        cajasVm.Add(cajaVm);
                    }


                    return cajasVm;
                }
            }
        }




        public class GetCajaModuloPrestamoByEmpresaId
        {
            public class Query : IRequest<CajaVm>
            {
                public Guid? EmpresaId { get; set; }
            }

            public class QueryCajaHandler : IRequestHandler<Query, CajaVm>
            {
                private readonly CooperativaDbContext _context;

                public QueryCajaHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<CajaVm> Handle(Query query, CancellationToken cancellationToken)
                {
                    var caja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled && x.Modulo == ModuloCaja.Prestamos)
                        .AsNoTracking()
                        .ProjectToType<CajaVm>()
                        .FirstOrDefaultAsync();

                    return caja;
                }
            }
        }




        public class GetSaldoActivo
        {
            public class QueryGetSaldoActivo : IRequest<CajaVm>
            {

            }

            public class QueryCajaHandler : IRequestHandler<QueryGetSaldoActivo, CajaVm>
            {
                private readonly CooperativaDbContext _context;

                public QueryCajaHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<CajaVm> Handle(QueryGetSaldoActivo query, CancellationToken cancellationToken)
                {

                    var caja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .OrderByDescending(x => x.CreatedDate)
                        .AsNoTracking()
                        .Include(x => x.Ingreso)
                        .Include(x => x.Egreso)
                        .ProjectToType<CajaVm>()
                        .FirstOrDefaultAsync();

                    return caja;

                }


            }


        }

        public class Index
        {
            public class QueryGetCajas : IRequest<List<CajaVm>>
            {

            }

            public class QueryCajaHandler : IRequestHandler<QueryGetCajas, List<CajaVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryCajaHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<CajaVm>> Handle(QueryGetCajas query, CancellationToken cancellationToken)
                {
                    var cajas = await _context.Caja.Where(x => !x.IsSoftDeleted )
                        .AsNoTracking()
                        .ProjectToType<CajaVm>()
                        .ToListAsync();

                    return cajas;
                }
            }
        }


        public class GetInformacionInicio
        {
            public class Res
            {
                public int CantidadPrestamosPIM { get; set; }
                public int CantidadPrestamosPIMPendientesAprobacion { get; set; }
                public int CantidadPrestamosPIMVigente { get; set; }
                public int CantidadPrestamosPIA { get; set; }
                public int CantidadSocios { get; set; }
                public int CantidadClientes { get; set; }
            }

            public class Query : IRequest<Res>
            {

            }

            public class QueryHandler : IRequestHandler<Query, Res>
            {
                private readonly CooperativaDbContext _context;

                public QueryHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<Res> Handle(Query query, CancellationToken cancellationToken)
                {
                    var socios = await _context.Socio.Where(x => !x.IsSoftDeleted).CountAsync();
                    var clientes = await _context.Cliente.Where(x => !x.IsSoftDeleted).CountAsync();
                    var prestamosPIM = await _context.Prestamo.Where(x => !x.IsSoftDeleted).CountAsync();

                    var prestamosPIMpendientesAprobacion = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.Estado == EstadoPrestamo.PendienteAprobacion).CountAsync();
                    var prestamosPIMvigentes = await _context.Prestamo.Where(x => !x.IsSoftDeleted && x.Estado == EstadoPrestamo.Vigente).CountAsync();

                    var newRes = new Res
                    {
                        CantidadClientes = clientes,
                        CantidadSocios = socios,
                        CantidadPrestamosPIA = prestamosPIM,
                        CantidadPrestamosPIM = prestamosPIM,

                        CantidadPrestamosPIMPendientesAprobacion = prestamosPIMpendientesAprobacion,
                        CantidadPrestamosPIMVigente = prestamosPIMvigentes
                    };
                    return newRes;
                }
            }
        }




        #region MoverDineroDeUnaCuentaAOtraEnCajaV1
        //public class MoverDineroDeUnaCuentaAOtraEnCajaV1
        //{
        //    public class CommandAR : IRequest<AppResult>
        //    {
        //        public decimal CantidadAMover { get; set; }
        //        public Guid? CuentaBancariaOrigenId { get; set; }
        //        public Guid? CuentaBancariaDestinoId { get; set; }
        //        public string Motivo { get; set; }
        //        public string Observacion { get; set; }
        //    }

        //    public class CommandHandler : IRequestHandler<CommandAR, AppResult>
        //    {
        //        private readonly CooperativaDbContext _context;
        //        private readonly IUtilidadesBase _utilidadesBase;
        //        public CommandHandler(CooperativaDbContext context, IUtilidadesBase utilidadesBase)
        //        {
        //            _context = context;
        //            _utilidadesBase = utilidadesBase;

        //        }

        //        public async Task<AppResult> Handle(CommandAR cmd, CancellationToken cancellationToken)
        //        {
        //            try
        //            {
        //                //var usuario = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");
        //                var primerSocio = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
        //                var usuario = primerSocio.Id;

        //                var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaOrigenId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
        //                cuentaOrigen.ThrowIfNull("Cuenta Origen no existe");
        //                var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaDestinoId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
        //                cuentaDestino.ThrowIfNull("Cuenta Destino no existe");

        //                if (cuentaOrigen.CajaId != cuentaDestino.CajaId)
        //                {
        //                    throw new Exception("Caja relacionada a Cuenta Origen es distinta a la de la Cuenta Destino");
        //                }

        //                if (cuentaOrigen.SaldoActual < cmd.CantidadAMover)
        //                {
        //                    throw new Exception("No hay dinero suficiente en Cuenta Origen para hacer movimiento solicitado");
        //                }

        //                var caja = await _context.Caja.Where(x => x.Id == cuentaOrigen.CajaId && !x.IsSoftDeleted == x.Enabled).FirstOrDefaultAsync();
        //                caja.ThrowIfNull("No se encontro Caja para Empresa");

        //                var tipoRetiro = TipoRetiro.MovimientoEntreCuentasCaja;

        //                #region Correlativo de NumeroRetiro
        //                var fechaActual = DateTime.Now;
        //                int anio = fechaActual.Year % 100; // Obtener los últimos dos dígitos del año
        //                int mes = fechaActual.Month;

        //                var correlativoBase = $"RTR-{(int)tipoRetiro}-{mes}{anio}"; // Formatear el número de retiro con ceros a la izquierda

        //                var secuencial = await _utilidadesBase.GenerarSecuencial("Retiro", correlativoBase);
        //                var correlativoB = $"{correlativoBase}-{secuencial:D5}";
        //                #endregion




        //                var newRetiro = Retiro.New(correlativoB, caja.Id, usuario, null, cuentaDestino.Id, tipoRetiro, cmd.CantidadAMover, cmd.Motivo, cmd.Observacion, "HNL", usuario);
        //                newRetiro.ActualizarEstado(EstadoRetiro.Aprobado, usuario, "Aprobado automaticamente");
        //                await _context.Retiro.AddAsync(newRetiro);
        //                //await _context.SaveChangesAsync();

        //                //No es necesario crear el ingreso o egreso (en este caso me tocaria crear ambas)
        //                //var newEgreso = Egreso.NewPorRetiro(0, newRetiro.NumeroRetiro, newRetiro.Monto, newRetiro.Motivo, newRetiro.Observacion, newRetiro.Id, usuario);
        //                //await _context.Egreso.AddAsync(newEgreso);


        //                int numTransaccion = await _context.Transaccion.CountAsync(); // Obtener el total de retiros existentes

        //                var newTransaccion = Transaccion.New(numTransaccion + 1, newRetiro.NumeroRetiro, newRetiro.Monto, cuentaOrigen.Id, newRetiro.CuentaBancariaId, usuario);
        //                newTransaccion.CajaId = caja.Id;

        //                newTransaccion.SaldoCajaEnElMomento = caja.SaldoActual;
        //                newTransaccion.SaldoQuedaEnCaja = caja.SaldoActual;


        //                cuentaOrigen.RestarMovimiento(newRetiro.Monto);                 //Caja Origen
        //                cuentaDestino.SumarMovimiento(newRetiro.Monto);                 //Caja Destino

        //                caja.UltimaTransaccionId = newTransaccion.Id;
        //                caja.ModifiedDate = DateTime.Now;

        //                await _context.Transaccion.AddAsync(newTransaccion);



        //                //Asi como no creo Ingreso ni Egreso, entonces no es necesario mhacer movimientos enc aja porque seria el mismo dinero solo que se movio cierta cantidad de una cuenta a otra
        //                //var saldoActual = caja.SaldoActual;
        //                //var nuevoSaldo = caja.SaldoActual - newEgreso.Monto;
        //                //caja.ActualizarSaldo(saldoActual, nuevoSaldo, newTransaccion.Id, usuario);
        //                //newTransaccion.SaldoCajaEnElMomento = saldoActual;
        //                //newTransaccion.SaldoQuedaEnCaja = nuevoSaldo;


        //                await _context.SaveChangesAsync();


        //                return AppResult.New(true, $"Dinero moviendo entre cuentas exitosamente");
        //            }
        //            catch (Exception ex)
        //            {
        //                return AppResult.New(false, ex.Message);
        //            }
        //        }
        //    }
        //}

        #endregion




        public class MoverDineroDeUnaCuentaAOtraEnCaja
        {
            public class CommandAR : IRequest<AppResult>
            {
                public decimal CantidadAMover { get; set; }
                public string Referencia { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public Guid? CuentaBancariaDestinoId { get; set; }
                public string Motivo { get; set; }
                //public string Observacion { get; set; }
            }

            public class CommandHandler : IRequestHandler<CommandAR, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _utilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase utilidadesBase)
                {
                    _context = context;
                    _utilidadesBase = utilidadesBase;

                }

                public async Task<AppResult> Handle(CommandAR cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var primerSocio = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        var usuario = primerSocio.Id;

                        var cuentaOrigen = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaOrigenId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        cuentaOrigen.ThrowIfNull("Cuenta Origen no existe");
                        var cuentaDestino = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaDestinoId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        cuentaDestino.ThrowIfNull("Cuenta Destino no existe");

                        if (cuentaOrigen.CajaId != cuentaDestino.CajaId)
                        {
                            throw new Exception("Caja relacionada a Cuenta Origen es distinta a la de la Cuenta Destino");
                        }

                        if (cuentaOrigen.SaldoActual < cmd.CantidadAMover)
                        {
                            throw new Exception("No hay dinero suficiente en Cuenta Origen para hacer movimiento solicitado");
                        }

                        var caja = await _context.Caja.Where(x => x.Id == cuentaOrigen.CajaId && !x.IsSoftDeleted == x.Enabled).FirstOrDefaultAsync();
                        caja.ThrowIfNull("No se encontro Caja para Empresa");

                        var observacionMovimiento = $"Movimiento entre cuentas de Caja: {cuentaOrigen.NumeroCuenta} - {cuentaDestino.NumeroCuenta}";

                        int numTransaccion = await _context.Transaccion.CountAsync(); // Obtener el total de retiros existentes

                        var newTransaccion = Transaccion.NewMovimientoEntreCuenta(numTransaccion + 1, cmd.Referencia, cmd.CantidadAMover, caja.Id, cuentaOrigen.Id, cuentaDestino.Id, cmd.Motivo, usuario);
                        newTransaccion.CajaId = caja.Id;

                        newTransaccion.SaldoCajaEnElMomento = caja.SaldoActual;
                        newTransaccion.SaldoQuedaEnCaja = caja.SaldoActual;
                        newTransaccion.MovimientoEntreCuenta.Observacion = observacionMovimiento;

                        cuentaOrigen.RestarMovimiento(newTransaccion.Monto);                 //Caja Origen
                        cuentaDestino.SumarMovimiento(newTransaccion.Monto);                 //Caja Destino

                        caja.UltimaTransaccionId = newTransaccion.Id;
                        caja.ModifiedDate = DateTime.Now;

                        await _context.Transaccion.AddAsync(newTransaccion);

                        await _context.SaveChangesAsync();


                        return AppResult.New(true, $"Dinero moviendo entre cuentas exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }


























        public class CajaVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public Guid ResponsableId { get; set; }     //Seria por identity u UsuarioId
            public string Referencia { get; set; }      //Seria el Correlativo Ingreso o de Egreso
            public double SaldoAnterior { get; set; }
            public double SaldoActual { get; set; }
            public string Moneda_Descripcion { get; set; }
            public ModuloCaja Modulo { get; set; }
            public string Modulo_Descripcion { get; set; }
            //public IngresoVm Ingreso { get; set; }                    //Ya no conectado directamente con Caja, ahora va con Trasaccion
            //public EgresoVm Egreso { get; set; }
            public List<CuentaBancariaVm> CuentaBancarias { get; set; }
        }



        public class IngresoVm
        {
            public Guid Id { get; set; }
            public int NumeroIngreso { get; set; }
            public string Correlativo { get; set; }
            public double Monto { get; set; }
            public string Moneda_Descripcion { get; set; }
            public string Motivo { get; set; }
            public string Observacion { get; set; }
        }
        public class EgresoVm
        {
            public Guid Id { get; set; }
            public int NumeroIngreso { get; set; }
            public string Correlativo { get; set; }
            public double Monto { get; set; }
            public string Moneda_Descripcion { get; set; }
            public string Motivo { get; set; }
            public string Observacion { get; set; }
        }

        public class TransaccionVm
        {
            public Guid Id { get; set; }
            public Guid? CajaId { get; set; }
            public int NumeroTransaccion { get; set; }
            public string Referencia { get; set; }
            public TipoTransaccion TipoTransaccion { get; set; }
            public decimal Monto { get; set; }
            public string Moneda_Descripcion { get; set; }
            public decimal SaldoCajaEnElMomento { get; set; }
            public decimal SaldoQuedaEnCaja { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public Guid? CuentaBancariaOrigenId { get; set; }
            public Guid? CuentaBancariaDestinoId { get; set; }
            public Guid? IngresoId { get; set; }
            public Guid? EgresoId { get; set; }
        }






    }
}

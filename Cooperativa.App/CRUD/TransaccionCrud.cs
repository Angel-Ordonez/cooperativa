using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.CajaCrud;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;

namespace Cooperativa.App.CRUD
{
    public class TransaccionCrud
    {

        public class Index
        {
            public class QueryIndexTransaccion : IRequest<List<TransaccionVm>>
            {

            }

            public class QueryTransaccionHandler : IRequestHandler<QueryIndexTransaccion, List<TransaccionVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryTransaccionHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<TransaccionVm>> Handle(QueryIndexTransaccion query, CancellationToken cancellationToken)
                {

                    var transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted)
                        .AsNoTracking()
                        .OrderByDescending(x => x.CreatedDate)
                        .ProjectToType<TransaccionVm>()
                        .ToListAsync();

                    List<Guid> prestamosId = new List<Guid>();
                    List<Guid> detallesPrestamosId = new List<Guid>();
                    List<Guid> inversionesId = new List<Guid>();
                    List<Guid> ingresosId = new List<Guid>();
                    List<Guid> egresosId = new List<Guid>();


                    foreach (var t in transacciones)
                    {
                        var ingreso = t.Ingreso;
                        var egreso = t.Egreso;

                        if (ingreso != null)
                        {
                            if (ingreso.PrestamoDetalleId != null && ingreso.PrestamoDetalleId != Guid.Empty)
                            {
                                detallesPrestamosId.Add((Guid)ingreso.PrestamoDetalleId);
                            }
                            else if (ingreso.SocioInversionId != null && ingreso.SocioInversionId != Guid.Empty)
                            {
                                inversionesId.Add((Guid)ingreso.SocioInversionId);
                            }

                            ingresosId.Add(ingreso.Id);
                        }

                        if (egreso != null)
                        {
                            if (egreso.PrestamoId != null && egreso.PrestamoId != Guid.Empty)
                            {
                                prestamosId.Add(egreso.PrestamoId); 
                            }
                            egresosId.Add(egreso.Id);
                        }
                    }

                    var prestamos = await _context.Prestamo.Where(x => prestamosId.Contains(x.Id)).AsNoTracking().ToListAsync();
                    var detallesPrestamos = await _context.PrestamoDetalle.Where(x => detallesPrestamosId.Contains(x.Id)).Include(x => x.Prestamo).AsNoTracking().ToListAsync();
                    var sociosInversiones = await _context.SocioInversion.Where(x => inversionesId.Contains(x.Id)).AsNoTracking().ToListAsync();
                    var cajas = await _context.Caja.Where(x => ingresosId.Contains((Guid)x.IngresoId) || egresosId.Contains((Guid)x.EgresoId)).AsNoTracking().ToListAsync();

                    foreach (var t in transacciones)
                    {
                        var ingreso = t.Ingreso;
                        var egreso = t.Egreso;

                        if (ingreso!=null)
                        {
                            var caja = cajas.Where(x => x.IngresoId == ingreso.Id).FirstOrDefault();
                            ingreso.SaldoCaja = (caja != null) ? caja.SaldoActual : 0;

                            if (ingreso.PrestamoDetalleId!=null && ingreso.PrestamoDetalleId != Guid.Empty)
                            {
                                var prestamoDetalle = detallesPrestamos.Where(x => x.Id == ingreso.PrestamoDetalleId).FirstOrDefault();

                                if (prestamoDetalle != null)
                                {
                                    ingreso.CodigoRazon = prestamoDetalle.Prestamo.CodigoPrestamo;
                                }
                            }
                            else if (ingreso.SocioInversionId != null && ingreso.SocioInversionId != Guid.Empty)
                            {
                                var socioInversion = sociosInversiones.Where(x => x.Id == ingreso.SocioInversionId).FirstOrDefault();
                                if (socioInversion != null) 
                                {
                                    ingreso.CodigoRazon = socioInversion.CodigoInversion;
                                }
                            }
                        }

                        if (egreso != null)
                        {
                            var caja = cajas.Where(x => x.EgresoId == egreso.Id).FirstOrDefault();
                            egreso.SaldoCaja = (caja != null) ? caja.SaldoActual : 0;

                            if (egreso.PrestamoId!=null && egreso.PrestamoId != Guid.Empty)
                            {
                                var prestamo = prestamos.Where(x => x.Id == egreso.PrestamoId).FirstOrDefault();

                                if (prestamo != null)
                                {
                                    egreso.CodigoPrestamo = prestamo.CodigoPrestamo;
                                }
                            }
                        }
                    }

                    return transacciones;

                }


            }


        }






        public class GetTransaccionesByAnio
        {
            public class QueryIndexTransaccion : IRequest<List<TransaccionVm>>
            {
                public int Anio { get; set; }
            }

            public class QueryTransaccionHandler : IRequestHandler<QueryIndexTransaccion, List<TransaccionVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryTransaccionHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<TransaccionVm>> Handle(QueryIndexTransaccion query, CancellationToken cancellationToken)
                {

                    var transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.Anio)
                        .AsNoTracking()
                        .OrderByDescending(x => x.CreatedDate)
                        .ProjectToType<TransaccionVm>()
                        .ToListAsync();

                    List<Guid> prestamosId = new List<Guid>();
                    List<Guid> detallesPrestamosId = new List<Guid>();
                    List<Guid> inversionesId = new List<Guid>();
                    List<Guid> ingresosId = new List<Guid>();
                    List<Guid> egresosId = new List<Guid>();


                    foreach (var t in transacciones)
                    {
                        var ingreso = t.Ingreso;
                        var egreso = t.Egreso;

                        if (ingreso != null)
                        {
                            if (ingreso.PrestamoDetalleId != null && ingreso.PrestamoDetalleId != Guid.Empty)
                            {
                                detallesPrestamosId.Add((Guid)ingreso.PrestamoDetalleId);
                            }
                            else if (ingreso.SocioInversionId != null && ingreso.SocioInversionId != Guid.Empty)
                            {
                                inversionesId.Add((Guid)ingreso.SocioInversionId);
                            }

                            ingresosId.Add(ingreso.Id);
                        }

                        if (egreso != null)
                        {
                            if (egreso.PrestamoId != null && egreso.PrestamoId != Guid.Empty)
                            {
                                prestamosId.Add(egreso.PrestamoId);
                            }
                            egresosId.Add(egreso.Id);
                        }
                    }

                    var prestamos = await _context.Prestamo.Where(x => prestamosId.Contains(x.Id)).AsNoTracking().ToListAsync();
                    var detallesPrestamos = await _context.PrestamoDetalle.Where(x => detallesPrestamosId.Contains(x.Id)).Include(x => x.Prestamo).AsNoTracking().ToListAsync();
                    var sociosInversiones = await _context.SocioInversion.Where(x => inversionesId.Contains(x.Id)).AsNoTracking().ToListAsync();
                    var cajas = await _context.Caja.Where(x => ingresosId.Contains((Guid)x.IngresoId) || egresosId.Contains((Guid)x.EgresoId)).AsNoTracking().ToListAsync();

                    foreach (var t in transacciones)
                    {
                        var ingreso = t.Ingreso;
                        var egreso = t.Egreso;

                        if (ingreso != null)
                        {
                            var caja = cajas.Where(x => x.IngresoId == ingreso.Id).FirstOrDefault();
                            ingreso.SaldoCaja = (caja != null) ? caja.SaldoActual : 0;

                            if (ingreso.PrestamoDetalleId != null && ingreso.PrestamoDetalleId != Guid.Empty)
                            {
                                var prestamoDetalle = detallesPrestamos.Where(x => x.Id == ingreso.PrestamoDetalleId).FirstOrDefault();

                                if (prestamoDetalle != null)
                                {
                                    ingreso.CodigoRazon = prestamoDetalle.Prestamo.CodigoPrestamo;
                                }
                            }
                            else if (ingreso.SocioInversionId != null && ingreso.SocioInversionId != Guid.Empty)
                            {
                                var socioInversion = sociosInversiones.Where(x => x.Id == ingreso.SocioInversionId).FirstOrDefault();
                                if (socioInversion != null)
                                {
                                    ingreso.CodigoRazon = socioInversion.CodigoInversion;
                                }
                            }
                        }

                        if (egreso != null)
                        {
                            var caja = cajas.Where(x => x.EgresoId == egreso.Id).FirstOrDefault();
                            egreso.SaldoCaja = (caja != null) ? caja.SaldoActual : 0;

                            if (egreso.PrestamoId != null && egreso.PrestamoId != Guid.Empty)
                            {
                                var prestamo = prestamos.Where(x => x.Id == egreso.PrestamoId).FirstOrDefault();

                                if (prestamo != null)
                                {
                                    egreso.CodigoPrestamo = prestamo.CodigoPrestamo;
                                }
                            }
                        }
                    }

                    return transacciones;

                }


            }


        }





        public class GetTransacciones
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
            public class QueryTransacciones : IRequest<List<TransaccionVm>>
            {
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
            }

            public class QueryTransaccionHandler : IRequestHandler<QueryTransacciones, List<TransaccionVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryTransaccionHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<TransaccionVm>> Handle(QueryTransacciones query, CancellationToken cancellationToken)
                {


                    List<TransaccionVm> transacciones = new List<TransaccionVm>();

                    if (query.Index)
                    {
                        transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionVm>()
                            .ToListAsync();
                    }
                    else if(query.Anio != 0)
                    {
                        transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.Anio)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionVm>()
                            .ToListAsync();
                    }
                    else if(query.AnioMes != null)
                    {
                        transacciones = await _context.Transaccion.Where(x => x.CreatedDate.Date.Year == query.AnioMes.Anio && x.CreatedDate.Date.Month == query.AnioMes.Mes && !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionVm>()
                            .ToListAsync();
                    }
                    else if (query.RangoFechas != null)
                    {
                        var inicio = query.RangoFechas.FechaInicio.Date;
                        var fin = query.RangoFechas.FechaFin.Date;

                        transacciones = await _context.Transaccion.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin && !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionVm>()
                            .ToListAsync();
                    }




                    List<Guid> prestamosId = new List<Guid>();
                    List<Guid> detallesPrestamosId = new List<Guid>();
                    List<Guid> inversionesId = new List<Guid>();
                    List<Guid> ingresosId = new List<Guid>();
                    List<Guid> egresosId = new List<Guid>();


                    foreach (var t in transacciones)
                    {
                        var ingreso = t.Ingreso;
                        var egreso = t.Egreso;

                        if (ingreso != null)
                        {
                            if (ingreso.PrestamoDetalleId != null && ingreso.PrestamoDetalleId != Guid.Empty)
                            {
                                detallesPrestamosId.Add((Guid)ingreso.PrestamoDetalleId);
                            }
                            else if (ingreso.SocioInversionId != null && ingreso.SocioInversionId != Guid.Empty)
                            {
                                inversionesId.Add((Guid)ingreso.SocioInversionId);
                            }

                            ingresosId.Add(ingreso.Id);
                        }

                        if (egreso != null)
                        {
                            if (egreso.PrestamoId != null && egreso.PrestamoId != Guid.Empty)
                            {
                                prestamosId.Add(egreso.PrestamoId);
                            }
                            egresosId.Add(egreso.Id);
                        }
                    }

                    var prestamos = await _context.Prestamo.Where(x => prestamosId.Contains(x.Id)).AsNoTracking().ToListAsync();
                    var detallesPrestamos = await _context.PrestamoDetalle.Where(x => detallesPrestamosId.Contains(x.Id)).Include(x => x.Prestamo).AsNoTracking().ToListAsync();
                    var sociosInversiones = await _context.SocioInversion.Where(x => inversionesId.Contains(x.Id)).AsNoTracking().ToListAsync();
                    var cajas = await _context.Caja.Where(x => ingresosId.Contains((Guid)x.IngresoId) || egresosId.Contains((Guid)x.EgresoId)).AsNoTracking().ToListAsync();

                    foreach (var t in transacciones)
                    {
                        var ingreso = t.Ingreso;
                        var egreso = t.Egreso;

                        if (ingreso != null)
                        {
                            var caja = cajas.Where(x => x.IngresoId == ingreso.Id).FirstOrDefault();
                            ingreso.SaldoCaja = (caja != null) ? caja.SaldoActual : 0;

                            if (ingreso.PrestamoDetalleId != null && ingreso.PrestamoDetalleId != Guid.Empty)
                            {
                                var prestamoDetalle = detallesPrestamos.Where(x => x.Id == ingreso.PrestamoDetalleId).FirstOrDefault();

                                if (prestamoDetalle != null)
                                {
                                    ingreso.CodigoRazon = prestamoDetalle.Prestamo.CodigoPrestamo;
                                }
                            }
                            else if (ingreso.SocioInversionId != null && ingreso.SocioInversionId != Guid.Empty)
                            {
                                var socioInversion = sociosInversiones.Where(x => x.Id == ingreso.SocioInversionId).FirstOrDefault();
                                if (socioInversion != null)
                                {
                                    ingreso.CodigoRazon = socioInversion.CodigoInversion;
                                }
                            }
                        }

                        if (egreso != null)
                        {
                            var caja = cajas.Where(x => x.EgresoId == egreso.Id).FirstOrDefault();
                            egreso.SaldoCaja = (caja != null) ? caja.SaldoActual : 0;

                            if (egreso.PrestamoId != null && egreso.PrestamoId != Guid.Empty)
                            {
                                var prestamo = prestamos.Where(x => x.Id == egreso.PrestamoId).FirstOrDefault();

                                if (prestamo != null)
                                {
                                    egreso.CodigoPrestamo = prestamo.CodigoPrestamo;
                                }
                            }
                        }
                    }

                    return transacciones;

                }


            }


        }




        public class GetTransaccionesV2
        {
            public class CajaRes
            {
                public Guid Id { get; set; }
                public string Referencia { get; set; }
                public double SaldoAnterior { get; set; }
                public double SaldoActual { get; set; }
                public ModuloCaja Modulo { get; set; }
                public string Modulo_Descripcion { get; set; }
            }
            public class TransaccionRes
            {
                public Guid Id { get; set; }
                public int NumeroTransaccion { get; set; }
                public string Referencia { get; set; }
                public TipoTransaccion TipoTransaccion { get; set; }
                public decimal Monto { get; set; }
                public string Moneda_Descripcion { get; set; }
                public decimal SaldoCajaEnElMomento { get; set; }
                public decimal SaldoQuedaEnCaja { get; set; }
                public DateTime CreatedDate { get; set; }
                public DateTime ModifiedDate { get; set; }
                public Guid? IngresoId { get; set; }
                public Guid? EgresoId { get; set; }
                public CuentaBancariaVm CuentaBancariaOrigen { get; set; }
                public CuentaBancariaVm CuentaBancariaDestino { get; set; }
            }
            public class Respuesta
            {
                public CajaRes Caja { get; set; }
                public List<TransaccionRes> Transacciones { get; set; }
            }

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
            public class QueryTransacciones : IRequest<Respuesta>
            {
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
            }

            public class QueryTransaccionHandler : IRequestHandler<QueryTransacciones, Respuesta>
            {
                private readonly CooperativaDbContext _context;

                public QueryTransaccionHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<Respuesta> Handle(QueryTransacciones query, CancellationToken cancellationToken)
                {


                    List<TransaccionRes> transacciones = new List<TransaccionRes>();

                    if (query.Index)
                    {
                        transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionRes>()
                            .ToListAsync();
                    }
                    else if (query.Anio != 0)
                    {
                        transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted && x.CreatedDate.Year == query.Anio)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionRes>()
                            .ToListAsync();
                    }
                    else if (query.AnioMes != null)
                    {
                        transacciones = await _context.Transaccion.Where(x => x.CreatedDate.Date.Year == query.AnioMes.Anio && x.CreatedDate.Date.Month == query.AnioMes.Mes && !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionRes>()
                            .ToListAsync();
                    }
                    else if (query.RangoFechas != null)
                    {
                        var inicio = query.RangoFechas.FechaInicio.Date;
                        var fin = query.RangoFechas.FechaFin.Date;

                        transacciones = await _context.Transaccion.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin && !x.IsSoftDeleted)
                            .AsNoTracking()
                            .OrderByDescending(x => x.CreatedDate)
                            .ProjectToType<TransaccionRes>()
                            .ToListAsync();
                    }

                    var caja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .OrderByDescending(x => x.CreatedDate)
                        .ProjectToType<CajaRes>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                    var respuesta = new Respuesta
                    {
                        Caja = caja,
                        Transacciones = transacciones
                    };

                    return respuesta;

                }
            }
        }





        public class MantenimientoSaldoAnteriorCaja //Decidi guardar el saldo que tenia la caja en ese momento
        {
            public class QueryIndexTransaccion : IRequest<AppResult>
            {

            }

            public class QueryTransaccionHandler : IRequestHandler<QueryIndexTransaccion, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public QueryTransaccionHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(QueryIndexTransaccion query, CancellationToken cancellationToken)
                {

                    try
                    {
                        var transacciones = await _context.Transaccion.Where(x => !x.IsSoftDeleted).ToListAsync();
                        var cajas = await _context.Caja.Where(x => !x.IsSoftDeleted).ToListAsync();
                        int cuantos = 0;
                        foreach (var transaccion in transacciones)
                        {
                            if (transaccion.IngresoId != null && transaccion.IngresoId != Guid.Empty)
                            {
                                var caja = cajas.Where(x => x.IngresoId == transaccion.IngresoId).FirstOrDefault();
                                if(caja != null)
                                {
                                    transaccion.SaldoCajaEnElMomento = caja.SaldoAnterior;
                                    transaccion.SaldoQuedaEnCaja = caja.SaldoActual;
                                    transaccion.CajaId = caja.Id;       //Este no era necesario, pero como necesitaba no eprder la informacion de lo que ya habia trabajado
                                    cuantos++;
                                }

                            }

                            if (transaccion.EgresoId != null && transaccion.EgresoId != Guid.Empty)
                            {
                                var caja = cajas.Where(x => x.EgresoId == transaccion.EgresoId).FirstOrDefault();
                                if (caja != null)
                                {
                                    transaccion.SaldoCajaEnElMomento = caja.SaldoAnterior;
                                    transaccion.SaldoQuedaEnCaja = caja.SaldoActual;
                                    transaccion.CajaId = caja.Id;       //Este no era necesario, pero como necesitaba no eprder la informacion de lo que ya habia trabajado
                                    cuantos++;
                                }

                            }
                        }

                        await _context.SaveChangesAsync();

                        return AppResult.New(true, $"Se actualizaron {cuantos} transacciones");
                    }
                    catch(Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }











        public class TransaccionVm
        {
            public Guid Id { get; set; }
            public int NumeroTransaccion { get; set; }
            public string Referencia { get; set; }
            public TipoTransaccion TipoTransaccion { get; set; }
            public decimal Monto { get; set; }
            public string Moneda_Descripcion { get; set; }
            public decimal SaldoCajaEnElMomento { get; set; }
            public decimal SaldoQuedaEnCaja { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public Guid? CajaId { get; set; }
            public CuentaBancariaVm CuentaBancariaOrigen { get; set; }
            public Guid? CuentaBancariaOrigenId { get; set; }
            public CuentaBancariaVm CuentaBancariaDestino { get; set; }
            public Guid? CuentaBancariaDestinoId { get; set; }

            public virtual IngresoVm Ingreso { get; set; }
            public Guid? IngresoId { get; set; }
            public virtual EgresoVm Egreso { get; set; }
            public Guid? EgresoId { get; set; }            
            public string MovimientoEntreCuentaReferencia { get; set; }
            public string MovimientoEntreCuentaMotivo { get; set; }
            public string MovimientoEntreCuentaObservacion { get; set; }
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
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public Guid? PrestamoDetalleId { get; set; }
            public Guid? SocioInversionId { get; set; }
            public string CodigoRazon { get; set; }
            public decimal SaldoCaja { get; set; }

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
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public Guid PrestamoId { get; set; }
            public string CodigoPrestamo { get; set; }
            public decimal SaldoCaja { get; set; }
        }




    }








}

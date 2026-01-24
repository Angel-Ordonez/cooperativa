using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cooperativa.App.Utilidades;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using static Cooperativa.App.Domain.Model.Entidad.RetornoCliente;
using Mapster;
using static Cooperativa.App.CRUD.PrestamoCrud;
using System.Dynamic;

namespace Cooperativa.App.CRUD
{
    public class RetornoClienteCrud
    {

        public class Crear
        {
            public class Command : IRequest<AppResult>
            {
                public Guid PrestamoId { get; set; }
                public Guid ClienteId { get; set; }
                public decimal Cantidad { get; set; }
                //public Guid? CajaId { get; set; }
                public Guid? SolicitanteId { get; set; }
                public Guid CuentaBancariaId { get; set; }
                public string Motivo { get; set; }  //Lo ocupo para Retiro
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var socioTemporal = await _context.Socio.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();

                        var createdBy = socioTemporal.Id;
                        cmd.SolicitanteId = createdBy;

                        var cliente = await _context.Cliente.Where(x => x.Id == cmd.ClienteId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        cliente.ThrowIfNull("Persona no existe");
                        var prestamo = await _context.Prestamo.Where(x => x.Id == cmd.PrestamoId && !x.IsSoftDeleted).FirstOrDefaultAsync();
                        prestamo.ThrowIfNull("Prestamo no existe");

                        if (prestamo.Estado != EstadoPrestamo.Pagado)
                        {
                            throw new Exception("Prestamo aun esta Vigente");
                        }
                        if (cmd.Cantidad > prestamo.Ganancia)
                        {
                            throw new Exception("Cantidad Devolucion es mayor a la ganancia del prestamo");
                        }

                        var retornoPendiente = await _context.RetornoCliente.Where(x => x.PrestamoId == cmd.PrestamoId && !x.IsSoftDeleted && x.Estado == EstadoMovimientoPreAprobacion.Pendiente)
                            .Include(x => x.Retiro)
                            .FirstOrDefaultAsync();
                        if(retornoPendiente != null)
                        {
                            throw new Exception($"Cliente tiene un Retorno en estado Pendiente: {retornoPendiente.Retiro.NumeroRetiro}");
                        }


                        var caja = await _context.Caja.Where(x => !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync(); //Traera la unica habilitada por mientras
                        if (caja == null) { throw new Exception("No se encontro Caja!"); }

                        if (caja.SaldoActual < cmd.Cantidad)
                        {
                            throw new Exception("No hay saldo suficiente en caja para gestion de Devolucion");
                        }

                        var cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == cmd.CuentaBancariaId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        cuentaBancaria.ThrowIfNull("Cuenta Bancaria Destino no existe");

                        var solicitante = await _context.Socio.Where(x => x.Id == cmd.SolicitanteId && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                        solicitante.ThrowIfNull("Persona que Solicita no existe o no tiene permiso para realziar esta gestion");


                        #region Crear Retiro

                        #region Correlativo de NumeroRetiro
                        var anio = DateTime.Now.Year % 100;
                        var codigoEmpresa = "COOPAZ";

                        var codigoBaseSecuencial = $"RTR{(int)TipoRetiro.Devolucion}-{codigoEmpresa}{anio}";
                        var secuencial = await _iUtilidadesBase.GenerarSecuencial("Retiro", codigoBaseSecuencial);
                        if (secuencial <= 0)
                            return AppResult.New(false, "Error al generar secuencial de socio.");

                        var codigoRetiro = $"{codigoBaseSecuencial}-{secuencial:D4}";
                        #endregion

                        if(cmd.Motivo == null || cmd.Motivo.Length < 1)
                        {
                            cmd.Motivo = $"Retorno a cliente sobre prestamo {prestamo.CodigoPrestamo}";
                        }

                        var newRetiro = Retiro.New(codigoRetiro, caja.Id, solicitante.Id, null, cuentaBancaria.Id, TipoRetiro.Devolucion, cmd.Cantidad, cmd.Motivo, "", "", (Guid)cmd.SolicitanteId);
                        await _context.Retiro.AddAsync(newRetiro);

                        #endregion



                        var newRetornoClientePendiente = RetornoCliente.New(cliente.Id, prestamo.Id, cmd.Cantidad, newRetiro.Id, createdBy);


                        await _context.RetornoCliente.AddAsync(newRetornoClientePendiente);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, "Retorno Cliente pendiente creado exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }



        public class Atender
        {
            public class Command : IRequest<AppResult>
            {
                public decimal CantidadAprobada { get; set; }   //Aqui podria utilizar el mismo modal, solo que activo el input si el tipoRetiro es este y en ts tambien lo valido y consumo este metodo
                public Guid RetiroId { get; set; }
                public EstadoRetiro Estado { get; set; }
                public Guid? CuentaBancariaOrigenId { get; set; }
                public string Observacion { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");



                        return AppResult.New(true, "Retorno Cliente pendiente creado exitosamente");
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
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;
                }

                public async Task<AppResult> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    try
                    {
                        var createdBy = new Guid("70E11ECF-657F-4AE8-A431-08DBA69C704A");

                        var idsDistinc = cmd.Ids.Distinct().ToList();

                        var retornos = await _context.RetornoCliente.Where(x => idsDistinc.Contains(x.ClienteId)).ToListAsync();

                        if(idsDistinc.Count() != retornos.Count())
                        {
                            for(int i =0; i<idsDistinc.Count(); i++)
                            {
                                var item = idsDistinc.ElementAt(i);
                                var existe = retornos.Where(x => x.Id == item).FirstOrDefault();
                                if (existe == null)
                                {
                                    throw new Exception($"No existe RetornoCliente con Id: {item}");
                                }
                                if(existe.Estado == EstadoMovimientoPreAprobacion.Aprobado)
                                {
                                    throw new Exception($"RetornoCliente con Id: {item} esta aprobado no es posible eliminarlo");
                                }
                                if (existe.IsSoftDeleted)
                                {
                                    throw new Exception($"RetornoCliente con Id: {item} ya esta eliminado");
                                }
                            }
                        }


                        foreach(var item in retornos)
                        {
                            item.IsSoftDeleted = true;
                            item.ModifiedBy = createdBy;
                            item.ModifiedDate = DateTime.Now;
                        }
                        await _context.SaveChangesAsync();


                        return AppResult.New(true, $"Se elimino {retornos.Count()} Retorno Cliente exitosamente");
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }






        public class GetByClientes
        {
            public class Command : IRequest<List<RetornoClienteVm>>
            {
                public List<Guid> ClienteIds { get; set; }
            }

            public class CommandHandler : IRequestHandler<Command, List<RetornoClienteVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;
                }

                public async Task<List<RetornoClienteVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var retornos = await _context.RetornoCliente.Where(x => cmd.ClienteIds.Contains(x.ClienteId) && !x.IsSoftDeleted)
                        .ProjectToType<RetornoClienteVm>()
                        .ToListAsync();

                    return retornos;
                }
            }
        }



        public class GetPendientes
        {
            public class Command : IRequest<List<RetornoClienteVm>>
            {

            }

            public class CommandHandler : IRequestHandler<Command, List<RetornoClienteVm>>
            {
                private readonly CooperativaDbContext _context;
                private readonly IUtilidadesBase _iUtilidadesBase;
                public CommandHandler(CooperativaDbContext context, IUtilidadesBase iUtilidadesBase)
                {
                    _context = context;
                    _iUtilidadesBase = iUtilidadesBase;
                }

                public async Task<List<RetornoClienteVm>> Handle(Command cmd, CancellationToken cancellationToken)
                {
                    var retornos = await _context.RetornoCliente.Where(x => x.Estado == EstadoMovimientoPreAprobacion.Pendiente && !x.IsSoftDeleted)
                        .ProjectToType<RetornoClienteVm>()
                        .ToListAsync();

                    return retornos;
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
                public EstadoMovimientoPreAprobacion Estado { get; set; }
            }
            public class QueryByFiltros : IRequest<List<RetornoClienteVm>>
            {
                public bool Index { get; set; }
                public int Anio { get; set; }
                public AnioMesVm AnioMes { get; set; }
                public RangoFechasVm RangoFechas { get; set; }
                public EstadoInputVm Estado { get; set; }
            }

            public class QueryPrestamosByFiltrosHandler : IRequestHandler<QueryByFiltros, List<RetornoClienteVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryPrestamosByFiltrosHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<RetornoClienteVm>> Handle(QueryByFiltros query, CancellationToken cancellationToken)
                {
                    List<RetornoClienteVm> retornosClientes = new List<RetornoClienteVm>();


                    //Aqui no he ido a la base de datos, solo tengo armada la consulta: Select * from RetornoCLiente where IsSoftDeleted = false
                    var retornosQuery = _context.RetornoCliente.Where(x => !x.IsSoftDeleted).AsNoTracking();

                    if (query.Estado != null)
                    {
                        //Aqui agregi un filtro mas: Select * from RetornoCliente where IsSoftDeleted = false && Estado == query.Estado
                        retornosQuery = retornosQuery.Where(x => x.Estado == query.Estado.Estado);
                    }

                    if (query.Index)
                    {

                    }
                    else if (query.Anio != 0)
                    {
                        retornosQuery = retornosQuery.Where(x => x.CreatedDate.Year == query.Anio);
                    }
                    else if (query.AnioMes != null)
                    {
                        retornosQuery = retornosQuery.Where(x => x.CreatedDate.Year == query.AnioMes.Anio && x.CreatedDate.Month == query.AnioMes.Mes);
                    }
                    else if (query.RangoFechas != null)
                    {
                        var inicio = query.RangoFechas.FechaInicio.Date;
                        var fin = query.RangoFechas.FechaFin.Date;
                        retornosQuery = retornosQuery.Where(x => x.CreatedDate >= inicio && x.CreatedDate <= fin);
                    }


                    //Aqui oficialmente voy a la base da datos
                    retornosClientes = await retornosQuery
                        .ProjectToType<RetornoClienteVm>()
                        .ToListAsync();


                    return retornosClientes;
                }
            }
        }















    }
}

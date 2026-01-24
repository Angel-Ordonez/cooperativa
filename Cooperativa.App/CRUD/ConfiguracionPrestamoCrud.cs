using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Configuraciones;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.CajaCrud;
using static Cooperativa.App.Domain.Model.Configuraciones.ConfiguracionPrestamo;

namespace Cooperativa.App.CRUD
{
    public class ConfiguracionPrestamoCrud
    {

        //public class Create
        //{
        //    public class CommandConfiguracionPrestamo : IRequest<AppResult>
        //    {
        //        public bool TomarSocioInversion { get; set; }
        //        public bool PrimerMesInteresObligatorio { get; set; }
        //        public Guid CreatedBy { get; set; }
        //    }

        //    public class CommandConfiguracionPrestamoHandler : IRequestHandler<CommandConfiguracionPrestamo, AppResult>
        //    {
        //        private readonly CooperativaDbContext _context;

        //        public CommandConfiguracionPrestamoHandler(CooperativaDbContext context)
        //        {
        //            _context = context;
        //        }

        //        public async Task<AppResult> Handle(CommandConfiguracionPrestamo cmd, CancellationToken cancellationToken)
        //        {

        //            var configuraciones = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
        //            if(configuraciones.Any())
        //            {
        //                return AppResult.New(false, "Ya existe Registro para Configuracion de Prestamo");
        //            }


        //            try
        //            {
        //                var newConfig = ConfiguracionPrestamo.New(cmd.TomarSocioInversion, cmd.PrimerMesInteresObligatorio, cmd.CreatedBy);

        //                await _context.ConfiguracionPrestamo.AddAsync(newConfig);
        //                await _context.SaveChangesAsync();

        //                return AppResult.New(true, newConfig);
        //            }
        //            catch(Exception ex) 
        //            {
        //                return AppResult.New(false, $"ERROR: {ex.Message}");
        //            }



        //        }
        //    }
        //}




        public class Create
        {
            public class CommandConfiguracionPrestamo : IRequest<AppResult>
            {
                public bool TomarSocioInversion { get; set; }
                public bool PrimerMesInteresObligatorio { get; set; }
                public int CantidadPrestamosPorCliente { get; set; }
                public decimal InteresPIM { get; set; }
                public decimal InteresAnual { get; set; }
                public decimal InteresPorMora { get; set; }
                public decimal MontoMinimo { get; set; }
                public decimal MontoMaximo { get; set; }
                public decimal PlazoMesesMinimo { get; set; }
                public decimal PlazoMesesMaximo { get; set; }
                public int DiasDeGracias { get; set; }
                public Moneda MonedaPorDefecto { get; set; }
                public string Observacion { get; set; }
                public Guid CreatedBy { get; set; }
            }

            public class CommandConfiguracionPrestamoHandler : IRequestHandler<CommandConfiguracionPrestamo, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandConfiguracionPrestamoHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandConfiguracionPrestamo cmd, CancellationToken cancellationToken)
                {

                    var configuraciones = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled).AsNoTracking().ToListAsync();
                    if (configuraciones.Any())
                    {
                        return AppResult.New(false, "Ya existe Registro para Configuracion de Prestamo");
                    }


                    try
                    {
                        var newConfig = ConfiguracionPrestamo.New(
                                  cmd.TomarSocioInversion,
                                  cmd.PrimerMesInteresObligatorio,
                                  cmd.CantidadPrestamosPorCliente,
                                  cmd.InteresPIM,
                                  cmd.InteresAnual,
                                  cmd.InteresPorMora,
                                  cmd.MontoMinimo,
                                  cmd.MontoMaximo,
                                  cmd.PlazoMesesMinimo,
                                  cmd.PlazoMesesMaximo,
                                  cmd.DiasDeGracias,
                                  cmd.MonedaPorDefecto,
                                  cmd.Observacion,
                                  cmd.CreatedBy
                              );

                        await _context.ConfiguracionPrestamo.AddAsync(newConfig);
                        await _context.SaveChangesAsync();

                        return AppResult.New(true, newConfig);
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, $"ERROR: {ex.Message}");
                    }



                }
            }
        }






        public class UpdateViejo
        {
            public class CommandConfiguracionPrestamoU : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public bool TomarSocioInversion { get; set; }
                public bool PrimerMesInteresObligatorio { get; set; }
                public Guid ModifiendBy { get; set; }
            }

            public class CommandConfiguracionPrestamoHandler : IRequestHandler<CommandConfiguracionPrestamoU, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandConfiguracionPrestamoHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandConfiguracionPrestamoU cmd, CancellationToken cancellationToken)
                {

                    var configuracion = await _context.ConfiguracionPrestamo.Where(x => x.Id == cmd.Id && !x.IsSoftDeleted).FirstOrDefaultAsync();

                    int seCambio = 0;
                    try
                    {
                        if (configuracion == null) { throw new Exception("No existe ConfiguracionPrestamo con ese Id"); }

                        string queSeCambio = "Se edito ";

                        if(configuracion.TomarSocioInversion != cmd.TomarSocioInversion)
                        {
                            configuracion.TomarSocioInversion = cmd.TomarSocioInversion;
                            queSeCambio += ": TomarSocioInversion ";
                            seCambio++;
                        }
                        if(configuracion.TomarSocioInversion == cmd.PrimerMesInteresObligatorio)
                        {
                            configuracion.PrimerMesInteresObligatorio = cmd.PrimerMesInteresObligatorio;
                            queSeCambio += ": PrimerMesInteresObligatorio ";
                            seCambio++;
                        }
                        

                        if(seCambio > 0)
                        {
                            configuracion.ModifiedBy = cmd.ModifiendBy;
                            configuracion.ModifiedDate = DateTime.Now;
                            await _context.SaveChangesAsync();
                            return AppResult.New(true, queSeCambio);
                        }
                        else
                        {
                            return AppResult.New(false, "No hay nada que cambiar.");
                        }


                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, $"ERROR: {ex.Message}");
                    }



                }
            }
        }



        public class Update
        {
            public class CommandConfiguracionPrestamoU : IRequest<AppResult>
            {
                public Guid Id { get; set; }
                public bool TomarSocioInversion { get; set; }
                public bool PrimerMesInteresObligatorio { get; set; }
                public int CantidadPrestamosPorCliente { get; set; }
                public decimal InteresPIM { get; set; }
                public decimal InteresAnual { get; set; }
                public decimal InteresPorMora { get; set; }
                public decimal MontoMinimo { get; set; }
                public decimal MontoMaximo { get; set; }
                public decimal PlazoMesesMinimo { get; set; }
                public decimal PlazoMesesMaximo { get; set; }
                public int DiasDeGracias { get; set; }
                public Moneda MonedaPorDefecto { get; set; }
                public string Observacion { get; set; }
                //public Guid ModifiedBy { get; set; }
            }

            public class CommandConfiguracionPrestamoHandler : IRequestHandler<CommandConfiguracionPrestamoU, AppResult>
            {
                private readonly CooperativaDbContext _context;

                public CommandConfiguracionPrestamoHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<AppResult> Handle(CommandConfiguracionPrestamoU cmd, CancellationToken cancellationToken)
                {
                    var configuracion = await _context.ConfiguracionPrestamo
                        .Where(x => x.Id == cmd.Id && !x.IsSoftDeleted)
                        .FirstOrDefaultAsync();

                    if (configuracion == null)
                        return AppResult.New(false, "No existe ConfiguracionPrestamo con ese Id");

                    int seCambio = 0;
                    string queSeCambio = "Se editó:";

                    try
                    {
                        // Comparaciones y actualizaciones
                        if (configuracion.TomarSocioInversion != cmd.TomarSocioInversion)
                        {
                            configuracion.TomarSocioInversion = cmd.TomarSocioInversion;
                            queSeCambio += " TomarSocioInversion;";
                            seCambio++;
                        }
                        if (configuracion.PrimerMesInteresObligatorio != cmd.PrimerMesInteresObligatorio)
                        {
                            configuracion.PrimerMesInteresObligatorio = cmd.PrimerMesInteresObligatorio;
                            queSeCambio += " PrimerMesInteresObligatorio;";
                            seCambio++;
                        }
                        if (configuracion.CantidadPrestamosPorCliente != cmd.CantidadPrestamosPorCliente)
                        {
                            configuracion.CantidadPrestamosPorCliente = cmd.CantidadPrestamosPorCliente;
                            queSeCambio += " CantidadPrestamosPorCliente;";
                            seCambio++;
                        }
                        if (configuracion.InteresPIM != cmd.InteresPIM)
                        {
                            configuracion.InteresPIM = cmd.InteresPIM;
                            queSeCambio += " InteresPIM;";
                            seCambio++;
                        }
                        if (configuracion.InteresAnual != cmd.InteresAnual)
                        {
                            configuracion.InteresAnual = cmd.InteresAnual;
                            queSeCambio += " InteresAnual;";
                            seCambio++;
                        }
                        if (configuracion.InteresPorMora != cmd.InteresPorMora)
                        {
                            configuracion.InteresPorMora = cmd.InteresPorMora;
                            queSeCambio += " InteresPorMora;";
                            seCambio++;
                        }
                        if (configuracion.MontoMinimo != cmd.MontoMinimo)
                        {
                            configuracion.MontoMinimo = cmd.MontoMinimo;
                            queSeCambio += " MontoMinimo;";
                            seCambio++;
                        }
                        if (configuracion.MontoMaximo != cmd.MontoMaximo)
                        {
                            configuracion.MontoMaximo = cmd.MontoMaximo;
                            queSeCambio += " MontoMaximo;";
                            seCambio++;
                        }
                        if (configuracion.PlazoMesesMinimo != cmd.PlazoMesesMinimo)
                        {
                            configuracion.PlazoMesesMinimo = cmd.PlazoMesesMinimo;
                            queSeCambio += " PlazoMesesMinimo;";
                            seCambio++;
                        }
                        if (configuracion.PlazoMesesMaximo != cmd.PlazoMesesMaximo)
                        {
                            configuracion.PlazoMesesMaximo = cmd.PlazoMesesMaximo;
                            queSeCambio += " PlazoMesesMaximo;";
                            seCambio++;
                        }
                        if (configuracion.DiasDeGracias != cmd.DiasDeGracias)
                        {
                            configuracion.DiasDeGracias = cmd.DiasDeGracias;
                            queSeCambio += " DiasDeGracias;";
                            seCambio++;
                        }
                        if (configuracion.MonedaPorDefecto != cmd.MonedaPorDefecto)
                        {
                            configuracion.MonedaPorDefecto = cmd.MonedaPorDefecto;
                            configuracion.Moneda_Descripcion = MonedaDescripcion.GetMonedaTexto((int)cmd.MonedaPorDefecto);
                            queSeCambio += " MonedaPorDefecto;";
                            seCambio++;
                        }
                        if (configuracion.Observacion != cmd.Observacion)
                        {
                            configuracion.Observacion = cmd.Observacion;
                            queSeCambio += " Observacion;";
                            seCambio++;
                        }

                        if (seCambio > 0)
                        {
                            var usuarioId = Guid.Empty;
                            configuracion.ModifiedBy = usuarioId;
                            configuracion.ModifiedDate = DateTime.Now;

                            // Se veria algo asi:   10/09/2025 09:15: Se editó: TomarSocioInversion; (User: 9a8b7c6d-1234-5678-90ab-112233445566) 
                            configuracion.HistoricoCambios += $"{DateTime.Now}: {queSeCambio} (User: {usuarioId}){Environment.NewLine}";

                            await _context.SaveChangesAsync();
                            return AppResult.New(true, queSeCambio);
                        }
                        else
                        {
                            return AppResult.New(false, "No hay cambios que hacer.");
                        }
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, $"ERROR: {ex.Message}");
                    }
                }
            }
        }





        public class IndexConfiguracionPrestamo
        {
            public class QueryIndexConfiguracionPrestamo : IRequest<List<ConfiguracionPrestamoVm>>
            {

            }

            public class QueryIndexConfiguracionPrestamoHandler : IRequestHandler<QueryIndexConfiguracionPrestamo, List<ConfiguracionPrestamoVm>>
            {
                private readonly CooperativaDbContext _context;

                public QueryIndexConfiguracionPrestamoHandler(CooperativaDbContext context)
                {
                    _context = context;
                }

                public async Task<List<ConfiguracionPrestamoVm>> Handle(QueryIndexConfiguracionPrestamo query, CancellationToken cancellationToken)
                {

                    var configuraciones = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted && x.Enabled)
                        .AsNoTracking()
                        .ProjectToType<ConfiguracionPrestamoVm>()
                        .ToListAsync();

                    return configuraciones;

                }
            }
        }









    }
}

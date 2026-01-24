using Cooperativa.App.Domain.Data.Config;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Configuraciones;
using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model.EntidadesUtiles;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model.Socios;
using Cooperativa.App.Migrations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data
{
    public class CooperativaDbContext : DbContext
    {

        public CooperativaDbContext(DbContextOptions<CooperativaDbContext> options) : base(options)
        {
        }

        public DbSet<Secuencial> Secuencial { get; set; }
        public DbSet<Model.People.Persona> Persona { get; set; }
        public DbSet<Caja> Caja { get; set; }
        public DbSet<Egreso> Egreso { get; set; }
        public DbSet<Ingreso>  Ingreso { get; set; }
        public DbSet<Transaccion> Transaccion { get; set; }
        public DbSet<Prestamo> Prestamo { get; set; }
        public DbSet<PrestamoDetalle> PrestamoDetalle { get; set; }
        public DbSet<Socio> Socio { get; set; }
        public DbSet<SocioInversion> SocioInversion { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<DetalleSocioInversion> DetalleSocioInversion { get; set; }
        public DbSet<MovimientoDetalleSocio> MovimientoDetalleSocio { get; set; }
        public DbSet<ConfiguracionPrestamo> ConfiguracionPrestamo { get; set; }
        public DbSet<Retiro> Retiro { get; set; }
        public DbSet<SocioInversionRetiro> SocioInversionRetiro { get; set; }
        public DbSet<HistorialCambioMoneda> HistorialCambioMoneda { get; set; } //Este ya no utilizo, la estructure mal
        public DbSet<TasasCambioDivisa> TasasCambioDivisa { get; set; }
        public DbSet<InstitucionBancaria> InstitucionBancaria { get; set; }
        public DbSet<CuentaBancaria> CuentaBancaria { get; set; }
        public DbSet<GananciaDetalleSocio> GananciaDetalleSocio { get; set; }
        public DbSet<Nota> Nota { get; set; }
        public DbSet<Devolucion> Devolucion { get; set; }
        public DbSet<RetornoCliente> RetornoCliente { get; set; }
        public DbSet<MovimientoEntreCuenta> MovimientoEntreCuenta { get; set; }

        //Falta una tabla llamada GananciaSocio

        //Migracion
        //add-migration nameMigration -context CooperativaDbContext	
        //update-database -context CooperativaDbContext


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Prestamo>()
                .HasMany(p => p.PrestamoDetalles)
                .WithOne(d => d.Prestamo)
                .HasForeignKey(d => d.PrestamoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ingreso>()
                .HasOne(i => i.PrestamoDetalle)
                .WithMany()
                .HasForeignKey(i => i.PrestamoDetalleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ingreso>()
                .HasOne(i => i.SocioInversion)
                .WithMany()
                .HasForeignKey(i => i.SocioInversionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleSocioInversion>()
                .HasOne(di => di.SocioInversion)
                .WithMany(si => si.DetallesSocioInversion)
                .HasForeignKey(di => di.SocioInversionId)
                .OnDelete(DeleteBehavior.Restrict);


            #region Configuraciones
            modelBuilder.ApplyConfiguration(new RetiroConfig());
            modelBuilder.ApplyConfiguration(new SocioInversionRetiroConfig());
            modelBuilder.ApplyConfiguration(new PrestamoConfig());
            modelBuilder.ApplyConfiguration(new PersonaConfig());
            modelBuilder.ApplyConfiguration(new CuentaBancariaConfig());
            modelBuilder.ApplyConfiguration(new SocioInversionConfig());
            modelBuilder.ApplyConfiguration(new DetalleSocioInversionConfig());
            modelBuilder.ApplyConfiguration(new CajaConfig());
            modelBuilder.ApplyConfiguration(new IngresoConfig());
            modelBuilder.ApplyConfiguration(new EgresoConfig());
            modelBuilder.ApplyConfiguration(new TransaccionConfig());
            modelBuilder.ApplyConfiguration(new PrestamoDetalleConfig());
            modelBuilder.ApplyConfiguration(new SocioConfig());
            modelBuilder.ApplyConfiguration(new MovimientoDetalleSocioConfig());
            modelBuilder.ApplyConfiguration(new GananciaDetalleSocioConfig());
            modelBuilder.ApplyConfiguration(new ConfiguracionPrestamoConfig());
            modelBuilder.ApplyConfiguration(new NotaConfig());
            modelBuilder.ApplyConfiguration(new DevolucionConfig());
            modelBuilder.ApplyConfiguration(new RetornoClienteConfig());
            modelBuilder.ApplyConfiguration(new MovimientoEntreCuentaConfig());
            modelBuilder.ApplyConfiguration(new TasasCambioDivisaConfig());
            #endregion

        }





        public async Task<int> SaveShanges()
        {
            return await base.SaveChangesAsync();
        }



    }
}

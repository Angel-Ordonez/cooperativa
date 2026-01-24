using Cooperativa.App.Domain.Model.Caja;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.People;

namespace Cooperativa.App.Domain.Data.Config
{

    public class CuentaBancariaConfig : IEntityTypeConfiguration<CuentaBancaria>
    {
        public void Configure(EntityTypeBuilder<CuentaBancaria> modelBuilder)
        {

            // Propiedades
            modelBuilder.Property(c => c.NumeroCuenta).IsRequired().HasMaxLength(50);

            modelBuilder.Property(c => c.TipoCuenta).IsRequired();
            modelBuilder.Property(c => c.FechaUltimoMovimiento).IsRequired();

            modelBuilder.Property(c => c.SaldoAnterior).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.SaldoTotalTransferido).HasColumnType("decimal(9,2)").IsRequired();

            modelBuilder.Property(c => c.UltimoMovimiento).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.SaldoActual).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.TotalEntrante).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.TotalSaliente).HasColumnType("decimal(9,2)");



            // Relaciones
            // Relación N:1 con Institución Bancaria (Una institución puede tener muchas cuentas)
            modelBuilder.HasOne(c => c.InstitucionBancaria)
                .WithMany()  // Si no necesitas navegación inversa, osea no es necesario que la entidad Institucion
                .HasForeignKey(c => c.InstitucionBancariaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación 1:N con Persona (Una persona puede tener varias cuentas)
            modelBuilder.HasOne(c => c.Persona)
                .WithMany(p => p.CuentaBancarias)  // Aquí debo asegurarte de que Persona tenga la propiedad ICollection<CuentaBancaria>
                .HasForeignKey(c => c.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación 1:N con Caja (Una Caja puede tener varias cuentas)
            modelBuilder.HasOne(c => c.Caja)
                .WithMany(p => p.CuentaBancarias)  // Aquí debo asegurarte de que Caja tenga la propiedad ICollection<CuentaBancaria>
                .HasForeignKey(c => c.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación N:1 (CuentaBancariaOrigen → Transacciones)
            modelBuilder.HasMany(p => p.TransaccionesOrigen)
                .WithOne(c => c.CuentaBancariaOrigen)
                .HasForeignKey(c => c.CuentaBancariaOrigenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación N:1 (CuentaBancariaDestino → Transacciones)
            modelBuilder.HasMany(p => p.TransaccionesDestino)
                .WithOne(c => c.CuentaBancariaDestino)
                .HasForeignKey(c => c.CuentaBancariaDestinoId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}

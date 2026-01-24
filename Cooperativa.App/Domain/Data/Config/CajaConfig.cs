using Cooperativa.App.Domain.Model.Prestamos;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.Caja;

namespace Cooperativa.App.Domain.Data.Config
{
    public class CajaConfig : IEntityTypeConfiguration<Caja>
    {
        public void Configure(EntityTypeBuilder<Caja> modelBuilder)
        {

            // Relación 1:N (Caja → CuentaBancaria)
            modelBuilder.HasMany(p => p.CuentaBancarias)
                .WithOne(c => c.Caja)
                .HasForeignKey(c => c.CajaId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrado en cascada

            modelBuilder.HasMany(p => p.Transacciones)
                .WithOne(c => c.Caja)
                .HasForeignKey(c => c.CajaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.HasMany(p => p.MovimientoEntreCuentas)
                .WithOne(c => c.Caja)
                .HasForeignKey(c => c.CajaId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Property(c => c.SaldoAnterior).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.SaldoActual).HasColumnType("decimal(9,2)");




        }
    }
}

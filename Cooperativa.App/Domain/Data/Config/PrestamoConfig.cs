using Cooperativa.App.Domain.Model.Prestamos;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Cooperativa.App.Domain.Data.Config
{
    public class PrestamoConfig : IEntityTypeConfiguration<Prestamo>
    {
        public void Configure(EntityTypeBuilder<Prestamo> modelBuilder)
        {
            // Relación N:1 (Prestamo → CuentaBancaria)
            modelBuilder.HasOne(p => p.CuentaBancaria)
                .WithMany() // No es bidireccional (CuentaBancaria no necesita una lista de Préstamos)
                .HasForeignKey(p => p.CuentaBancariaId)
                .OnDelete(DeleteBehavior.Restrict); // Evita que se eliminen préstamos al borrar una cuenta




            modelBuilder.Property(c => c.CantidadInicial).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.InteresPorcentaje).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.CantidadEnDolar).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CantidadEnEuro).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CuotaMensual).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.EstimadoAPagarMes).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.MontoPagado).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.RestaCapital).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Mora).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Ganancia).HasColumnType("decimal(9,3)");







        }
    }
}

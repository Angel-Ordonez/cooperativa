using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Configuraciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{
    public class ConfiguracionPrestamoConfig : IEntityTypeConfiguration<ConfiguracionPrestamo>
    {


        public void Configure(EntityTypeBuilder<ConfiguracionPrestamo> modelBuilder)
        {

            modelBuilder.Property(c => c.InteresPIM).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.InteresAnual).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.InteresPorMora).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.MontoMinimo).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.MontoMaximo).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.PlazoMesesMinimo).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.PlazoMesesMaximo).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.MonedaPorDefecto).IsRequired();

        }










    }
}

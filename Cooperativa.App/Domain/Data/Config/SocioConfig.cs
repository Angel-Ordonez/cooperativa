using Cooperativa.App.Domain.Model.Caja;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.Socios;

namespace Cooperativa.App.Domain.Data.Config
{
    public class SocioConfig : IEntityTypeConfiguration<Socio>
    {
        public void Configure(EntityTypeBuilder<Socio> modelBuilder)
        {
            modelBuilder.Property(c => c.TotalMontoInvertido).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.PorcentajeGanancia).HasColumnType("decimal(9,2)");

            modelBuilder.Property(c => c.GananciaTotal).HasColumnType("decimal(9,3)");
            modelBuilder.Property(c => c.TotalRetirado).HasColumnType("decimal(9,3)");
            modelBuilder.Property(c => c.SaldoDisponibleARetirar).HasColumnType("decimal(9,3)");
        }
    }
}

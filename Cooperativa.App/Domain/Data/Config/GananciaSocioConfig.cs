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

    public class GananciaDetalleSocioConfig : IEntityTypeConfiguration<GananciaDetalleSocio>
    {
        public void Configure(EntityTypeBuilder<GananciaDetalleSocio> modelBuilder)
        {
            modelBuilder.Property(c => c.DeCantidad).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Ganancia).HasColumnType("decimal(9,3)");
            
            modelBuilder.Property(c => c.CantidadRetirada).HasColumnType("decimal(9,3)");
            
            modelBuilder.Property(c => c.CantidadDisponibleARetirar).HasColumnType("decimal(9,3)");



            // relaciones (Foreign Keys)
            modelBuilder
                .HasOne(c => c.DetalleSocioInversion)
                .WithMany()
                .HasForeignKey(c => c.DetalleSocioInversionId)
                .OnDelete(DeleteBehavior.Restrict);



        }
    }
}

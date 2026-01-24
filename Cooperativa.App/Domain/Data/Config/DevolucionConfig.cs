using Cooperativa.App.Domain.Model.Entidad;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{

    public class DevolucionConfig : IEntityTypeConfiguration<Devolucion>
    {
        public void Configure(EntityTypeBuilder<Devolucion> modelBuilder)
        {
            modelBuilder.HasOne(d => d.Retiro)
               .WithOne(r => r.Devolucion)
               .HasForeignKey<Devolucion>(d => d.RetiroId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Property(d => d.Cantidad).HasColumnType("decimal(8,2)");

            modelBuilder.Property(d => d.CantidadAprobada).HasColumnType("decimal(8,2)");

        }
    }
}

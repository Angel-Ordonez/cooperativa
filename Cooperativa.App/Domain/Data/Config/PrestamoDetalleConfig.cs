using Cooperativa.App.Domain.Model.Caja;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.Prestamos;

namespace Cooperativa.App.Domain.Data.Config
{
    public class PrestamoDetalleConfig : IEntityTypeConfiguration<PrestamoDetalle>
    {
        public void Configure(EntityTypeBuilder<PrestamoDetalle> modelBuilder)
        {
            modelBuilder.Property(c => c.MontoCapital).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.MontoInteres).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.TotalAPagar).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.RestaCapital).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.ProximoPago).HasColumnType("decimal(9,2)");


        }
    }
}

using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{
    public class DetalleSocioInversionConfig : IEntityTypeConfiguration<DetalleSocioInversion>
    {
        public void Configure(EntityTypeBuilder<DetalleSocioInversion> modelBuilder)
        {

            modelBuilder.Property(c => c.Habia).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.SePresto).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Quedan).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.PorcentajeEnPrestamo).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CantidadPagadaDePrestamo).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Ganancia).HasColumnType("decimal(9,3)");




        }

    }
}




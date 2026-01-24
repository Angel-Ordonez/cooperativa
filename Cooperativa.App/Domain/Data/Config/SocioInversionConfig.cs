using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{
    public class SocioInversionConfig : IEntityTypeConfiguration<SocioInversion>
    {
        public void Configure(EntityTypeBuilder<SocioInversion> modelBuilder)
        {

            modelBuilder.Property(c => c.Cantidad).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CantidadActiva).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CantidadEnDolar).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CantidadEnEuro).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.CantidadPrestada).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.PorcetajePrestado).HasColumnType("decimal(9,3)");
            
            modelBuilder.Property(c => c.NoPrestado).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Retirado).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.Ganancia).HasColumnType("decimal(9,3)");

            modelBuilder.Property(c => c.GananciaDisponible).HasColumnType("decimal(9,3)");
            modelBuilder.Property(c => c.CantidadDisponibleRetirar).HasColumnType("decimal(9,3)");


        }

    }
}

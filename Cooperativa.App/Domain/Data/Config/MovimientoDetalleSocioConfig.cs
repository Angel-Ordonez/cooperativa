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
    public class MovimientoDetalleSocioConfig : IEntityTypeConfiguration<MovimientoDetalleSocio>
    {
        public void Configure(EntityTypeBuilder<MovimientoDetalleSocio> modelBuilder)
        {
            modelBuilder.Property(c => c.Cantidad).HasColumnType("decimal(9,3)");


        }
    }
}

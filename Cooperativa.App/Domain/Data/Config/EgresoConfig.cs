using Cooperativa.App.Domain.Model.Caja;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{
    public class EgresoConfig : IEntityTypeConfiguration<Egreso>
    {
        public void Configure(EntityTypeBuilder<Egreso> modelBuilder)
        {
            modelBuilder.Property(c => c.Monto).HasColumnType("decimal(9,2)");


        }
    }
}
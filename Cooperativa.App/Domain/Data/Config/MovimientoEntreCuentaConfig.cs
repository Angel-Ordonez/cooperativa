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

    public class MovimientoEntreCuentaConfig : IEntityTypeConfiguration<MovimientoEntreCuenta>
    {
        public void Configure(EntityTypeBuilder<MovimientoEntreCuenta> modelBuilder)
        {
            modelBuilder.Property(c => c.Monto).HasColumnType("decimal(9,2)");


        }
    }


}

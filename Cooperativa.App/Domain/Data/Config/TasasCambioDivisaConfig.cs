using Cooperativa.App.Domain.Model.Caja;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.EntidadesUtiles;

namespace Cooperativa.App.Domain.Data.Config
{

    public class TasasCambioDivisaConfig : IEntityTypeConfiguration<TasasCambioDivisa>
    {
        public void Configure(EntityTypeBuilder<TasasCambioDivisa> modelBuilder)
        {

            modelBuilder.Property(c => c.Diferencia).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.TasaAnterior).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.Tasa).HasColumnType("decimal(9,2)");
        }
    }

}

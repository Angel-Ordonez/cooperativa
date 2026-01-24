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
    public class TransaccionConfig : IEntityTypeConfiguration<Transaccion>
    {
        public void Configure(EntityTypeBuilder<Transaccion> modelBuilder)
        {

            modelBuilder.Property(c => c.Monto).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.SaldoCajaEnElMomento).HasColumnType("decimal(9,2)");
            modelBuilder.Property(c => c.SaldoQuedaEnCaja).HasColumnType("decimal(9,2)");
        }
    }
}

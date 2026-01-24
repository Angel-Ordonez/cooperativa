using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.People;

namespace Cooperativa.App.Domain.Data.Config
{

    public class ClienteConfig : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> modelBuilder)
        {
            // Relación 1:N (Cliente → RetornoCliente)
            modelBuilder.HasMany(p => p.Retornos)
                .WithOne(c => c.Cliente)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrado en cascada
        }
    }


}

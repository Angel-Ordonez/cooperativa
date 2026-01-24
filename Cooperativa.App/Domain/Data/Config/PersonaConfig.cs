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
    public class PersonaConfig : IEntityTypeConfiguration<Persona>
    {
        public void Configure(EntityTypeBuilder<Persona> modelBuilder)
        {

            // Relación 1:N (Persona → CuentaBancaria)
            modelBuilder.HasMany(p => p.CuentaBancarias)
                .WithOne(c => c.Persona)
                .HasForeignKey(c => c.PersonaId)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrado en cascada



        }
    }
}

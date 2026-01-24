using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{

    public class NotaConfig : IEntityTypeConfiguration<Nota>
    {

        public void Configure(EntityTypeBuilder<Nota> modelBuilder)
        {

            // Relación con Persona (muchas notas por persona)
            modelBuilder.HasOne(n => n.Persona)
                   .WithMany(p => p.Notas)
                   .HasForeignKey(n => n.PersonaId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relación con Prestamo (muchas notas por préstamo)
            modelBuilder.HasOne(n => n.Prestamo)
                   .WithMany(p => p.Notas)
                   .HasForeignKey(n => n.PrestamoId)
                   .OnDelete(DeleteBehavior.Restrict);



        }
    }
}

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
    public class SocioInversionRetiroConfig : IEntityTypeConfiguration<SocioInversionRetiro>
    {
        public void Configure(EntityTypeBuilder<SocioInversionRetiro> modelBuilder)
        {
            // Configuración de la clave primaria compuesta
            modelBuilder.HasKey(rs => new { rs.SocioInversionId, rs.RetiroId });

            // Relación muchos a muchos con la entidad Socio
            modelBuilder.HasOne(rs => rs.SocioInversion)
                .WithMany(s => s.SocioInversionRetiros)
                .HasForeignKey(rs => rs.SocioInversionId)
                .OnDelete(DeleteBehavior.Restrict); // Especifica el comportamiento al eliminar un Socio (opcional)

            // Relación muchos a muchos con la entidad Retiro
            modelBuilder.HasOne(rs => rs.Retiro)
                .WithMany(r => r.SocioInversionRetiros)
                .HasForeignKey(rs => rs.RetiroId)
                .OnDelete(DeleteBehavior.Restrict); // Especifica el comportamiento al eliminar un Retiro (opcional)




            modelBuilder.Property(c => c.Cantidad).HasColumnType("decimal(9,3)");




        }
    }
}

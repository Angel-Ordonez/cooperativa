using Cooperativa.App.Domain.Model.Caja;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model.Entidad;

namespace Cooperativa.App.Domain.Data.Config
{

    public class RetornoClienteConfig : IEntityTypeConfiguration<RetornoCliente>
    {
        public void Configure(EntityTypeBuilder<RetornoCliente> modelBuilder)
        {
            modelBuilder.HasOne(r => r.Cliente)             //Cada retorno pertenece a un cliente
               .WithMany(r => r.Retornos)                                  //Un cliente puede tener muchos retornos
               .HasForeignKey(r => r.ClienteId)
               .OnDelete(DeleteBehavior.Restrict);


            // Relación con Prestamo (muchos retorno por préstamo)
            modelBuilder.HasOne(n => n.Prestamo)
                   .WithMany(p => p.Retornos)
                   .HasForeignKey(n => n.PrestamoId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }


}

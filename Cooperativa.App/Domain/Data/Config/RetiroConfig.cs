using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Entidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Data.Config
{
    public class RetiroConfig : IEntityTypeConfiguration<Retiro>
    {
        public void Configure(EntityTypeBuilder<Retiro> modelBuilder)
        {

            modelBuilder.HasKey(r => r.Id);  // `Id` es la clave primaria heredada de `EntityBasic`

            // Configuración de las propiedades de tipo `decimal(9,3)` para el monto
            modelBuilder.Property(r => r.Monto)
                .HasColumnType("decimal(9, 3)");

            modelBuilder.Property(r => r.CantidadEnEuro)
                .HasColumnType("decimal(9, 3)");

            modelBuilder.Property(r => r.CantidadEnDolar)
                .HasColumnType("decimal(9, 3)");

            // Configuración de relaciones
            // Relación con Caja
            modelBuilder.HasOne(r => r.Caja)
                .WithMany()  // No es necesario tener una propiedad de navegación inversa en `Caja`
                .HasForeignKey(r => r.CajaId)
                .OnDelete(DeleteBehavior.Restrict);  // Evitar que un retiro se elimine si tiene registros de caja

            // Relación con Egreso (opcional, ya que es nullable)
            modelBuilder.HasOne(r => r.Egreso)
                .WithMany()  // No es necesario tener una propiedad de navegación inversa en `Egreso`
                .HasForeignKey(r => r.EgresoId)
                .OnDelete(DeleteBehavior.SetNull);  // Si se elimina el `Retiro`, se pone a null el `EgresoId`

            // Relación con Socio (opcional, ya que es nullable)
            modelBuilder.HasOne(r => r.Solicitante)
                .WithMany()  // No es necesario tener una propiedad de navegación inversa en `Socio`
                .HasForeignKey(r => r.SolicitanteId)
                .OnDelete(DeleteBehavior.SetNull);  // Si se elimina el `Retiro`, se pone a null el `SocioId`

            // Relación con Responsable de Aprobación
            modelBuilder.HasOne(r => r.ResponsableAtendio)
                .WithMany()  // No es necesario tener una propiedad de navegación inversa en `Socio` para aprobación
                .HasForeignKey(r => r.ResponsableAtendioId)
                .OnDelete(DeleteBehavior.Restrict);  // Evitar eliminar un retiro si tiene un responsable de aprobación

            // Relación con HistorialCambioMoneda
            modelBuilder.HasOne(r => r.HistorialCambioMoneda)       //Estoy diciendo que cada Retiro puede estar asociado con un solo HistorialCambioMoneda
                .WithMany()  // No es necesario tener una propiedad de navegación inversa en `HistorialCambioMoneda`
                .HasForeignKey(r => r.HistorialCambioMonedaId)
                .OnDelete(DeleteBehavior.Restrict);  // Evitar eliminar el historial si el retiro está asociado

            // Relación con SocioInversionRetiro (tabla intermedia)
            modelBuilder.HasMany(r => r.SocioInversionRetiros)
                .WithOne()  // No es necesario tener propiedad de navegación inversa en `SocioInversionRetiro`
                .HasForeignKey(sir => sir.RetiroId)  // La propiedad `RetiroId` de la tabla intermedia
                .OnDelete(DeleteBehavior.Cascade);  // Si se elimina el retiro, se eliminan las relaciones con SocioInversionRetiro



            // Configurar otros campos opcionales como texto
            modelBuilder.Property(r => r.NumeroRetiro)
                .IsRequired()
                .HasMaxLength(20);  // Longitud máxima del número de retiro

            modelBuilder.Property(r => r.Moneda_Descripcion)
                .HasMaxLength(10);  // Longitud máxima de la descripción de moneda









        }
    }
    
}

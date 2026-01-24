using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Caja
{
    public class Ingreso : EntityBasic
    {
        public int NumeroIngreso { get; set; }
        public string Correlativo { get; set; }
        public decimal Monto { get; set; }
        public string Moneda_Descripcion { get; set; }
        public string Motivo { get; set; }
        public string Observacion { get; set; }


        #region foreigkey
        //Un Ingreso puede ser una Inversion de un Socio y cuando un cliente paga couta de prestamo
        public virtual SocioInversion SocioInversion { get; set; }
        public Guid? SocioInversionId { get; set; }
        public virtual PrestamoDetalle PrestamoDetalle { get; set; }
        public Guid? PrestamoDetalleId { get; set; }


        //Prestamo por si cancelan un Prestamo, entonces se devuelve el dinero
        public virtual Prestamo Prestamo { get; set; }
        public Guid? PrestamoId { get; set; }
        #endregion

        #region publicMethods
        public static Ingreso NewPorSocioInversion(int numeroIngreso, string correlativo, decimal monto, string motivo, string observacion, Guid socioInversionId,  Guid createdby)
        {
            var newIngreso = new Ingreso
            {
                NumeroIngreso = numeroIngreso,
                Correlativo = correlativo,
                Monto = monto,
                Moneda_Descripcion = "HNL",
                Motivo = motivo,
                Observacion = observacion,
                SocioInversionId = socioInversionId,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newIngreso;
        }


        public static Ingreso NewPorPrestamoDetalle(int numeroIngreso, string correlativo, decimal monto, string motivo, string observacion, Guid prestamoDetalle, Guid createdby)
        {
            var newIngreso = new Ingreso
            {
                NumeroIngreso = numeroIngreso,
                Correlativo = correlativo,
                Monto = monto,
                Motivo = motivo,
                Observacion = observacion,
                PrestamoDetalleId = prestamoDetalle,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newIngreso;
        }


        public static Ingreso NewPorPrestamoEliminado(int numeroIngreso, string correlativo, decimal monto, string motivo, string observacion, Guid prestamoId, Guid createdby)
        {
            var newIngreso = new Ingreso
            {
                NumeroIngreso = numeroIngreso,
                Correlativo = correlativo,
                Monto = monto,
                Motivo = motivo,
                Observacion = observacion,
                PrestamoId = prestamoId,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newIngreso;
        }



        #endregion








    }
}

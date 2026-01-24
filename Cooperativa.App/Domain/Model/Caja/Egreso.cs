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
    public class Egreso : EntityBasic
    {
        public int NumeroEgreso { get; set; }
        public string Correlativo { get; set; }
        public decimal Monto { get; set; }
        public string Moneda_Descripcion { get; set; }
        public string Motivo { get; set; }
        public string Observacion { get; set; }


        #region foreigkey
        public virtual Prestamo Prestamo { get; set; }
        public Guid? PrestamoId { get; set; }
        public virtual SocioInversion SocioInversion { get; set; }
        public Guid? SocioInversionId { get; set; }

        public virtual Retiro Retiro { get; set; }
        public Guid? RetiroId { get; set; }
        #endregion





        #region publicMethods
        public static Egreso New(int numeroEgreso, string correlativo, decimal monto, string motivo, string observacion, Guid prestamoId, Guid createdby)
        {
            var newIngreso = new Egreso
            {
                NumeroEgreso = numeroEgreso,
                Correlativo = correlativo,
                Monto = monto,
                Moneda_Descripcion = "HNL",
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


        public static Egreso NewPorInversionSocio(int numeroEgreso, string correlativo, decimal monto, string motivo, string observacion, Guid socioInversionId, Guid createdby)
        {
            var newIngreso = new Egreso
            {
                NumeroEgreso = numeroEgreso,
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



        public static Egreso NewPorRetiro(int numeroEgreso, string correlativo, decimal monto, string motivo, string observacion, Guid retiroId, Guid createdby)
        {
            var newIngreso = new Egreso
            {
                NumeroEgreso = numeroEgreso,
                Correlativo = correlativo,
                Monto = monto,
                Moneda_Descripcion = "HNL",
                Motivo = motivo,
                Observacion = observacion,
                RetiroId = retiroId,

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

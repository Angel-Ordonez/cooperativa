using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cooperativa.App.Domain.Model.People.InstitucionBancaria;

namespace Cooperativa.App.Domain.Model.Caja
{
    public class MovimientoEntreCuenta : EntityBasic
    {
        #region Propiedades
        public string Referencia { get; set; }
        public decimal Monto { get; set; }
        public string Motivo { get; set; }
        public string Observacion { get; set; }

        #endregion

        #region foreigkey
        public virtual Caja Caja { get; set; }
        public Guid CajaId { get; set; }
        public virtual CuentaBancaria CuentaOrigen { get; set; }
        public Guid CuentaBancariaOrigenId { get; set; }
        public virtual CuentaBancaria CuentaDestino { get; set; }
        public Guid CuentaBancariaDestinoId { get; set; }

        public virtual Transaccion Transaccion { get; set; }

        #endregion








        #region publicMethods

        public static MovimientoEntreCuenta New(string referencia, decimal monto, Guid cajaId, Guid cuentaBancariaOrigenId, Guid cuentaBancariaDestinoId, string motivo, Guid createdby)
        {
            var newRegistro = new MovimientoEntreCuenta
            {
                Referencia = referencia,
                Monto = monto,
                CuentaBancariaOrigenId = cuentaBancariaOrigenId,
                CuentaBancariaDestinoId = cuentaBancariaDestinoId,
                CajaId = cajaId,
                Motivo = motivo,
                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newRegistro;
        }





        #endregion







        #region Obj
        public class MovimientoEntreCuentaVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool Enabled { get; set; }
            public string Referencia { get; set; }
            public decimal Monto { get; set; }
            public string Motivo { get; set; }
            public string Observacion { get; set; }


            public Guid CajaId { get; set; }
            public Guid CuentaBancariaOrigenId { get; set; }
            public Guid CuentaBancariaDestinoId { get; set; }
        }


        #endregion





    }
}

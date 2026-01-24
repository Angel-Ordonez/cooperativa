using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Caja
{
    public class Caja : EntityBasic
    {
        public Guid ResponsableId { get; set; }     //Seria por identity u UsuarioId
        public string Referencia { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoActual { get; set; }
        public string Moneda_Descripcion { get; set; }
        public ModuloCaja Modulo { get; set; }
        public string Modulo_Descripcion { get; set; }

        #region foreigkey

        public Guid? EmpresaId { get; set; }

        //public Guid? CajaPadreId { get; set; }  //Esta vendria siendo la general, cuando una empresa ocupe mas modulos del sistema

        public virtual Transaccion UltimaTransaccion { get; set; }
        public Guid? UltimaTransaccionId { get; set; }




        //Estos ya no me sirven porque pertenecen a Transaccion
        public virtual Ingreso Ingreso { get; set; }
        public Guid? IngresoId { get; set; }
        public virtual Egreso Egreso { get; set; }
        public Guid? EgresoId { get; set; }
        #endregion


        #region nav
        public virtual ICollection<Transaccion> Transacciones { get; set; }
        public virtual ICollection<CuentaBancaria> CuentaBancarias { get; set; }
        public virtual ICollection<MovimientoEntreCuenta> MovimientoEntreCuentas { get; set; }
        public Caja()
        {
            CuentaBancarias = new HashSet<CuentaBancaria>();
            Transacciones = new HashSet<Transaccion>();
            MovimientoEntreCuentas = new HashSet<MovimientoEntreCuenta>();
        }

        #endregion



        #region publicMethods


        public static Caja New(Guid responsableId, Guid empresaId, string referencia, ModuloCaja modulo, Guid createdby)
        {
            var newCaja = new Caja
            {
                ResponsableId = responsableId,
                Referencia = referencia,
                EmpresaId = empresaId,
                Modulo = modulo,

                SaldoAnterior = 0,
                SaldoActual = 0,
                Moneda_Descripcion = "HNL",


                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newCaja.Modulo_Descripcion = ModuloCajaDescripcion.GetEstadoTexto((int)newCaja.Modulo);

            return newCaja;
        }

        public void ActualizarSaldo(decimal saldoAnterior, decimal nuevoSaldo, Guid ultimaTransaccionId, Guid usuario)
        {
            SaldoAnterior = saldoAnterior;
            SaldoActual = nuevoSaldo;
            UltimaTransaccionId = ultimaTransaccionId;
            ModifiedDate = DateTime.Now;
            ModifiedBy = usuario;
        }


        public static Caja NewEgreso(Guid responsableId, string referencia, decimal saldoAnterior, decimal saldoNuevo, Guid egresoId, Guid createdby)
        {
            var newCaja = new Caja
            {
                ResponsableId = responsableId,
                Referencia = referencia,
                SaldoAnterior = saldoAnterior,
                SaldoActual = saldoNuevo,
                Moneda_Descripcion = "HNL",
                EgresoId = egresoId,


                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newCaja;
        }


        public static Caja NewIngreso(Guid responsableId, string referencia, decimal saldoAnterior, decimal saldoNuevo, Guid ingresoId, Guid createdby)
        {
            var newCaja = new Caja
            {
                ResponsableId = responsableId,
                Referencia = referencia,
                SaldoAnterior = saldoAnterior,
                SaldoActual = saldoNuevo,
                Moneda_Descripcion = "HNL",
                IngresoId = ingresoId,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newCaja;
        }

        #endregion







    }
}

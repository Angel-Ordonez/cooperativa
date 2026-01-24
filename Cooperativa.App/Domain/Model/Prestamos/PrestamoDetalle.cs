using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Prestamos
{
    public class PrestamoDetalle : EntityBasic
    {

        public int NumeroCuota { get; set; }
        public decimal MontoCapital { get; set; }
        public decimal MontoInteres { get; set; }
        public decimal TotalAPagar { get; set; }
        public decimal RestaCapital { get; set; }   //Aqui seria guardar lo que aun debe del capital



        public Moneda Moneda { get; set; }
        public string Moneda_Descripcion { get; set; }

        public DateTime FechaPago { get; set; }
        public DateTime FechaProximoPago { get; set; }

        public decimal ProximoPago { get; set; }
        public string Observacion { get; set; }
        public string ReferenciaBancaria { get; set; }


        public int DiasDeGraciasAplicados { get; set; }

        #region foreigkey
        public virtual Prestamo Prestamo{ get; set; }
        public Guid PrestamoId { get; set; }
        public virtual Cliente Cliente { get; set; }
        public Guid ClienteId { get; set; }
        public virtual CuentaBancaria CuentaBancaria { get; set; }
        public Guid? CuentaBancariaId { get; set; }

        //Falta relcionar con la tabla con que pago el cliente la cuota (Habra una nueva tabla: donde una Persona puede tener muchas formas de pago... Efectivo, Bac, Ficohosa)
        //**AQUI SERIA LA CUENTA BANCARIA DE LA COOPERATIVA A LA QUE DEPOSITO EL CLIENTE...... si, la cooperativa debe tener relacionado cuentas bancarias tambien

        //Si el cliente pago via banco entonces se guardara el numero de transferencia
        //public string ReferenciaBancaria { get; set; }

        #endregion





        #region publicMethods
        public static PrestamoDetalle New(int numeroCouta, decimal montoCapital, decimal montoInteres, decimal totalAPagar, decimal restaCapital, DateTime fechaPago, string observacion, Guid prestamoId, Guid clienteId, Guid? cuentaBancariaId, Guid createdby)
        {
            var newPrestamoDetalle = new PrestamoDetalle
            {
                NumeroCuota = numeroCouta,
                MontoCapital = montoCapital,
                MontoInteres = montoInteres,
                TotalAPagar = totalAPagar,
                RestaCapital = restaCapital,
                FechaPago = fechaPago,
                Observacion = observacion,
                PrestamoId = prestamoId,
                ClienteId = clienteId,
                CuentaBancariaId = cuentaBancariaId,
                Moneda_Descripcion = "HNL",

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newPrestamoDetalle;
        }


        public static PrestamoDetalle New(int numeroCouta, decimal montoCapital, decimal montoInteres, decimal totalAPagar, decimal restaCapital, DateTime fechaPago, string observacion, Guid prestamoId, Guid clienteId, Guid? cuentaBancariaId, string referenciaBancaria, Guid createdby)
        {
            var newPrestamoDetalle = new PrestamoDetalle
            {
                NumeroCuota = numeroCouta,
                MontoCapital = montoCapital,
                MontoInteres = montoInteres,
                TotalAPagar = totalAPagar,
                RestaCapital = restaCapital,
                FechaPago = fechaPago,
                Observacion = observacion,
                PrestamoId = prestamoId,
                ClienteId = clienteId,
                CuentaBancariaId = cuentaBancariaId,
                ReferenciaBancaria = referenciaBancaria,
                Moneda_Descripcion = "HNL",

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newPrestamoDetalle;
        }

        #endregion















        #region Obj

        public class PrestamoDetalleVm
        {
            public Guid Id { get; set; }
            public int NumeroCuota { get; set; }
            public decimal MontoCapital { get; set; }
            public decimal MontoInteres { get; set; }
            public decimal TotalAPagar { get; set; }
            public int DiasDeGraciasAplicados { get; set; }
            public decimal RestaCapital { get; set; }
            public DateTime FechaPago { get; set; }
            public decimal ProximoPago { get; set; }
            public DateTime FechaProximoPago { get; set; }
            public string Observacion { get; set; }
            public Guid PrestamoId { get; set; }
            public Guid ClienteId { get; set; }
            public bool Enabled { get; set; }
            public bool IsSoftDeleted { get; set; }

            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
        }


        #endregion










    }
}

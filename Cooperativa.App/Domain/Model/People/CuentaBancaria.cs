using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Prestamos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Cooperativa.App.Domain.Model.People.InstitucionBancaria;

namespace Cooperativa.App.Domain.Model.People
{
    public class CuentaBancaria : EntityBasic
    {

        public string NumeroCuenta { get; set; }
        public string NumeroTarjeta { get; set; }           //Por ejmplo la tarjeta de debito
        public TipoCuentaBancaria TipoCuenta { get; set; }
        public string TipoCuenta_Descripcion { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal SaldoTotalTransferido { get; set; }
        public decimal UltimoMovimiento { get; set; }
        public int CantidadMovimientos { get;set; }
        public DateTime FechaUltimoMovimiento { get; set; }
        public string Observacion { get; set; }
        public decimal TotalEntrante { get; set; }
        public decimal TotalSaliente { get; set; }
        //No puse moneda, debo analizar si crear una nueva tabla


        //Para no crear una nueva tabla, pondre a que cuenta bancaria se realizo la transaccion en la tabla Transaccion
        //Seria bueno crear un componente especifico para mover dinero entre cuentas nada mas

        #region Foreigkey

        public virtual Persona Persona { get; set; }
        public Guid? PersonaId { get; set; }
        public virtual InstitucionBancaria InstitucionBancaria { get; set; }
        public Guid? InstitucionBancariaId { get; set; }    //Nulleable porque tambien hay que agregar la entrega en efectivo


        public virtual Cooperativa.App.Domain.Model.Caja.Caja Caja { get; set; }          //Una caja puede tener varias cuentas
        public Guid? CajaId { get; set; }
        #endregion




        #region nav
        public virtual ICollection<Transaccion> TransaccionesOrigen { get; set; }
        public virtual ICollection<Transaccion> TransaccionesDestino { get; set; }
        public CuentaBancaria()
        {
            TransaccionesOrigen = new HashSet<Transaccion>();
            TransaccionesDestino = new HashSet<Transaccion>();
        }
        #endregion

        #region Methods

        public static CuentaBancaria New(Guid personaId, Guid? institucionBancaria, string numeroCuenta, string numeroTarjeta, TipoCuentaBancaria tipoCuentaBancaria, Guid createdby)
        {
            var newEntidad = new CuentaBancaria
            {
                PersonaId = personaId,
                InstitucionBancariaId = institucionBancaria,
                NumeroCuenta = numeroCuenta,
                NumeroTarjeta = numeroTarjeta,
                TipoCuenta = tipoCuentaBancaria,
                SaldoAnterior = 0,
                //SaldoActual = 0,
                //CantidadMovimiento = 0,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            newEntidad.TipoCuenta_Descripcion = TipoCuentaBancarianDescripcion.GetEstadoTexto((int)newEntidad.TipoCuenta);

            return newEntidad;
        }


        public static CuentaBancaria NewParaCaja(Guid cajaId, Guid? institucionBancaria, string numeroCuenta, string numeroTarjeta, TipoCuentaBancaria tipoCuentaBancaria, Guid createdby)
        {
            var newEntidad = new CuentaBancaria
            {
                CajaId = cajaId,
                InstitucionBancariaId = institucionBancaria,
                NumeroCuenta = numeroCuenta,
                NumeroTarjeta = numeroTarjeta,
                TipoCuenta = tipoCuentaBancaria,
                SaldoAnterior = 0,
                //SaldoActual = 0,
                //CantidadMovimiento = 0,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            newEntidad.TipoCuenta_Descripcion = TipoCuentaBancarianDescripcion.GetEstadoTexto((int)newEntidad.TipoCuenta);

            return newEntidad;
        }










        public void SumarMovimiento(decimal cantidad)
        {
            var actual = SaldoActual;

            //SaldoAnterior = SaldoTotalTransferido;
            SaldoAnterior = actual;
            SaldoTotalTransferido += cantidad;
            SaldoActual += cantidad;                        //Basicamente este es ek mismo que SaldoTotalTransferido
            UltimoMovimiento = cantidad;
            TotalEntrante += cantidad;
            FechaUltimoMovimiento = DateTime.Now;
            ModifiedDate = DateTime.Now;
            CantidadMovimientos++;
        }
        public void RestarMovimiento(decimal cantidad)
        {
            var actual = SaldoActual;

            //SaldoAnterior = SaldoTotalTransferido;
            SaldoAnterior = actual;
            SaldoTotalTransferido -= cantidad;
            SaldoActual -= cantidad;
            UltimoMovimiento = -1 * cantidad;
            TotalSaliente += cantidad;
            FechaUltimoMovimiento = DateTime.Now;
            ModifiedDate = DateTime.Now;
            CantidadMovimientos++;
        }

        public void RestarMovimiento()
        {
            SaldoAnterior = SaldoTotalTransferido;

            var penultimoMovimiento = SaldoTotalTransferido - UltimoMovimiento;
            if(penultimoMovimiento == 0)
            {
                penultimoMovimiento = UltimoMovimiento;
            }

            SaldoTotalTransferido -= penultimoMovimiento;
            UltimoMovimiento = -1 * penultimoMovimiento;
            FechaUltimoMovimiento = DateTime.Now;
            ModifiedDate = DateTime.Now;
            CantidadMovimientos++;
        }


        #endregion









        #region Obj
        public class CuentaBancariaVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool Enabled { get; set; }
            public string NumeroCuenta { get; set; }
            public string NumeroTarjeta { get; set; }
            public TipoCuentaBancaria TipoCuenta { get; set; }
            public string TipoCuenta_Descripcion { get; set; }
            public decimal SaldoAnterior { get; set; }
            public decimal SaldoActual { get; set; }
            public decimal SaldoTotalTransferido { get; set; }
            public decimal UltimoMovimiento { get; set; }
            public int CantidadMovimientos { get; set; }
            public decimal TotalEntrante { get; set; }
            public decimal TotalSaliente { get; set; }
            public DateTime FechaUltimoMovimiento { get; set; }
            public Guid PersonaId { get; set; }
            public Guid? CajaId { get; set; }
            public Guid? InstitucionBancariaId { get; set; }
            public InstitucionBancariaVm InstitucionBancaria { get; set; }
            public string InstitucionBancariaNombre { get; set; }



            public string NombreCompletoCuenta
            {
                get
                {
                    var partes = new List<string>();

                    if (!string.IsNullOrWhiteSpace(InstitucionBancariaNombre))
                        partes.Add(InstitucionBancariaNombre);

                    if (!string.IsNullOrWhiteSpace(NumeroCuenta) &&
                        !partes.Contains(NumeroCuenta))
                        partes.Add(NumeroCuenta);

                    if (!string.IsNullOrWhiteSpace(NumeroTarjeta) &&
                        !partes.Contains(NumeroTarjeta))
                        partes.Add(NumeroTarjeta);

                    return string.Join(" - ", partes);
                }
            }

        }


        #endregion













    }
}

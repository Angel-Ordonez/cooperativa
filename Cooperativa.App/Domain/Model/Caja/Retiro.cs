using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Caja
{
    public class Retiro : EntityBasic
    {
        public string NumeroRetiro { get; set; }
        public decimal Monto { get; set; }
        public string Moneda_Descripcion { get; set; }
        public decimal CantidadEnEuro { get; set; }
        public decimal CantidadEnDolar { get; set; }
        public string Motivo { get; set; }
        public string Observacion { get; set; }
        public TipoRetiro TipoRetiro { get; set; }
        public string TipoRetiro_Descripcion { get; set; }
        public EstadoRetiro Estado { get; set; }
        public string Estado_Descripcion { get; set; }



        #region foreigkey
        public virtual Caja Caja { get; set; }
        public Guid CajaId { get; set; }
        public virtual Egreso Egreso { get; set; }      //Este solo se llenara cuando aprueben el retiro
        public Guid? EgresoId { get; set; }
        public virtual Socio Solicitante { get; set; }        //Puse Socio como Solicitante
        public Guid? SolicitanteId { get; set; }
        //public virtual SocioInversion SocioInversion { get; set; }  //DEBE SER UNA LISTA      //Si cancelo una inversion y llega el Socio arrepentido a las horas de haberlo dado...o si solo quiere sacar la ganancia de una de sus inversiones
        //public Guid? SocioInversionId { get; set; }
        public virtual Socio ResponsableAtendio { get; set; }
        public Guid? ResponsableAtendioId { get; set; }
        public virtual CuentaBancaria CuentaBancaria { get; set; }
        public Guid? CuentaBancariaId { get; set; }
        public virtual HistorialCambioMoneda HistorialCambioMoneda { get; set; }
        public Guid? HistorialCambioMonedaId { get; set; }

        //Pueda que al crear el prstamo pagaron ach
        public virtual Prestamo Prestamo { get; set; }
        public Guid? PrestamoId { get; set; }

        public virtual Devolucion Devolucion { get; set; }
        #endregion




        #region nav
        public virtual ICollection<SocioInversionRetiro> SocioInversionRetiros { get; set; }

        public Retiro()
        {
            SocioInversionRetiros = new HashSet<SocioInversionRetiro>();
        }

        #endregion






        #region publicMethods       Se debe crear un Egreso tambien
        public static Retiro New(string numeroRetiro, Guid cajaId, Guid? SolicitanteId, Guid? historialCambioMonedaId, Guid cuentaBancariaId, TipoRetiro tipoRetiro, decimal monto, string motivo, string observacion, string moneda_descripcion, Guid createdby)
        {
            var newRetiro = new Retiro
            {
                NumeroRetiro = numeroRetiro,
                CajaId = cajaId,
                SolicitanteId = SolicitanteId,
                HistorialCambioMonedaId = historialCambioMonedaId,
                Monto = monto,
                //Moneda_Descripcion = "HNL",
                Moneda_Descripcion = moneda_descripcion,
                Motivo = motivo,
                Observacion = observacion,
                Estado = EstadoRetiro.Pendiente,
                TipoRetiro = tipoRetiro,
                CuentaBancariaId = cuentaBancariaId,


                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newRetiro.Estado_Descripcion = EstadoRetiroDescripcion.GetEstadoTexto((int)newRetiro.Estado);
            newRetiro.TipoRetiro_Descripcion = TipoRetiroDescripcion.GetEstadoTexto((int)newRetiro.TipoRetiro);

            return newRetiro;
        }



        public void ActualizarEstado(EstadoRetiro estado, Guid usuario, string obs)
        {
            Estado = estado;
            Estado_Descripcion = EstadoRetiroDescripcion.GetEstadoTexto((int)estado);
            ModifiedDate = DateTime.Now;
            ModifiedBy = usuario;
            Observacion += obs;
        }

        #endregion








        #region Obj

        public class RetiroVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsSoftDeleted { get; set; }
            public bool Enabled { get; set; }
            public string NumeroRetiro { get; set; }
            public decimal Monto { get; set; }
            public string Moneda_Descripcion { get; set; }
            public decimal CantidadEnEuro { get; set; }
            public decimal CantidadEnDolar { get; set; }
            public string Motivo { get; set; }
            public string Observacion { get; set; }
            public TipoRetiro TipoRetiro { get; set; }
            public string TipoRetiro_Descripcion { get; set; }
            public EstadoRetiro Estado { get; set; }
            public string Estado_Descripcion { get; set; }
            public Guid CajaId { get; set; }
            public Guid? EgresoId { get; set; }
            public Guid? SolicitanteId { get; set; }
            public Guid? ResponsableAprobacionId { get; set; }
            public Guid? CuentaBancariaId { get; set; }
            public Guid? HistorialCambioMonedaId { get; set; }
        }
        #endregion













    }
}

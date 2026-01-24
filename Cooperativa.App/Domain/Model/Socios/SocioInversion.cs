using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Socios
{
    public class SocioInversion : EntityBasic
    {
        public string CodigoInversion { get; set; }
        public DateTime FechaIngreso { get; set; }

        public decimal Cantidad { get; set; }
        public decimal CantidadActiva { get; set; }  //Cantidad que se esta trabajando



        public decimal CantidadEnEuro { get; set; }
        public decimal CantidadEnDolar { get; set; }
        public decimal CantidadPrestada { get; set; }
        public decimal PorcetajePrestado { get; set; }
        public decimal NoPrestado { get; set; }
        public int Movimientos { get; set; }                        //Todos los movimientos que tuvo la inversion (Pueda que esa inevrsion se haya prestado o divido en varios prestamos).. solo cuando se preste

        public string Moneda_Descripcion { get; set; }
        public string Descripcion { get; set; }
        public EstadoInversion Estado { get; set; }
        public string Estado_Descripcion { get; set; }



        public decimal Ganancia { get; set; }
        public decimal GananciaDisponible { get; set; }
        public decimal CantidadDisponibleRetirar { get; set; }      //NoPrestado + GananciaDisponible
        public decimal Retirado { get; set; }                       //Para esto habra un Id de Retiro, pero este dato solo para un mejor control de cuanto a retirado de esta inversion





        #region foreigkey
        public virtual Socio Socio { get; set; }
        public Guid SocioId { get; set; }
        public string SocioNombre { get; set; }
        public virtual CuentaBancaria CuentaBancaria { get; set; }
        public Guid? CuentaBancariaId { get; set; }
        public virtual HistorialCambioMoneda HistorialCambioMoneda { get; set; }
        public Guid? HistorialCambioMonedaId { get; set; }
        #endregion





        #region publicMethods
        public static SocioInversion New(string codigoInversion, DateTime fechaIngreso, decimal cantidad, string moneda, string descipcion, Guid socioId, string socioNombre, Guid createdby)
        {
            var newInversion = new SocioInversion
            {
                CodigoInversion = codigoInversion,
                FechaIngreso = fechaIngreso,
                Cantidad = cantidad,
                CantidadActiva = cantidad,
                CantidadDisponibleRetirar = cantidad,
                Descripcion = descipcion,
                SocioId = socioId,
                SocioNombre = socioNombre,
                Estado = EstadoInversion.EnCaja,
                Moneda_Descripcion = moneda,
                NoPrestado = cantidad,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newInversion.Estado_Descripcion = EstadoSocioInversionDescripcion.GetEstadoTexto((int)newInversion.Estado);

            return newInversion;
        }
        #endregion



        #region nav
        public virtual ICollection<DetalleSocioInversion> DetallesSocioInversion { get; set; }
        public virtual ICollection<SocioInversionRetiro> SocioInversionRetiros { get; set; }

        public SocioInversion()
        {
            DetallesSocioInversion = new HashSet<DetalleSocioInversion>();
            SocioInversionRetiros = new HashSet<SocioInversionRetiro>();
        }

        #endregion




    }
}

using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model.People;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Prestamos
{
    public class Prestamo: EntityBasic
    {
        #region propiedades

        public int NumeroPrestamo { get; set; }
        public Guid Responsable { get; set; }
        public string CodigoPrestamo { get; set; }
        public TipoPrestamo TipoPrestamo { get; set; }
        public string TipoPrestamo_Descripcion { get; set; }
        public DateTime FechaEntragado { get; set; }
        public DateTime FechaEstimadoFinPrestamo { get; set; }
        public Moneda Moneda { get; set; }
        public string Moneda_Descripcion { get; set; }
        public EstadoPrestamo Estado { get; set; }
        public string Estado_Descripcion { get; set; }
        public DateTime FechaUltimoPago { get; set; }
        public string Garantia { get; set; }
        public string Observacion { get; set; }
        public bool? RequiereDocumento { get; set; }
        public string LinkDocumento { get; set; }

        public int CantidadMeses { get; set; }
        public int MesesPagados { get; set; }
        public int CuotasPagadas { get; set; }
        public int DebeMeses { get; set; }          //mediante un metodo que calcule cuantos meses no ha pagado el cliente


        public decimal CantidadInicial { get; set; }
        public decimal InteresPorcentaje { get; set; }
        public decimal CantidadEnEuro { get; set; }
        public decimal CantidadEnDolar { get; set; }
        public decimal CuotaMensual { get; set; }
        public decimal EstimadoAPagarMes { get; set; }  //Hacer un calculo rapido por: Cantidad - %Interes - cantidad de meses
        public decimal MontoPagado { get; set; }
        public decimal RestaCapital { get; set; }
        public decimal? Ganancia { get; set; }
        public decimal Mora { get; set; }

        public string ReferenciaBancaria { get; set; }
        #endregion


        #region foreigkey
        public virtual Cliente Cliente { get; set; }
        public Guid ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public virtual CuentaBancaria CuentaBancaria { get; set; }
        public Guid? CuentaBancariaId { get; set; }
        public virtual HistorialCambioMoneda HistorialCambioMoneda { get; set; }
        public Guid? HistorialCambioMonedaId { get; set; }


        //Falta el motivoId
        #endregion


        #region nav
        public virtual ICollection<PrestamoDetalle> PrestamoDetalles { get; set; }
        public virtual ICollection<Nota> Notas { get; set; }
        public virtual ICollection<RetornoCliente> Retornos { get; set; }
        #endregion


        #region publicMethods
        public static Prestamo NewAprobado(int numeroPrestamo, Guid responsableId, string codigoPrestamo, decimal cantidadInicial, decimal interesPorcentaje, int cantidadMeses,
            string garantia, DateTime fechaEntragado, string observacion,  Guid clienteId, string clienteNombre, int tipoPrestamo, decimal cuotaMnesual, Guid createdby)
        {
            var newPrestamo = new Prestamo
            {
                NumeroPrestamo = numeroPrestamo,
                Responsable = responsableId,
                CodigoPrestamo = codigoPrestamo,
                CantidadInicial = cantidadInicial,
                InteresPorcentaje = interesPorcentaje,
                CantidadMeses = cantidadMeses,
                Garantia = garantia,
                FechaEntragado = fechaEntragado,
                Observacion = observacion,
                ClienteId = clienteId,
                ClienteNombre = clienteNombre,
                MesesPagados = 0,
                CuotasPagadas = 0,
                DebeMeses = 0,
                Estado = EstadoPrestamo.Vigente,
                
                Moneda = Moneda.HNL,
                Moneda_Descripcion = "HNL",
                RestaCapital = cantidadInicial,

                TipoPrestamo = (TipoPrestamo)tipoPrestamo,
                TipoPrestamo_Descripcion = TipoPrestamonDescripcion.GetEstadoTexto(tipoPrestamo),
                CuotaMensual = cuotaMnesual,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newPrestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)newPrestamo.Estado);

            if (cantidadMeses > 0)
            {
                newPrestamo.FechaEstimadoFinPrestamo = fechaEntragado.AddMonths(cantidadMeses);
            }


            return newPrestamo;
        }




        public static Prestamo NewPendienteAprobacion(int numeroPrestamo, Guid responsableId, string codigoPrestamo, decimal cantidadInicial, decimal interesPorcentaje, int cantidadMeses,
            string garantia, string observacion, Guid clienteId, string clienteNombre, int tipoPrestamo, decimal cuotaMnesual, Guid createdby)
        {
            var newPrestamo = new Prestamo
            {
                NumeroPrestamo = numeroPrestamo,
                Responsable = responsableId,
                CodigoPrestamo = codigoPrestamo,
                CantidadInicial = cantidadInicial,
                InteresPorcentaje = interesPorcentaje,
                CantidadMeses = cantidadMeses,
                Garantia = garantia,
                //FechaEntragado = fechaEntragado,
                Observacion = observacion,
                ClienteId = clienteId,
                ClienteNombre = clienteNombre,
                MesesPagados = 0,
                CuotasPagadas = 0,
                DebeMeses = 0,
                Estado = EstadoPrestamo.PendienteAprobacion,

                Moneda = Moneda.HNL,
                Moneda_Descripcion = "HNL",
                RestaCapital = cantidadInicial,

                TipoPrestamo = (TipoPrestamo)tipoPrestamo,
                TipoPrestamo_Descripcion = TipoPrestamonDescripcion.GetEstadoTexto(tipoPrestamo),
                CuotaMensual = cuotaMnesual,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newPrestamo.Estado_Descripcion = EstadoPrestamoDescripcion.GetEstadoTexto((int)newPrestamo.Estado);


            return newPrestamo;
        }









        public Prestamo()
        {
            PrestamoDetalles = new HashSet<PrestamoDetalle>();
            Notas = new HashSet<Nota>();
            Retornos = new HashSet<RetornoCliente>();
        }


        #endregion

















        #region Obj


        #endregion








    }
}

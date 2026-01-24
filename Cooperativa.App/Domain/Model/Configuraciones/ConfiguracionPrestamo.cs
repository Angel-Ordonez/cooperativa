using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Configuraciones
{
    public class ConfiguracionPrestamo : EntityBasic
    {

        // por ahora solo abra 1, despues podriamos agregar el ClienteId para que existan mas configuraciones

        #region Properties

        public bool TomarSocioInversion { get; set; }               //Si hay dinero en caja, y quiero que tome el dinero que ha en caja antes de una version que no se ha trabajado
        public bool PrimerMesInteresObligatorio { get; set; }       //Si paga antes de mes debera pagar el mes completo de interes, si no, solo los dias generados

        public int CantidadPrestamosPorCliente { get; set; }
        public decimal InteresPIM { get; set; }                     //Prestamo Interes Mensual
        public decimal InteresAnual { get; set; }
        public decimal InteresPorMora { get; set; }
        public decimal MontoMinimo { get; set; }
        public decimal MontoMaximo { get; set; }
        public decimal PlazoMesesMinimo { get; set; }
        public decimal PlazoMesesMaximo { get; set; }
        public int DiasDeGracias { get; set; }
        public Moneda MonedaPorDefecto { get; set; }
        public string Moneda_Descripcion { get; set; }
        public string Observacion { get; set; }
        public string HistoricoCambios { get; set; }

        #endregion




        #region Metodos

        public static ConfiguracionPrestamo New(bool tomarSocioInversion, bool primerMesInteresObligatorio, Guid creadtedBy)
        {
            var newConf = new ConfiguracionPrestamo
            {
                TomarSocioInversion = tomarSocioInversion,
                PrimerMesInteresObligatorio = primerMesInteresObligatorio,

                CreatedBy = creadtedBy,
                CreatedDate = DateTime.Now,
                Enabled = true,
            };
            return newConf;
        }


        public static ConfiguracionPrestamo New( bool tomarSocioInversion, bool primerMesInteresObligatorio, int cantidadPrestamosPorCliente, decimal interesPIM, decimal interesAnual, decimal interesPorMora, 
                                    decimal montoMinimo, decimal montoMaximo, decimal plazoMesesMinimo, decimal plazoMesesMaximo, int diasDeGracias, Moneda monedaPorDefecto, string observacion, Guid createdBy)
        {
            var newConf = new ConfiguracionPrestamo
            {
                TomarSocioInversion = tomarSocioInversion,
                PrimerMesInteresObligatorio = primerMesInteresObligatorio,

                CantidadPrestamosPorCliente = cantidadPrestamosPorCliente,
                InteresPIM = interesPIM,
                InteresAnual = interesAnual,
                InteresPorMora = interesPorMora,
                MontoMinimo = montoMinimo,
                MontoMaximo = montoMaximo,
                PlazoMesesMinimo = plazoMesesMinimo,
                PlazoMesesMaximo = plazoMesesMaximo,
                DiasDeGracias = diasDeGracias,
                MonedaPorDefecto = monedaPorDefecto,

                Observacion = observacion,
                HistoricoCambios = string.Empty, // arranca vacío

                CreatedBy = createdBy,
                CreatedDate = DateTime.Now,
                Enabled = true,
                IsSoftDeleted = false
            };
            newConf.Moneda_Descripcion = MonedaDescripcion.GetMonedaTexto((int)newConf.MonedaPorDefecto);


            return newConf;
        }



        #endregion







        #region Obj
        public class ConfiguracionPrestamoVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsSoftDeleted { get; set; }
            public bool Enabled { get; set; }
            public bool TomarSocioInversion { get; set; }
            public bool PrimerMesInteresObligatorio { get; set; }
            public int CantidadPrestamosPorCliente { get; set; }
            public decimal InteresPIM { get; set; }
            public decimal InteresAnual { get; set; }
            public decimal InteresPorMora { get; set; }
            public decimal MontoMinimo { get; set; }
            public decimal MontoMaximo { get; set; }
            public decimal PlazoMesesMinimo { get; set; }
            public decimal PlazoMesesMaximo { get; set; }
            public int DiasDeGracias { get; set; }
            public Moneda MonedaPorDefecto { get; set; }
            public string Moneda_Descripcion { get; set; }
            public string Observacion { get; set; }
            public string HistoricoCambios { get; set; }

        }


        #endregion









    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Socios
{
    public class GananciaDetalleSocio :  EntityBasic
    {

        #region Propiedades
        public decimal DeCantidad { get; set; }     //De que Cantidad se saco el calculo
        public decimal Ganancia { get; set; }
        public bool Retirado { get; set; }
        public decimal CantidadRetirada { get; set; }           //Cuando retire no creare otro registro, solo actualizare
        public decimal CantidadDisponibleARetirar { get; set; }
        public string Observacion { get; set; }
        #endregion


        #region ForechKey
        public virtual DetalleSocioInversion DetalleSocioInversion { get; set; }                //Aqui tengo el porcentaje de lo que se presto y a que interes
        public Guid DetalleSocioInversionId { get; set; }
        public virtual GananciaDetalleSocio GananciaDetalleSocioAnterior { get; set; }
        public Guid? GananciaDetalleSocioAnteriorId { get; set; }
        #endregion


        #region Methods
        public static GananciaDetalleSocio New(Guid detalleSocioInversionId, decimal deCantidad, decimal ganancia, Guid usuarioId) 
        {
            var newGananciaSocio = new GananciaDetalleSocio
            {
                DetalleSocioInversionId = detalleSocioInversionId,
                DeCantidad = deCantidad,
                Ganancia = ganancia,
                Retirado = false,

                CreatedBy = usuarioId,
                Enabled = true,
                CreatedDate = DateTime.Now,
            };

            newGananciaSocio.CantidadDisponibleARetirar = newGananciaSocio.Ganancia - newGananciaSocio.CantidadRetirada;

            return newGananciaSocio;
        }

        #endregion





        #region Obj

        public class GananciaDetalleSocioVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsSoftDeleted { get; set; }
            public bool Enabled { get; set; }
            public decimal DeCantidad { get; set; }
            public decimal Ganancia { get; set; }
            public bool Retirado { get; set; }
            public decimal CantidadRetirada { get; set; }
            public decimal CantidadDisponibleARetirar { get; set; }
            public string Observacion { get; set; }
            public Guid DetalleSocioInversionId { get; set; }
            public Guid? GananciaDetalleSocioAnteriorId { get; set; }
        }


        #endregion






    }
}

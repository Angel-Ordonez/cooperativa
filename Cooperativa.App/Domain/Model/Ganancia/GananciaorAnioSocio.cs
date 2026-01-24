using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Socios;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Ganancia
{
    public class GananciaorAnioSocio: EntityBasic
    {

        //AUN NO HE HECHO LA MIGRACION NI LO METODO... SOLO ES YA LA IDEA COMENZADA

        public int Anio { set; get; }
        public int Cantidad_Inversiones { set; get; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal TotalInvertido { set; get; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Ganancia { set; get; }
        public DateTime FechaCalculado { set; get; }


        public virtual Socio Socio { get; set; }
        public Guid SocioId { get; set; }





        #region publicMethods
        public static GananciaorAnioSocio New(int anio, int cantindadInversiones, decimal totalInvertido, decimal gananciaAnio, DateTime fechaCalculado, Guid socioId, Guid createdby)
        {
            var newGananciaAnualSocio = new GananciaorAnioSocio
            {
                Anio = anio,
                Cantidad_Inversiones = cantindadInversiones,
                TotalInvertido = totalInvertido,
                Ganancia = gananciaAnio,
                FechaCalculado = fechaCalculado,
                SocioId = socioId,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newGananciaAnualSocio;
        }

        #endregion





    }
}

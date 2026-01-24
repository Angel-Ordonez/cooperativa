using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Socios
{
    public class MovimientoDetalleSocio : EntityBasic
    {

        #region properties

        public decimal Cantidad { get; set; }
        #endregion

        #region foreigkey
        public virtual DetalleSocioInversion DetalleSocioInversion { get; set; }
        public Guid DetalleSocioInversionId { get; set; }
        #endregion




        #region Methods

        public static MovimientoDetalleSocio New(Guid detalleSocioInversionId, decimal cantidad, Guid createdBy)
        {
            var newMovimiento = new MovimientoDetalleSocio
            {
                DetalleSocioInversionId = detalleSocioInversionId,
                Cantidad = cantidad,
                CreatedBy = createdBy,
                Enabled = true,
                CreatedDate = DateTime.Now,
            };

            return newMovimiento;
        }

        #endregion




    }
}

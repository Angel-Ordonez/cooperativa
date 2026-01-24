using Cooperativa.App.Domain.Model.Socios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Caja
{
    public class SocioInversionRetiro : EntityBasic
    {

        public SocioInversion SocioInversion { get; set; }
        public Guid SocioInversionId { get; set; }

        public Retiro Retiro { get; set; }
        public Guid RetiroId { get; set; }


        public decimal Cantidad { get; set; }



        public static SocioInversionRetiro New(Guid SocioInversionId, Guid retiroId, decimal cantidad, Guid createdby)
        {
            var newSocioRetiro = new SocioInversionRetiro
            {
                SocioInversionId = SocioInversionId,
                RetiroId = retiroId,
                Cantidad = cantidad,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newSocioRetiro;
        }












    }
}

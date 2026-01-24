using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.EntidadesUtiles
{
    public class Secuencial : EntityBasic
    {


        #region Propiedades

        public string Entidad { get; set; }
        public string CodigoBase { get; set;}
        public int UltimoSecuencial { get; set; }

        #endregion


        #region Methods

        public static Secuencial New(string entidad, string codigoBase, Guid createdby)
        {
            var newEntidad = new Secuencial
            {
                Entidad = entidad,
                CodigoBase = codigoBase,
                UltimoSecuencial = 1,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newEntidad;
        }



        #endregion




        #region Obj

        public class SecuencialVm
        {
            public Guid Id { get; set; }
            public string Entidad { get; set; }
            public string CodigoBase { get; set; }
            public int UltimoSecuencial { get; set; }
            public DateTime CreatedDate { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        #endregion






    }
}

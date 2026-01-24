using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model
{
    public class EntityBasic
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        [DefaultValue(false)]
        public bool IsSoftDeleted { get; set; }

        [DefaultValue(true)]
        public bool Enabled { get; set; }

        //COMO CREO MI PROPIO Nuget para solo poner Entity e identifique que ya estan dadas 4 propiedads en una entidad


        public void Eliminar()
        {
            IsSoftDeleted = true;
            ModifiedDate = DateTime.Now;
        }



    }
}

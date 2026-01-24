using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Entidad
{
    public abstract class Devolucion : EntityBasic
    {
        /*Que es abstrac: NO se puede instanciar
            Devolucion d = new Devolucion(); // ❌ NO permitido
            RetornoCliente rc = new RetornoCliente(); // ✅ OK
         */


        public decimal Cantidad { get; set; }
        public decimal CantidadAprobada { get; set; }


        public EstadoMovimientoPreAprobacion Estado { get; set; }
        public string Estado_Descripcion { get; set; }

        //Una Devocion podria ser: RetornoPersona (Cliente o Socio), Devolucion por productos, ect


        #region Foreigkey
        public virtual Retiro Retiro { get; set; }
        public Guid RetiroId { get; set; }
        #endregion


    }
}

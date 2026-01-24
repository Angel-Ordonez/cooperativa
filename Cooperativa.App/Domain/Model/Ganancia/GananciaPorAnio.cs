using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Ganancia
{

    //AUN NO HE HECHO LA MIGRACION NI LO METODO... SOLO ES YA LA IDEA COMENZADA
    public class GananciaPorAnio: EntityBasic
    {

        public int Anio { set; get; }
        public int Cantidad_Ingresos { set; get; }
        public int Cantidad_Egresos { set; get; }
        public int Cantidad_Transacciones { set; get; }
        public int Cantidad_Socios { set; get; }
        public int Cantidad_Inversiones { set; get; }
        public int Total_Inversiones { set; get; }
        public int Ganancia_Interes { set; get; }




    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum Meses
    {
        Enero = 1,
        Febrero = 2,
        Marzo = 3,
        Abril = 4,
        Mayo = 5,
        Junio = 6,
        Julio = 7,
        Agosto = 8,
        Septiembre = 9,
        Octubre = 10,
        Noviembre = 11,
        Diciembre = 12
    }



    public static class MesDescripcion
    {
        public static string GetMesTexto(int mesId)
        {
            Meses mes = (Meses)mesId;

            switch (mes)
            {
                case Meses.Enero:
                    return "Enero";
                case Meses.Febrero:
                    return "Febrero";
                case Meses.Marzo:
                    return "Marzo";
                case Meses.Abril:
                    return "Abril";
                case Meses.Mayo:
                    return "Mayo";
                case Meses.Junio:
                    return "Junio";
                case Meses.Julio:
                    return "Julio";
                case Meses.Agosto:
                    return "Agosto";
                case Meses.Septiembre:
                    return "Septiembre";
                case Meses.Octubre:
                    return "Octubre";
                case Meses.Noviembre:
                    return "Noviembre";
                case Meses.Diciembre:
                    return "Diciembre";
                default:
                    throw new ArgumentException("Id de mes no válido");
            }
        }
    }


}

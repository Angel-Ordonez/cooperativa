using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum PlanSuscripcion
    {
        Basico = 1,
        Premium = 2,
        Prueba = 3
    }


    public static class PlanSusbcripcionDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            PlanSuscripcion estado = (PlanSuscripcion)estadoId;

            switch (estado)
            {
                case PlanSuscripcion.Basico:
                    return "Basico";
                case PlanSuscripcion.Premium:
                    return "Premium";
                case PlanSuscripcion.Prueba:
                    return "Prueba";
                default:
                    throw new ArgumentException("ID de estado PlanSuscripcion no válido");
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{

    public enum EstadoMovimientoPreAprobacion
    {
        Pendiente = 1,
        Aprobado = 2,
        Rechazado = 3,
    }



    public static class EstadoMovimientoPreAprobacionDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            EstadoMovimientoPreAprobacion estado = (EstadoMovimientoPreAprobacion)estadoId;

            switch (estado)
            {
                case EstadoMovimientoPreAprobacion.Pendiente:
                    return "Pendiente";
                case EstadoMovimientoPreAprobacion.Aprobado:
                    return "Aprobado";
                case EstadoMovimientoPreAprobacion.Rechazado:
                    return "Rechazado";
                default:
                    throw new ArgumentException("Id de estado Retiro no válido");
            }
        }
    }


}

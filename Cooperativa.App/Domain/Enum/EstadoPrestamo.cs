using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum EstadoPrestamo
    {
        Vigente = 1,
        Pagado = 2,
        Cancelado = 3,
        PendienteAprobacion = 4,
        Rechazado = 5,
    }



    public static class EstadoPrestamoDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            EstadoPrestamo estado = (EstadoPrestamo)estadoId;

            switch (estado)
            {
                case EstadoPrestamo.Vigente:
                    return "Vigente";
                case EstadoPrestamo.Pagado:
                    return "Pagado";
                case EstadoPrestamo.Cancelado:
                    return "Cancelado";
                case EstadoPrestamo.PendienteAprobacion:
                    return "Pendiente Aprobacion";
                case EstadoPrestamo.Rechazado:
                    return "Rechazado";
                default:
                    throw new ArgumentException("ID de estado prestamo no válido");
            }
        }
    }


}

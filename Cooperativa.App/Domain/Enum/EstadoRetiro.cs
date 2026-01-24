using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{

    public enum EstadoRetiro
    {
        Pendiente = 1,
        Aprobado = 2,
        Rechazado = 3,
    }



    public static class EstadoRetiroDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            EstadoRetiro estado = (EstadoRetiro)estadoId;

            switch (estado)
            {
                case EstadoRetiro.Pendiente:
                    return "Pendiente";
                case EstadoRetiro.Aprobado:
                    return "Aprobado";
                case EstadoRetiro.Rechazado:
                    return "Rechazado";
                default:
                    throw new ArgumentException("Id de estado Retiro no válido");
            }
        }
    }







}

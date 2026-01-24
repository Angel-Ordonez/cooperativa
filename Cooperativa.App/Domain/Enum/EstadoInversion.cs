using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum EstadoInversion
    {
        EnCaja = 1,
        Agotado = 2,
        Retirado = 3
    }


    public static class EstadoSocioInversionDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            EstadoInversion estado = (EstadoInversion)estadoId;

            switch (estado)
            {
                case EstadoInversion.EnCaja:
                    return "En Caja";
                case EstadoInversion.Agotado:
                    return "Agotado";
                case EstadoInversion.Retirado:
                    return "Retirado";
                default:
                    throw new ArgumentException("ID de estado Inversion no válido");
            }
        }
    }








}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum EstadoBasico
    {
        Activo = 1,
        Inactivo = 2
    }

    public static class EstadoBasicoDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            EstadoBasico estado = (EstadoBasico)estadoId;

            switch (estado)
            {
                case EstadoBasico.Activo:
                    return "Activo";
                case EstadoBasico.Inactivo:
                    return "Inactivo";
                default:
                    throw new ArgumentException("Id de Estado no válido");
            }
        }
    }





}

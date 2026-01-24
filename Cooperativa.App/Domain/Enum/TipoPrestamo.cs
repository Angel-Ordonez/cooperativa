using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum TipoPrestamo
    {
        InteresMensual = 1,
        CuotaFija = 2,
    }


    public static class TipoPrestamonDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            TipoPrestamo estado = (TipoPrestamo)estadoId;

            switch (estado)
            {
                case TipoPrestamo.InteresMensual:
                    return "Interes Mensual";
                case TipoPrestamo.CuotaFija:
                    return "Cuota Fija";
                default:
                    throw new ArgumentException("ID de estado TipoPrestamo no válido");
            }
        }
    }
}

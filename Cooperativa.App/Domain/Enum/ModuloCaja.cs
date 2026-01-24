using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{

    public enum ModuloCaja
    {
        Prestamos = 1,
        Inactivo = 2
    }

    public static class ModuloCajaDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            ModuloCaja estado = (ModuloCaja)estadoId;

            switch (estado)
            {
                case ModuloCaja.Prestamos:
                    return "Modulo Prestamos";
                case ModuloCaja.Inactivo:
                    return "Otro";
                default:
                    throw new ArgumentException("Id de Estado no válido");
            }
        }
    }



}

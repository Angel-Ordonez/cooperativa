using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Utilidades
{
    public static class GuardExtensions
    {

        //Excepcion de objeto null; Ejemplo de uso: configuracionPrestamos.ThrowIfNull("No existe una configuración válida.");
        public static T ThrowIfNull<T>(this T obj, string mensaje = null)
        {
            if (obj == null)
                throw new InvalidOperationException(
                    mensaje ?? $"{typeof(T).Name} no puede ser nulo."
                );

            return obj;
        }
        



    }
}

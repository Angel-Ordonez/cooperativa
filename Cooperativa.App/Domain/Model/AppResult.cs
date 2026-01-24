using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model
{
    public class AppResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
        public byte[] Archivo { get; set; }
        public string NombreArchivo { get; set; }

        public static AppResult New(bool sucess, object result)
        {
            var n = new AppResult
            {
                Success = sucess,
                Result = result
            };
            return n;
        }

        public static AppResult New(bool sucess, string mensaje)
        {
            var n = new AppResult
            {
                Success = sucess,
                Message = mensaje
            };
            return n;
        }

        public static AppResult New(bool sucess, string mensaje, object result)
        {
            var n = new AppResult
            {
                Success = sucess,
                Message = mensaje,
                Result = result
            };
            return n;
        }


        //Para Pdf
        public static AppResult New(bool sucess, byte[] archivo, string nombreArchivo)
        {
            var n = new AppResult
            {
                Success = sucess,
                Archivo = archivo,
                NombreArchivo = nombreArchivo
            };
            return n;
        }




    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{

    public enum TipoInstitucionBancaria
    {
        Banco = 1,
        Cooperativa = 2,
        Otro = 3,
    }



    public static class TipoInstitucionBancariaDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            TipoInstitucionBancaria estado = (TipoInstitucionBancaria)estadoId;

            switch (estado)
            {
                case TipoInstitucionBancaria.Banco:
                    return "Banco";
                case TipoInstitucionBancaria.Cooperativa:
                    return "Cooperativa";
                case TipoInstitucionBancaria.Otro:
                    return "Otro";
                default:
                    throw new ArgumentException("Id de estado Retiro no válido");
            }
        }
    }





}

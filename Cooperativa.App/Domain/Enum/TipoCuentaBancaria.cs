using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{


    public enum TipoCuentaBancaria
    {
        Ahorros = 1,
        Planilla = 2,
        Corriente = 3,
        Otro = 4,
    }


    public static class TipoCuentaBancarianDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            TipoCuentaBancaria estado = (TipoCuentaBancaria)estadoId;

            switch (estado)
            {
                case TipoCuentaBancaria.Ahorros:
                    return "Ahorros";
                case TipoCuentaBancaria.Planilla:
                    return "Planilla";
                case TipoCuentaBancaria.Corriente:
                    return "Corriente";
                case TipoCuentaBancaria.Otro:
                    return "Otro";
                default:
                    throw new ArgumentException("Id de TipoCuenta no válido");
            }
        }
    }






}

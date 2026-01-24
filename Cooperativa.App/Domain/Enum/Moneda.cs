using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum Moneda
    {
        HNL = 1,
        USD= 2,
        EUR = 3,
    }


    public static class MonedaDescripcion
    {
        public static string GetMonedaTexto(int moendaId)
        {
            Moneda estado = (Moneda)moendaId;

            switch (estado)
            {
                case Moneda.HNL:
                    return "HNL";
                case Moneda.USD:
                    return "USD";
                case Moneda.EUR:
                    return "EUR";
                default:
                    throw new ArgumentException("ID de Moneda no válido");
            }
        }
    }



}

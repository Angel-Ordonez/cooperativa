using Cooperativa.App.Domain.Model.Caja;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.EntidadesUtiles
{
    public class TasasCambioDivisa : EntityBasic
    {


        #region Propiedades

        public DateTime Fecha { get; set; }
        public string MonedaBase { get; set; }
        public string MonedaDestino { get; set; }
        public decimal Tasa { get; set; }
        public string Fuente { get; set; }
        public string Observacion { get; set; }

        public decimal TasaAnterior { get; set; }
        public decimal Diferencia { get; set; }
        #endregion



        #region Metodos

        public static TasasCambioDivisa New(DateTime fecha, string monedaBase, string monedaDestino, decimal tasa, string fuente, string obs)
        {
            var newMomentoMoneda = new TasasCambioDivisa
            {
                Fecha = fecha,
                MonedaBase = monedaBase,
                MonedaDestino = monedaDestino,
                Tasa = tasa,
                Fuente = fuente,
                Observacion = obs,

                CreatedBy = Guid.Empty,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newMomentoMoneda;
        }


        //Monedas que quiero guardar
        public static readonly string[] Divisas = new string[]
        {
            "USD",
            "EUR",
            "HNL",
            "GTQ",
            "MXN",
            "NIO",
            "CRC"
        };
        //readonly significa que la variable solo se puede asignar una vez (al declararla o en el constructor) y no se puede cambiar la referencia después.

        //public static readonly string[] Divisas = { "USD", "EUR" };

        //// Esto NO se puede hacer:
        //Divisas = new string[] { "HNL" }; // ❌ Error

        //// Esto SÍ se puede hacer:
        //Divisas[0] = "HNL"; // ✅ La referencia sigue igual, solo cambia el contenido

        #endregion









        #region Obj

        public class TasasCambioDivisaVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsSoftDeleted { get; set; }
            public bool Enabled { get; set; }
            public DateTime Fecha { get; set; }
            public string MonedaBase { get; set; }
            public string MonedaDestino { get; set; }
            public decimal Tasa { get; set; }
            public string Fuente { get; set; }
            public string Observacion { get; set; }
            public decimal TasaAnterior { get; set; }
            public decimal Diferencia { get; set; }
        }


        #endregion







    }
}

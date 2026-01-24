using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Caja
{
    public class HistorialCambioMoneda : EntityBasic
    {
        public string MonedaBase { get; set; }      //Es por api que consumo, toma de arranque EUR

        [Column(TypeName = "decimal(9, 2)")]
        public decimal Euro { get; set; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Dolar { get; set; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Lempira { get; set; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal Quetzal { get; set; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal MXN { get; set; }
        [Column(TypeName = "decimal(9, 2)")]
        public decimal CordobaNicaragua { get; set; }    //Nicaragua
        [Column(TypeName = "decimal(9, 2)")]
        public decimal ColonCostaRica { get; set; }     //Costa Rica
        public string Observacion { get; set; }


        public static HistorialCambioMoneda New (string monedaBase, decimal euros, decimal dolar, decimal lempira, decimal quetzal, decimal mxn, decimal cordoba, decimal colon)
        {
            var newMomentoMoneda = new HistorialCambioMoneda
            {
                MonedaBase = monedaBase,
                Euro = euros,
                Dolar = dolar,
                Lempira = lempira,
                Quetzal = quetzal,
                MXN = mxn,
                CordobaNicaragua = cordoba,
                ColonCostaRica = colon,

                CreatedBy = Guid.Empty,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newMomentoMoneda;
        }







    }
}

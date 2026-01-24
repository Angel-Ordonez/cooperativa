using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Model.EntidadesUtiles;
using Cooperativa.App.Engine;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cooperativa.App.Utilidades
{
    public interface IUtilidadesBase
    {
        Task<int> GenerarSecuencial(string entidad, string codigobase);
        DescribirFechaVm DescribirFecha(DateTime fecha);
        DescribirCantidadVm DescribirCantidad(decimal cantidad);
    }

    public class UtilidadesBase : IUtilidadesBase
    {

        private readonly CooperativaDbContext _context;

        public UtilidadesBase(CooperativaDbContext bd)
        {
            _context = bd;
        }


        public async Task<int> GenerarSecuencial(string entidad, string codigobase)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entidad) || entidad.Contains(" "))
                    throw new ArgumentException("El parámetro 'entidad' no puede estar vacío ni contener espacios.");

                if (string.IsNullOrWhiteSpace(codigobase) || codigobase.Contains(" "))
                    throw new ArgumentException("El parámetro 'codigobase' no puede estar vacío ni contener espacios.");

                int numero = 0;

                var secuencialExiste = await _context.Secuencial.Where(x => x.Entidad.ToLower() == entidad.ToLower() && x.CodigoBase.ToLower() == codigobase.ToLower() && !x.IsSoftDeleted && x.Enabled).FirstOrDefaultAsync();
                if (secuencialExiste != null)
                {
                    numero = secuencialExiste.UltimoSecuencial + 1;

                    secuencialExiste.UltimoSecuencial += 1;
                    secuencialExiste.ModifiedDate = DateTime.Now;
                }
                else
                {
                    var newSecuencial = Secuencial.New(entidad, codigobase, Guid.Empty);
                    await _context.Secuencial.AddAsync(newSecuencial);

                    numero = newSecuencial.UltimoSecuencial;
                }

                await _context.SaveChangesAsync();

                return numero;

            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la tasa de cambio: {ex.Message}");
            }
        }





        public DescribirFechaVm DescribirFecha(DateTime fecha)
        {

            string diaEnLetras = NumeroDiaEnPalabras(fecha.Day);
            string mesEnLetras = NumeroMesEnPalabras(fecha.Month);
            string anioEnLetras = NumeroAnioEnPalabras(fecha.Year);

            var descripcion = $"{fecha.Day.ToString()} de {mesEnLetras} del año {fecha.Year.ToString()}";
            var descripcionCompleta = $"{diaEnLetras} días del mes de {mesEnLetras} del año {anioEnLetras}";

            var newRes = new DescribirFechaVm
            {
                Dia = diaEnLetras, 
                Mes = mesEnLetras, 
                Anio = anioEnLetras,
                Descripcion = descripcion,
                DescripcionCompleta = descripcionCompleta
            };
            return newRes;
        }

        public DescribirCantidadVm DescribirCantidad(decimal cantidad)
        {
            long parteEntera = (long)Math.Floor(cantidad);
            int parteDecimal = (int)Math.Round((cantidad - parteEntera) * 100);

            string enteroEnLetras = NumeroEnPalabras(parteEntera);
            //string decimalEnLetras = parteDecimal > 0 ? NumeroEnPalabras(parteDecimal) : "cero";
            var decimalEnLetras = "";
            string descripcion = "";
            if (parteDecimal > 0)
            {
                decimalEnLetras = NumeroEnPalabras(parteDecimal);
                descripcion = $"{enteroEnLetras} lempiras con {decimalEnLetras} centavos";
            }
            else
            {
                descripcion = $"{enteroEnLetras} lempiras exactos";
            }

            return new DescribirCantidadVm
            {
                EnteroEnLetras = enteroEnLetras,
                DecimalEnLetras = decimalEnLetras,
                Descripcion = descripcion
            };
        }


































































        private string NumeroDiaEnPalabras(int numero)
        {
            var unidades = new[]
            {
                "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete",
                "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince",
                "dieciséis", "diecisiete", "dieciocho", "diecinueve", "veinte",
                "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco",
                "veintiséis", "veintisiete", "veintiocho", "veintinueve", "treinta",
                "treinta y uno", "treinta y dos", "treinta y tres", "treinta y cuatro",
                "treinta y cinco", "treinta y seis", "treinta y siete", "treinta y ocho",
                "treinta y nueve", "cuarenta", "cuarenta y uno", "cuarenta y dos",
                "cuarenta y tres", "cuarenta y cuatro", "cuarenta y cinco",
                "cuarenta y seis", "cuarenta y siete", "cuarenta y ocho", "cuarenta y nueve",
                "cincuenta", "cincuenta y uno", "cincuenta y dos", "cincuenta y tres",
                "cincuenta y cuatro", "cincuenta y cinco", "cincuenta y seis",
                "cincuenta y siete", "cincuenta y ocho", "cincuenta y nueve", "sesenta",
                "setenta", "ochenta", "noventa", "cien"
            };

            return (numero >= 1 && numero < unidades.Length) ? unidades[numero] : numero.ToString();
        }

        private string NumeroMesEnPalabras(int numero)
        {
            var meses = new[]
            {
                "", "enero", "febrero", "marzo", "abril", "mayo", "junio",
                "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
            };

            if (numero >= 1 && numero <= 12)
                return meses[numero];

            return numero.ToString();
        }

        private string NumeroAnioEnPalabras(int anio)
        {
            // Soporte básico para años entre 1900 y 2099
            if (anio >= 2000 && anio <= 2099)
            {
                int resto = anio - 2000;

                if (resto == 0)
                    return "dos mil";
                else
                    return $"dos mil {NumeroDiaEnPalabras(resto)}";
            }
            else if (anio >= 1900 && anio <= 1999)
            {
                int resto = anio - 1900;
                if (resto == 0)
                    return "mil novecientos";
                else
                    return $"mil novecientos {NumeroDiaEnPalabras(resto)}";
            }

            return anio.ToString(); // Fallback por si el año no está en rango
        }

        private string NumeroEnPalabras(long numero)
        {
            var unidades = new[]
            {
                "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete",
                "ocho", "nueve", "diez", "once", "doce", "trece", "catorce", "quince",
                "dieciséis", "diecisiete", "dieciocho", "diecinueve"
            };

            var decenas = new[]
            {
                "", "", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa"
            };

            var centenas = new[]
            {
                "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos",
                "seiscientos", "setecientos", "ochocientos", "novecientos"
            };

            string ConvertirMenor1000(int n)
            {
                if (n == 0) return "";
                if (n < 20) return unidades[n];
                if (n < 100)
                {
                    int d = n / 10;
                    int u = n % 10;
                    return u == 0 ? decenas[d] : $"{decenas[d]} y {unidades[u]}";
                }
                else
                {
                    int c = n / 100;
                    int resto = n % 100;
                    string palabraC = c == 1 && resto == 0 ? "cien" : centenas[c];
                    return resto == 0 ? palabraC : $"{palabraC} {ConvertirMenor1000(resto)}";
                }
            }

            if (numero == 0) return "cero";

            var partes = new List<string>();
            long divisor;

            // Miles de millones
            divisor = 1_000_000_000;
            if (numero / divisor > 0)
            {
                long n = numero / divisor;
                partes.Add($"{ConvertirMenor1000((int)n)} mil millones");
                numero %= divisor;
            }

            // Millones
            divisor = 1_000_000;
            if (numero / divisor > 0)
            {
                long n = numero / divisor;
                partes.Add($"{ConvertirMenor1000((int)n)} millón{(n > 1 ? "es" : "")}");
                numero %= divisor;
            }

            // Miles
            divisor = 1_000;
            if (numero / divisor > 0)
            {
                long n = numero / divisor;
                if (n == 1)
                    partes.Add("mil");
                else
                    partes.Add($"{ConvertirMenor1000((int)n)} mil");
                numero %= divisor;
            }

            // Centenas restantes
            if (numero > 0)
                partes.Add(ConvertirMenor1000((int)numero));

            return string.Join(" ", partes).Trim();
        }
















    }













    public class DescribirFechaVm
    {
        public string Dia { get; set; }
        public string Mes { get; set; }
        public string Anio { get; set; }
        public string Descripcion { get; set; }
        public string DescripcionCompleta { get; set; }
    }
    public class DescribirCantidadVm
    {
        public string EnteroEnLetras { get; set; }    // Solo la parte entera
        public string DecimalEnLetras { get; set; }   // Parte decimal (centavos)
        public string Descripcion { get; set; }       // Texto completo
    }


}

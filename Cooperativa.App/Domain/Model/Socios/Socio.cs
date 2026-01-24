using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Socios
{
    public class Socio : Persona
    {
        public DateTime FechaDeIngreso { get; set; }
        public EstadoPersona Estado { get; set; }
        public DateTime FechaUltimaInversion { get; set; }
        public int? CantidadInvensiones { get; set; }
        public decimal? TotalMontoInvertido { get; set; }
        public decimal PorcentajeGanancia { get; set; }

        public decimal GananciaTotal { get; set; }
        public decimal TotalRetirado { get; set; }
        public decimal SaldoDisponibleARetirar { get; set; }
        public int CantidadRetiros { get; set; }      //No he migrado esta

        public string Beneficiario_Identificacion { get; set; }
        public string Beneficiario_Nombre { get; set; }
        public string Parentesco { get; set; }


        #region nav
        public virtual ICollection<SocioInversion> SocioInversiones { get; set; }

        #endregion


        #region
        public Socio()
        {
            SocioInversiones= new HashSet<SocioInversion>();
        }
        #endregion


        #region publicMethods
        public static Socio New(string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento, string estadoCivil, string genero, string lugarTrabajo, string ocupacion, string Observacion,
            string pais, string ciudad, string direccion, string correo, string telefono, string telefono2, string otrocontacto, string rtn, string beneficiario_Identificacion, string parentesco, string beneficiario_nombre, DateTime fechadeIngreso, string codigoPersona, decimal porcentajeGanancia, Guid createdby)
        {
            var newSocio = new Socio
            {
                Nombre = nombre,
                Apellido = apellido,
                TipoIdentificacion = tipoIdentificacion,
                Identificacion = identificacion,
                Pais = pais,
                Ciudad = ciudad,
                Direccion = direccion,
                Correo = correo,
                Telefono = telefono,
                Telefono2 = telefono2,
                OtroContacto = otrocontacto,
                Observacion = Observacion,
                TieneUsuario = false,
                FechaNacimiento = fechaNacimiento,
                Edad = DateTime.Now.Year - fechaNacimiento.Year,
                EstadoCivil = estadoCivil,
                Genero = genero,
                LugarTrabajo = lugarTrabajo,
                Ocupacion = ocupacion,
                Estado = EstadoPersona.Activo,
                FechaDeIngreso = fechadeIngreso,
                CodigoPersona = codigoPersona,
                RTN = rtn,
                Beneficiario_Identificacion = beneficiario_Identificacion,
                Beneficiario_Nombre = beneficiario_nombre,
                Parentesco = parentesco,
                PorcentajeGanancia = porcentajeGanancia,

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newSocio;
        }
        #endregion





    }
}

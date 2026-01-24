using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model.Prestamos;

namespace Cooperativa.App.Domain.Model.People
{
    public class Persona : EntityBasic
    {


        #region propiedades

        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string LugarTrabajo { get; set; }
        public string Ocupacion { get; set; }
        public int Edad { get; set; }
        public string Genero { get; set; }
        public string EstadoCivil { get; set; }
        public string Pais { get; set; }
        public string Region { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Telefono2 { get; set; }
        public string OtroContacto { get; set; }
        public string Observacion { get; set; }
        public bool TieneUsuario { get; set; }
        public string CodigoPersona { get; set; }
        public string RTN { get; set; }
        #endregion



        #region nav

        public virtual ICollection<CuentaBancaria> CuentaBancarias { get; set; }
        public virtual ICollection<Nota> Notas { get; set; }
        public Persona()
        {
            CuentaBancarias = new HashSet<CuentaBancaria>();
            Notas = new HashSet<Nota>();
        }

        #endregion




    }

}

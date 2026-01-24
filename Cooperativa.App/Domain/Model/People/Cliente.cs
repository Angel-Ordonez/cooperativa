using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Entidad;
using Cooperativa.App.Domain.Model.Prestamos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.People
{
    public class Cliente : Persona
    {

        #region propiedades
        public bool PrestamoActivo { get; set; }
        public int? CantidadPrestamos { get; set; }
        public string RecomendadoPor { get; set; }
        public string Nota { get; set; }
        public bool ClienteBueno { get; set; }
        public EstadoPersona Estado { get; set; }
        #endregion


        #region nav
        public virtual ICollection<Prestamo> Prestamos { get; set; }
        public virtual ICollection<RetornoCliente> Retornos { get; set; }

        #endregion


        #region
        public Cliente()
        {
            Prestamos = new HashSet<Prestamo>();
            Retornos = new HashSet<RetornoCliente>();
        }
        #endregion






        #region publicMethods
        public static Cliente New(string nombre, string apellido, string tipoIdentificacion, string identificacion, DateTime fechaNacimiento, string estadoCivil, string genero, string lugarTrabajo, string ocupacion, string recomendadopor, string Observacion,
            string pais, string ciudad, string direccion, string correo, string telefono, string telefono2, string otrocontacto, string rtn, string codigoPersona, Guid createdby)
        {
            var cliente = new Cliente
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
                RTN = rtn,
                Observacion = Observacion,
                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                TieneUsuario = false,
                PrestamoActivo = false,
                ClienteBueno = false,
                FechaNacimiento = fechaNacimiento,
                Edad = DateTime.Now.Year - fechaNacimiento.Year,
                EstadoCivil = estadoCivil,
                Genero = genero,
                LugarTrabajo = lugarTrabajo,
                Ocupacion = ocupacion,
                RecomendadoPor = recomendadopor,
                Nota = null,
                Estado = EstadoPersona.Activo,
                CodigoPersona = codigoPersona,

                IsSoftDeleted = false,
                Enabled = true
            };



            return cliente;
        }
        #endregion






        #region Obj

        public class ClienteVm
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public DateTime FechaNacimiento { get; set; }
            public string Identidad { get; set; }
            public string LugarTrabajo { get; set; }
            public string Ocupacion { get; set; }
            public int Edad { get; set; }
            public string EstadoCivil { get; set; }
            public string Pais { get; set; }
            public string Ciudad { get; set; }
            public string Direccion { get; set; }
            public string Correo { get; set; }
            public string Telefono { get; set; }
            public string OtroContacto { get; set; }
            public string Observacion { get; set; }
            public string CodigoPersona { get; set; }
            public EstadoPersona Estado { get; set; }
            public string RecomendadoPor { get; set; }
            public string Nota { get; set; }
            public bool PrestamoActivo { get; set; }
            public int? CantidadPrestamos { get; set; }
            public bool ClienteBueno { get; set; }
            public DateTime CreatedDate { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

        #endregion










    }
}

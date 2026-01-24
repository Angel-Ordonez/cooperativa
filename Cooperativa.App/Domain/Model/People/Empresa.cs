using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.People
{
    public class Empresa : EntityBasic
    {

        #region propieades
        public string Nombre { get; set; }
        public string RTN { get; set; }
        public string Pais { get; set; }
        public string Region { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Movil { get; set; }
        public string Telefono { get; set; }
        public string Fax { get; set; }
        public string PaginaWeb { get; set; }
        public string ContactoPrincipal { get; set; }
        public string RUC { get; set; }                         //Registro Único de Contribuyentes): Identificador fiscal de la empresa.
        public DateTime FechaConstitucion { get; set; }
        public string NumeroLicencia { get; set; }
        public string CodigoPostal { get; set; }
        public string Sector { get; set; }
        public string DescripcionActividad { get; set; }
        public string Zona { get; set; }
        public string ReferenciaBancaria { get; set; }
        public string Observaciones { get; set; }



        public string Estado { get; set; }                          //Podria ser Activa, inactiva (pero para esto tengo enbaled)
        public string TipoEmpresa { get; set; }                     //Deberia ser un enum
        public string NombreRepresentanteLegal { get; set; }        //Este deberia ser un SocioId


        //Aqui iria la relacion de 1 a muchos.... 1 empresa a muchos socios
        //Aqui iria la relacion de 1 a muchos.... 1 empresa a muchos clientes.... cada empresa tiene sus clientes




        #endregion



    }
}

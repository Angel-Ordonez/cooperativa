using Cooperativa.App.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.People
{
    public class InstitucionBancaria : EntityBasic
    {

        public string Nombre { get; set; }
        public string Pais { get; set; }
        public string Direccion { get; set; }
        public string Direccion2 { get; set; }
        public string Telefono { get; set; }
        public string Telefono2 { get; set; }
        public string SitioWeb { get; set; }
        public TipoInstitucionBancaria TipoInstitucion { get; set; }
        public string TipoInstitucion_Descripcion { get; set; }


        //El objetivo de esta tabla, es tener direcciones donde ir a dejar el dinero al cliente, osea a que bancos... por ejemplo a bac del Mall Cascadas


        public static InstitucionBancaria New(string nombre, string pais, string direccion, string direccion2, string tel, string tel2, string sitioWeb, TipoInstitucionBancaria tipoInstitucionBancaria, Guid createdby)
        {
            var newEntidad = new InstitucionBancaria
            {
                Nombre = nombre,
                Pais = pais,
                Direccion = direccion,
                Direccion2 = direccion2,
                Telefono = tel,
                Telefono2 = tel2,
                SitioWeb = sitioWeb,
                TipoInstitucion = tipoInstitucionBancaria,


                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newEntidad.TipoInstitucion_Descripcion = TipoInstitucionBancariaDescripcion.GetEstadoTexto((int)newEntidad.TipoInstitucion);

            return newEntidad;
        }





        #region Obj
        public class InstitucionBancariaVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool Enabled { get; set; }
            public string Nombre { get; set; }
            public string Pais { get; set; }
            public string Direccion { get; set; }
            public string Direccion2 { get; set; }
            public string Telefono { get; set; }
            public string Telefono2 { get; set; }
            public string SitioWeb { get; set; }
            public TipoInstitucionBancaria TipoInstitucion { get; set; }
            public string TipoInstitucion_Descripcion { get; set; }

        }


        #endregion







    }
}

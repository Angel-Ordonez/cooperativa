using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Entidad
{
    public class Nota : EntityBasic
    {

        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public string Detalle { get; set; } //Aqui seria si actualizaron la nota


        public TipoNota Tipo { get; set; }
        public string Tipo_Descripcion { get; set; }
        public EstadoNota Estado { get; set; }
        public string Estado_Descripcion { get; set; }



        #region foreigkey
        public virtual Prestamo Prestamo { get; set; }
        public Guid? PrestamoId { get; set; }
        public virtual Persona Persona { get; set; }
        public Guid? PersonaId { get; set; }

        #endregion




        #region public Methods
        public static Nota New(Guid? prestamoId, Guid? personaId, string titulo, string contenido, TipoNota tipoNota, EstadoNota estadoNota, Guid createdby)
        {
            var newObj = new Nota
            {
                Titulo = titulo,
                Contenido = contenido,
                PrestamoId = prestamoId,
                PersonaId = personaId,
                Tipo = tipoNota,
                Estado = estadoNota,
                Tipo_Descripcion = TipoNotaDescripcion.GetEstadoTexto((int)tipoNota),
                Estado_Descripcion = EstadoNotaDescripcion.GetEstadoTexto((int)estadoNota),

                CreatedBy = createdby,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };

            return newObj;
        }
        #endregion









        #region Obj

        public class NotaVm
        {
            public Guid Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime ModifiedDate { get; set; }
            public Guid? PrestamoId { get; set; }
            public Guid? PersonaId { get; set; }
            public TipoNota Tipo { get; set; }
            public string Tipo_Descripcion { get; set; }
            public EstadoNota Estado { get; set; }
            public string Estado_Descripcion { get; set; }
            public string Titulo { get; set; }
            public string Contenido { get; set; }
            public string Detalle { get; set; }
        }






        #endregion












    }
}

using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model.Caja;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Model.Entidad
{
    public class RetornoCliente : Devolucion
    {

        #region Foreigkey

        public virtual Cliente Cliente { get; set; }
        public Guid ClienteId { get; set; }

        public virtual Prestamo Prestamo { get; set; }
        public Guid PrestamoId { get; set; }

        #endregion



        #region Methods


        public static RetornoCliente New(Guid clienteId, Guid prestamoId, decimal cantidad, Guid retiroId, Guid usuarioId)
        {
            var newRegistro = new RetornoCliente
            {
                RetiroId = retiroId,

                ClienteId = clienteId,
                PrestamoId = prestamoId,
                Cantidad = cantidad,
                CantidadAprobada = 0,
                Estado = EstadoMovimientoPreAprobacion.Pendiente,



                CreatedBy = usuarioId,
                CreatedDate = DateTime.Now,
                IsSoftDeleted = false,
                Enabled = true
            };
            newRegistro.Estado_Descripcion = EstadoMovimientoPreAprobacionDescripcion.GetEstadoTexto((int)newRegistro.Estado);

            return newRegistro;
        }

        #endregion






        #region Obj

        public class RetornoClienteVm
        {
            public Guid Id { get; set; }
            public Guid CreatedBy { get; set; }
            public Guid ModifiedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime ModifiedDate { get; set; }
            public bool IsSoftDeleted { get; set; }
            public bool Enabled { get; set; }
            public Guid ClienteId { get; set; }
            public Guid PrestamoId { get; set; }
            public decimal Cantidad { get; set; }
            public decimal CantidadAprobada { get; set; }
            public EstadoMovimientoPreAprobacion Estado { get; set; }
            public string Estado_Descripcion { get; set; }
            public Guid RetiroId { get; set; }
            public string ClienteNombre { get; set; }
            public string PrestamoNumeroPrestamo { get; set; }
            public string RetiroNumeroRetiro { get; set; }
        }
        #endregion





    }
}

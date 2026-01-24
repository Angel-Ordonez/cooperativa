using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum TipoRetiro
    {
        InversionGananciaSocio = 1,         //Validar Id de SocioInversion y cambiarle el estado
        //InversionSocio = 2,                 //Validar Id de SocioInversion y cambiarle el estado (Aqui seria solo a Inversion)
        Devolucion = 2,
        Ganancia = 3,                       //Validar Id de SocioInversion y cambiarle el estado... Aqui seria como restarle a la ganancia calcualda
        Emergencia = 4,
        Proyecto = 5,
        PagoProveedor = 6,
        PagoDeudas = 7,
        NecesidadesOperativas = 8,
        CompraInventario = 9,
        TransferenciaACH = 10,
        //MovimientoEntreCuentasCaja = 11
    }



    public static class TipoRetiroDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            TipoRetiro estado = (TipoRetiro)estadoId;

            switch (estado)
            {
                case TipoRetiro.InversionGananciaSocio:
                    return "Inversion y Ganancia de Socio";
                case TipoRetiro.Devolucion:
                    return "Devolucion";
                case TipoRetiro.Ganancia:
                    return "Ganancia de Inversion";
                case TipoRetiro.Emergencia:
                    return "Emergencia";
                case TipoRetiro.Proyecto:
                    return "Proyecto";
                case TipoRetiro.PagoProveedor:
                    return "Pago Proveedor";
                case TipoRetiro.PagoDeudas:
                    return "Pago Deudas";
                case TipoRetiro.NecesidadesOperativas:
                    return "Necesidades Operativas";
                case TipoRetiro.CompraInventario:
                    return "Compra Inventario";
                case TipoRetiro.TransferenciaACH:
                    return "Transferencia ACH";
                //case TipoRetiro.MovimientoEntreCuentasCaja:
                //    return "Movimiento Entre Cuentas de Caja";
                default:
                    throw new ArgumentException("Id de tipo Retiro no válido");
            }
        }
    }


}

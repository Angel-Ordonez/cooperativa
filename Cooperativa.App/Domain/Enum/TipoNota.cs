using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Domain.Enum
{
    public enum TipoNota
    {
        General = 1,
        Administrativo = 2,
        Seguimiento = 3,
        Cobranza = 4,
        Riesgo = 5,
        Informativa = 6,
        ServicioAlCliente = 7,
        Auditoria = 8,
        Alerta = 9,
    }

    public static class TipoNotaDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            TipoNota estado = (TipoNota)estadoId;

            switch (estado)
            {
                case TipoNota.General:
                    return "General";

                case TipoNota.Administrativo:
                    return "Administrativo";

                case TipoNota.Seguimiento:
                    return "Seguimiento";

                case TipoNota.Cobranza:
                    return "Cobranza";

                case TipoNota.Riesgo:
                    return "Riesgo";

                case TipoNota.Informativa:
                    return "Informativa";

                case TipoNota.ServicioAlCliente:
                    return "Servicio al Cliente";

                case TipoNota.Auditoria:
                    return "Auditoría";

                case TipoNota.Alerta:
                    return "Alerta";

                default:
                    throw new ArgumentException("Id de Tipo no válido");
            }
        }
    }







    public enum EstadoNota
    {
        Informativa = 1,
        Pendiente = 2,
        Resuelta = 3
    }

    public static class EstadoNotaDescripcion
    {
        public static string GetEstadoTexto(int estadoId)
        {
            EstadoNota estado = (EstadoNota)estadoId;

            switch (estado)
            {
                case EstadoNota.Informativa:
                    return "Informativa";                
                
                case EstadoNota.Pendiente:
                    return "Pendiente";

                case EstadoNota.Resuelta:
                    return "Resuelta";

                default:
                    throw new ArgumentException("Id de Tipo no válido");
            }
        }
    }







}

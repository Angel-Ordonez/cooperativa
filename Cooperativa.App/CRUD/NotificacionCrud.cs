using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Engine;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.SocioInversionCrud;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;

namespace Cooperativa.App.CRUD
{
    public class NotificacionCrud
    {

        public class EnviarCorreo
        {

            public class CommandCorreoGamil : IRequest<AppResult>
            {
                public string Titulo { get; set; }
                public string Body { get; set; }
                public List<string> correos { get; set; }
            }
            public class CommandHandlerSocioInversionBySocioAndAnio : IRequestHandler<CommandCorreoGamil, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly INotificacionesEngine _inotificacionesEngine;
                public CommandHandlerSocioInversionBySocioAndAnio(CooperativaDbContext context, INotificacionesEngine inotificacionesEngine)
                {
                    _context = context;
                    _inotificacionesEngine = inotificacionesEngine;
                }

                public async Task<AppResult> Handle(CommandCorreoGamil command, CancellationToken cancellationToken)
                {

                    var lista = await _context.Prestamo.ToListAsync();

                    try
                    {


                        var prestamoDetalle = await _context.PrestamoDetalle.Where(x => x.Id.ToString() == "606819C5-7F1E-42F7-AE3D-08DE555DE24B")
                            .Include(x => x.Cliente)
                            .Include(x => x.Prestamo)
                            .FirstOrDefaultAsync();

                        CuentaBancariaVm cuentaBancaria = null;

                        if(prestamoDetalle.CuentaBancariaId != null)
                        {
                            cuentaBancaria = await _context.CuentaBancaria.Where(x => x.Id == prestamoDetalle.CuentaBancariaId)
                                .ProjectToType<CuentaBancariaVm>()
                                .FirstOrDefaultAsync();
                        }

                        string body = "";

                        body += "<html>";
                        //body += "<body style='font-family: Arial, Helvetica, sans-serif; background-color:#ffffff; padding:0; margin:0;'>";
                        body += "<body style='font-family: Arial, Helvetica, sans-serif; background-color:#f4f6f8; padding:0; margin:0;'>";     //Este pone un fondo gris

                        /* ===== CONTENEDOR GENERAL ===== */
                        body += "<table width='100%' style='border-collapse:collapse;'>";

                        /* ===== LOGO SUPERIOR DERECHA ===== */
                        //body += "<tr>";
                        //body += "<td style='padding:10px 20px; text-align:right;'>";
                        //body += "<img src='https://www.freepik.es/fotos-vectores-gratis/logo-design' alt='Logo' style='height:40px;'>";
                        //body += "</td>";
                        //body += "</tr>";

                        /* ===== CONTENEDOR CENTRADO ===== */
                        body += "<tr>";
                        body += "<td align='center'>";

                        /* ===== COMPROBANTE ===== */
                        body += "<table width='600' style='background-color:#ffffff; border-collapse:collapse; border-radius:3px; overflow:hidden;'>";

                        /* ===== HEADER ===== */
                        body += "<tr>";
                        body += "<td style='background-color:#003a8f; padding:7px; color:#ffffff;'>";
                        body += "<table width='100%' style='border-collapse:collapse;'>";
                        body += "<tr>";
                        // Ícono a la izquierda
                        body += "<td width='30' style='text-align:left; font-size:18px; padding-left:15px; '> ✅ </td>";
                        // Texto centrado
                        body += "<td style='text-align:center; font-size:18px; font-weight:bold;'>COMPROBANTE DE PAGO</td>";
                        // Columna vacía para balancear el centrado
                        body += "<td width='30'>&nbsp;</td>";
                        body += "</tr>";
                        body += "</table>";
                        body += "</td>";
                        body += "</tr>";



                        /* ===== CONTENIDO ===== */
                        body += "<tr>";
                        body += "<td style='padding:25px; color:#333;'>";

                        /* Saludo */
                        body += $"<p style='font-size:16px; margin-top:0;'>Hola <strong>{prestamoDetalle.Cliente.Nombre.ToUpper()} {prestamoDetalle.Cliente.Apellido.ToUpper()}</strong>,</p>";

                        /* Mensaje principal */
                        body += "<table width='100%' style='border-collapse:collapse; margin:12px 0 0 0;'>";
                        body += "<tr>";
                        body += "<td style='font-size:13px; color:#333; font-weight:bold;'>";
                        body += "Se ha registrado pago por";
                        body += "</td>";
                        body += "<td style='font-size:18px; color:#003a8f; font-weight:bold; text-align:right;'>";
                        body += $"LPS {prestamoDetalle.TotalAPagar.ToString("N2")}";
                        body += "</td>";
                        body += "</tr>";
                        body += "</table>";

                        /* ===== LÍNEA SUAVE ===== */
                        body += "<table width='100%' style='border-collapse:collapse; margin:5px 0 20px ;'>";
                        body += "<tr>";
                        body += "<td style='border-bottom:1px solid #e0e0e0; height:1px; line-height:1px;'>&nbsp;</td>";
                        body += "</tr>";
                        body += "</table>";

                        //body += "<p style='font-size:13px; margin-bottom:20px;'>";
                        //body += "A continuación, le compartimos el detalle de su transacción:";
                        //body += "</p>";

                        /* ===== DETALLE DEL PAGO ===== */
                        body += "<table width='100%' style='border-collapse:collapse; font-size:13px;'>";

                        void AddRow(string label, string value)
                        {
                            body += "<tr>";
                            body += "<td style='padding:6px 4px; color:#555; width:45%;'>" + label + "</td>";
                            body += "<td style='padding:6px 4px; font-weight:bold; color:#000;'>" + value + "</td>";
                            body += "</tr>";
                        }


                        AddRow("Prestamo", prestamoDetalle.Prestamo.CodigoPrestamo);
                        AddRow("Cantidad Inicial", prestamoDetalle.Prestamo.CantidadInicial.ToString("N2"));
                        AddRow("Cantidad Actual", prestamoDetalle.Prestamo.RestaCapital.ToString("N2"));
                        AddRow("Fecha y hora de pago", prestamoDetalle.CreatedDate.ToString("dd-MM-yyyy hh:mm tt"));
                        AddRow("Numero de Cuota", prestamoDetalle.NumeroCuota.ToString());
                        //AddRow("Monto Recibido", $"<span style='color:#003a8f; font-size:15px;'> {prestamoDetalle.TotalAPagar.ToString("N2")} </span>" );
                        AddRow("Interes", prestamoDetalle.MontoInteres.ToString("N2"));
                        AddRow("Capital Aportado", prestamoDetalle.MontoCapital.ToString("N2"));
                        AddRow("Pago hasta el dia", prestamoDetalle.FechaPago.ToString("dd-MM-yyyy"));
                        if (cuentaBancaria != null)
                        {
                            AddRow("Cuenta Origen", cuentaBancaria.NombreCompletoCuenta);
                            AddRow("Referencia Bancaria", prestamoDetalle.ReferenciaBancaria);
                        }
                        AddRow("Fecha proximo pago", prestamoDetalle.FechaProximoPago.ToString("dd-MM-yyyy"));
                        AddRow("Proximo pago", prestamoDetalle.ProximoPago.ToString("N2"));



                        body += "</table>";

                        /* ===== FOOTER ===== */
                        body += "<p style='font-size:12px; color:#777; margin-top:45px;'>";
                        body += "Este correo electrónico es una notificación automática, por favor no responder.";
                        body += "</p>";

                        body += "<p style='font-size:13px; margin-bottom:0;'>";
                        body += "Saludos cordiales,<br>";
                        //body += "<strong>Departamento de Créditos y Cobros</strong>";
                        body += "</p>";

                        body += "</td>";
                        body += "</tr>";

                        /* ===== FIN COMPROBANTE ===== */
                        body += "</table>";

                        body += "</td>";
                        body += "</tr>";

                        /* ===== FIN CONTENEDOR GENERAL ===== */
                        body += "</table>";

                        body += "</body>";
                        body += "</html>";



                        var enviar = await _inotificacionesEngine.CrearCorreo(command.correos, body, command.Titulo);


                        return enviar;
                        //return AppResult.New(true, "Funciono");
                    }
                    catch (Exception ex) 
                    {
                        return AppResult.New(false, ex.Message);
                    }

                }
            }
        }





        public class EnviarCorreoPagoPIM
        {

            public class CommandCorreoGamil : IRequest<AppResult>
            {
                public Guid? PrestamoDetalleId { get; set; }
                public List<string> Correos { get; set; }
            }
            public class CommandHandlerSocioInversionBySocioAndAnio : IRequestHandler<CommandCorreoGamil, AppResult>
            {
                private readonly CooperativaDbContext _context;
                private readonly INotificacionesEngine _inotificacionesEngine;
                public CommandHandlerSocioInversionBySocioAndAnio(CooperativaDbContext context, INotificacionesEngine inotificacionesEngine)
                {
                    _context = context;
                    _inotificacionesEngine = inotificacionesEngine;
                }

                public async Task<AppResult> Handle(CommandCorreoGamil command, CancellationToken cancellationToken)
                {
                    try
                    {
                        var correos = new List<string>
                        {
                            "ordonezangel88@gmail.com"
                        };

                        if (command.Correos != null && command.Correos.Any())
                        {
                            correos.AddRange(command.Correos);
                        }

                        var prestamoDetalleId = new Guid("606819C5-7F1E-42F7-AE3D-08DE555DE24B");
                        if(command.PrestamoDetalleId != null && command.PrestamoDetalleId != Guid.Empty)
                        {
                            prestamoDetalleId = (Guid)command.PrestamoDetalleId;

                        }

                        var enviar = await _inotificacionesEngine.EnviarCorreoPagoPIM(correos, prestamoDetalleId);

                        return enviar;
                    }
                    catch (Exception ex)
                    {
                        return AppResult.New(false, ex.Message);
                    }
                }
            }
        }












    }
}

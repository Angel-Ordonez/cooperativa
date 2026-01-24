using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Cooperativa.App.Domain.Data;
using Cooperativa.App.Soluciones;
using Cooperativa.App.Utilidades;
using Cooperativa.App.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static Cooperativa.App.Domain.Model.People.CuentaBancaria;
using Mapster;
using static Cooperativa.App.CRUD.NotificacionCrud;

namespace Cooperativa.App.Engine
{

    public interface INotificacionesEngine
    {
        Task<AppResult> CrearCorreo(List<string> correo, string Body, string Subject);
        Task<AppResult> EnviarCorreoPagoPIM(List<string> correos, Guid prestamoDetalleId);
        Task<AppResult> EnviarCorreoPagoPIM( Guid prestamoDetalleId);
    }

    public class NotificacionesEngine : INotificacionesEngine
    {
        private readonly CooperativaDbContext _context;
        private readonly IQRServices _iQrServices;
        private readonly IUtilidadesBase iUtilidadesBase;
        private readonly ICalculationService _calculationService;

        public NotificacionesEngine(CooperativaDbContext context, IQRServices qrServices, IUtilidadesBase utilidadesBase, ICalculationService calculationService)
        {
            _context = context;
            _iQrServices = qrServices;
            iUtilidadesBase = utilidadesBase;
            _calculationService = calculationService;
        }



        public async Task<AppResult> CrearCorreoAngel(List<string> correo, string Body, string Subject)
        {
            try
            {
                MailMessage email = new MailMessage();
                correo.ForEach(s =>
                {
                    email.To.Add(s);
                });
                email.From = new MailAddress("angelvesta2022@gmail.com");
                email.Subject = Subject;
                AlternateView plainView = AlternateView.CreateAlternateViewFromString(Body, null, "text/html");
                string html = Body;
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, null, "text/html");

                email.AlternateViews.Add(plainView);
                email.AlternateViews.Add(htmlView);
                email.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("angelvesta2022@gmail.com", "mrey cffg iucx gwya");
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Send(email);

                return AppResult.New(true, "Correo enviado exitosamente");
            }
            catch(Exception ex)
            {
                return AppResult.New(false, ex.Message);
            }
        }
        
              
        public static void CrearCorreo2(List<string> correo, string Body, string Subject)
        {
            ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
            StringBuilder bodytmp = new StringBuilder();
            MailMessage mailmsg = new MailMessage();
            mailmsg.From = new MailAddress("angelvesta2022@gmail.com", "Project Vesta");
            //mailmsg.To.Add(to);
            correo.ForEach(s =>
            {
                mailmsg.To.Add(s);
            });

            mailmsg.Subject = Subject;
            //mailmsg.Attachments.Add(new Attachment(@"J:\Files1\TrackingTerrestre\HND\ValesImpresosUltimaMilla\" + fileName + extencion));

            bodytmp.Append(Body);

            AlternateView avHTML = AlternateView.CreateAlternateViewFromString(bodytmp.ToString(), null, MediaTypeNames.Text.Html);

            mailmsg.AlternateViews.Add(avHTML);

            mailmsg.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential("angelvesta2022@gmail.com", "20161002109");
            smtpClient.Send(mailmsg);
        }
       
          
        public static string ConvertirStringBase64(string base64EncodedData)
        {
            byte[] byt = System.Text.Encoding.UTF8.GetBytes(base64EncodedData);
            var strModified = Convert.ToBase64String(byt);
            return strModified;
        }



        public async Task<AppResult> CrearCorreo(List<string> correo, string Body, string Subject)
        {
            try
            {
                MailMessage email = new MailMessage();
                correo.ForEach(s =>
                {
                    email.To.Add(s);
                });
                email.From = new MailAddress("infoqts00@gmail.com");
                email.Subject = Subject;
                AlternateView plainView = AlternateView.CreateAlternateViewFromString(Body, null, "text/html");
                string html = Body;
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(html, null, "text/html");

                email.AlternateViews.Add(plainView);
                email.AlternateViews.Add(htmlView);
                email.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("infoqts00@gmail.com", "eiez magg dpnv yfmb");
                smtp.UseDefaultCredentials = false;
                smtp.EnableSsl = true;
                smtp.Send(email);

                return AppResult.New(true, "Correo enviado exitosamente");
            }
            catch (Exception ex)
            {
                return AppResult.New(false, ex.Message);
            }
        }




        public List<string> ObtenerCorreosValidos(List<string> correos)
        {
            if (correos == null || !correos.Any())
                return new List<string>();

            return correos
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .Where(c =>
                {
                    try
                    {
                        var mail = new MailAddress(c);
                        return mail.Address == c;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }







        public async Task<AppResult> EnviarCorreoPagoPIM(List<string> correos, Guid prestamoDetalleId)
        {
            try
            {
                var prestamoDetalle = await _context.PrestamoDetalle.Where(x => x.Id == prestamoDetalleId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.Prestamo)
                    .FirstOrDefaultAsync();

                if (prestamoDetalle != null)
                {

                    CuentaBancariaVm cuentaBancaria = null;

                    if (prestamoDetalle.CuentaBancariaId != null)
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
                    body += "Se le informa que se ha registrado pago por";
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
                    
                    



                    correos =correos.Distinct().ToList();
                    var correosValidos = ObtenerCorreosValidos(correos);
                    if (!correosValidos.Any())
                    {
                        throw new Exception("No se encontraron correos validos");
                    }

                    var enviarCorreo = CrearCorreo(correosValidos, body, $"Comprobante Pago {prestamoDetalle.Prestamo.CodigoPrestamo}");

                    return AppResult.New(true, "Correo enviado exitosamente");
                }
                else
                {
                    throw new Exception("No existe PrestamoDetalle");
                }

            }
            catch (Exception ex)
            {
                return AppResult.New(false, ex.Message);
            }
        }




        public async Task<AppResult> EnviarCorreoPagoPIM( Guid prestamoDetalleId)
        {
            try
            {
                var correos = new List<string>
                {
                    "ordonezangel88@gmail.com"
                };


                var prestamoDetalle = await _context.PrestamoDetalle.Where(x => x.Id == prestamoDetalleId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.Prestamo)
                    .FirstOrDefaultAsync();

                if (prestamoDetalle != null)
                {
                    var cliente = prestamoDetalle.Cliente;
                    cliente.ThrowIfNull("No existe cliente");
                    if(cliente.Correo != null && cliente.Correo.Length > 0)
                    {
                        correos.Add(cliente.Correo);
                    }



                    CuentaBancariaVm cuentaBancaria = null;

                    if (prestamoDetalle.CuentaBancariaId != null)
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
                    body += $"<p style='font-size:16px; margin-top:0;'>Hola <strong>{cliente.Nombre.ToUpper()} {cliente.Apellido.ToUpper()}</strong>,</p>";

                    /* Mensaje principal */
                    body += "<table width='100%' style='border-collapse:collapse; margin:12px 0 0 0;'>";
                    body += "<tr>";
                    body += "<td style='font-size:13px; color:#333; font-weight:bold;'>";
                    body += "Se le informa pago registrado por";
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





                    correos = correos.Distinct().ToList();
                    var correosValidos = ObtenerCorreosValidos(correos);
                    if (!correosValidos.Any())
                    {
                        throw new Exception("No se encontraron correos validos");
                    }

                    var enviarCorreo = CrearCorreo(correosValidos, body, $"Comprobante Pago {prestamoDetalle.Prestamo.CodigoPrestamo}");

                    return AppResult.New(true, "Correo enviado exitosamente");
                }
                else
                {
                    throw new Exception("No existe PrestamoDetalle");
                }

            }
            catch (Exception ex)
            {
                return AppResult.New(false, ex.Message);
            }
        }











    }
}

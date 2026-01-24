using Cooperativa.App.Domain.Data;
using Cooperativa.App.Utilidades;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Cooperativa.App.CRUD.CajaCrud;

namespace Cooperativa.App.Soluciones.Pdf
{
    public class PdfContrato
    {

        public class CrearPdfContrato
        {
            public class Query : IRequest<byte[]>
            {

            }

            public class Handler : IRequestHandler<Query, byte[]>
            {
                private readonly CooperativaDbContext _context;
                private readonly IQRServices _iQrServices;
                private readonly IUtilidadesBase iUtilidadesBase;

                public Handler(CooperativaDbContext context, IQRServices qrServices, IUtilidadesBase utilidadesBase)
                {
                    _context = context;
                    _iQrServices = qrServices;
                    iUtilidadesBase = utilidadesBase;
                }

                public async Task<byte[]> Handle(Query request, CancellationToken cancellationToken)
                {

                    #region 
                    /*
                     Si quiero poner subrayado .Underline()   por ejemplo;                       col.Item().Text("CONTRATO DE PRESTAMO PIM").Bold().FontSize(11).Underline().AlignCenter();
                     
                     */
                    #endregion


                    var prestamo = await _context.Prestamo.Where(x => x.Estado == Domain.Enum.EstadoPrestamo.Vigente)
                        .OrderByDescending(x => x.CreatedDate)
                        .Include(x => x.Cliente)
                        .Include(x => x.CuentaBancaria)
                        .FirstOrDefaultAsync();



                    // ==== DATOS EN DURO ====
                    DateTime fecha = DateTime.Now;




                    var fechaActualDesc = iUtilidadesBase.DescribirFecha(fecha);
                    var fechaEntregadoDesc = iUtilidadesBase.DescribirFecha(prestamo.FechaEntragado);
                    var fechaPrimerCuotaDesc = iUtilidadesBase.DescribirFecha(prestamo.FechaEntragado.AddMonths(1));
                    var cantidadInicialDes = iUtilidadesBase.DescribirCantidad(prestamo.CantidadInicial);
                    var cuotaDes = iUtilidadesBase.DescribirCantidad(prestamo.EstimadoAPagarMes);

                    //var textoQR = $"{prestamo.CodigoPrestamo} **{prestamo.CantidadInicial.ToString()} **{fechaEntregadoDesc.Descripcion}";
                    var textoQR = $"Codigo={prestamo.CodigoPrestamo}\n" +
                                  $"Cliente={prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}\n" +
                                  $"Monto=L {prestamo.CantidadInicial:N2}\n" +
                                  $"FechaEntrega={fechaEntregadoDesc.Descripcion}\n" +
                                  $"Interes={prestamo.InteresPorcentaje:N2}%\n" +
                                  $"Cuotas={prestamo.CuotasPagadas}";
                    var qrBytes = await _iQrServices.GenerarQR(textoQR);

                    var pdf = Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(40);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                            var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                            var logoBytes = File.ReadAllBytes(logoPath);


                            page.Header().Row(row =>
                            {
                                // Logo con tamaño fijo 80x60
                                row.ConstantColumn(100).Element(e =>
                                {
                                    e.AlignLeft().Height(80).Width(100).Image(logoBytes).FitArea();
                                });

                                // Texto centrado
                                row.RelativeColumn().Column(col =>
                                {
                                    col.Item().AlignCenter().Text("COOPERATIVA COOPAZ").Bold().FontSize(14);
                                    col.Item().AlignCenter().Text("Francisco Morazán, Honduras").Bold().FontSize(10);
                                    col.Item().AlignCenter().Text($"Fecha: {fecha:dd/MM/yyyy}").FontSize(9);
                                });

                                // QR con tamaño fijo 60x60
                                row.ConstantColumn(90).Height(60).Element(e =>
                                {
                                    e.AlignRight().Height(60).Width(80).Image(qrBytes).FitArea();
                                });
                            });


                            // ======= CONTENIDO =======
                            page.Content().PaddingVertical(10).Column(col =>
                            {
                                // Línea separadora
                                //col.Item().LineHorizontal(0.1f).LineColor(Colors.Grey.Lighten4);
                                col.Item().Height(5);


                                // --- DATOS DEL CLIENTE Y DEL PRÉSTAMO ---
                                col.Item().Text("CONTRATO DE PRESTAMO PIM").Bold().FontSize(12).AlignCenter();
                                col.Item().Height(30);

                                col.Item().Row(row =>
                                {
                                    // ===== Tabla Cliente (izquierda, más ancha) =====
                                    row.RelativeColumn(3).Column(colCliente => // 3 partes
                                    {
                                        colCliente.Item().Text("DATOS DEL CLIENTE").Bold().FontSize(9).AlignCenter();
                                        colCliente.Item().Height(5);

                                        colCliente.Item().Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(1); // Ancho etiqueta
                                                columns.RelativeColumn(2); // valor
                                            });

                                            void AddRow(string label, string value)
                                            {
                                                table.Cell().Border(0.2f).BorderColor(Colors.Grey.Lighten1).Padding(2).Text(label).FontSize(8).Bold();
                                                table.Cell().Border(0.2f).BorderColor(Colors.Grey.Lighten1).Padding(2).Text(value).FontSize(8);
                                            }

                                            AddRow("Nombre Completo", $"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}");
                                            AddRow("Codigo", prestamo.Cliente.CodigoPersona);
                                            AddRow("Dirección", $"{prestamo.Cliente.Direccion}, {prestamo.Cliente.Ciudad}");
                                            AddRow("Lugar de Trabajo", $"{prestamo.Cliente.Ocupacion}, {prestamo.Cliente.LugarTrabajo}");

                                            // Email y Tel en filas separadas
                                            table.Cell().Border(0.25f).BorderColor(Colors.Grey.Lighten1).Padding(2).Text("Email").FontSize(8).Bold();
                                            table.Cell().Border(0.25f).BorderColor(Colors.Grey.Lighten1).Padding(2).Text(prestamo.Cliente.Correo != null ? prestamo.Cliente.Correo : "").FontSize(8);

                                            table.Cell().Border(0.25f).BorderColor(Colors.Grey.Lighten1).Padding(2).Text("Teléfono").FontSize(8).Bold();
                                            table.Cell().Border(0.25f).BorderColor(Colors.Grey.Lighten1).Padding(2).Text(prestamo.Cliente.Telefono != null ? prestamo.Cliente.Telefono : "").FontSize(8);
                                        });
                                    });

                                    row.ConstantColumn(20); // Espacio entre tablas

                                    // ===== Tabla Préstamo (derecha, más estrecha) =====
                                    row.RelativeColumn(2).Column(colPrestamo => // 2 partes
                                    {
                                        colPrestamo.Item().Text("DATOS DEL PRÉSTAMO").Bold().FontSize(9).AlignCenter();
                                        colPrestamo.Item().Height(5);

                                        colPrestamo.Item().Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(2); // Ancho etiqueta
                                                columns.RelativeColumn(1); // Ancho valor
                                            });

                                            void AddRow(string label, string value)
                                            {
                                                table.Cell().Border(0.25f).Padding(2).Text(label).FontSize(8).Bold();
                                                table.Cell().Border(0.25f).Padding(2).Text(value).FontSize(8);
                                            }

                                            AddRow("Documento:", prestamo.CodigoPrestamo);
                                            AddRow("Fecha de Emisión", fecha.ToString("dd/MM/yyyy"));
                                            // Monto y Número de Cuotas en filas separadas
                                            AddRow("Monto", $"L {prestamo.CantidadInicial:N2}");
                                            AddRow("Interes PIM", "10 %");
                                            AddRow("Fecha de Entrega", prestamo.FechaEntragado.ToString("dd/MM/yyyy"));
                                        });
                                    });
                                });


                                col.Item().Height(20);

                                // Texto largo, sin Column ni Row, solo Item
                                // === Bloque completo del contrato ===
                                col.Item().Column(colContrato =>
                                {
                                    colContrato.Item().Text(
                                        $@"En la ciudad de Tegucigalpa, a los {fechaActualDesc.DescripcionCompleta}, COOPAZ otorga un préstamo al señor {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}, por un monto de L {prestamo.CantidadInicial:N2} ({cantidadInicialDes.Descripcion})."
                                    )
                                    .FontSize(10)
                                    .LineHeight(1.4f)
                                    .AlignLeft();

                                    colContrato.Item().Height(10);

                                    // --- Condiciones enumeradas ---
                                    colContrato.Item().Column(colItems =>
                                    {
                                        // Título del bloque de condiciones
                                        colItems.Item().Text("Condiciones del préstamo:").FontSize(10).Bold();
                                        colItems.Item().Height(10);

                                        // Lista de ítems
                                        var items = new (string numeroTitulo, string descripcion)[]
                                        {
        ($"1. Interés: ", $"{prestamo.InteresPorcentaje}% mensual, equivalente a L {prestamo.EstimadoAPagarMes:N2} ({cuotaDes.Descripcion}), calculado de forma diaria."),
        ($"2. Inicio del interés: ", $" A partir del {fechaPrimerCuotaDesc.Descripcion}"),
        ($"3. Fecha de entrega: ", $"{fechaEntregadoDesc.Descripcion}"),
        ($"4. Pago de interés: ", $"Cada día {prestamo.FechaEntragado.Day.ToString()} de mes, deberá pagarse el interés mensual de L {prestamo.EstimadoAPagarMes:N2}."),
        ($"5. Pago anticipado: ", " Si el cliente cancela el préstamo total antes de los primeros 15 días, deberá pagar obligatoriamente los 15 días completos de interés."),
        ($"6. Forma de pago: ", " Libre, permitiendo abonos o cancelación total, respetando la condición anterior."),
        ($"7. Cálculo de interés tras abonos: ", " Si el cliente realiza abonos al capital, el interés diario se recalcula con base en el nuevo saldo actual, lo cual resulta beneficioso para el cliente."),
        //("8. Métodos de pago:", " El cliente podrá realizar sus pagos en efectivo en el cuarto piso del edificio J1 (en secretaria de Microbiología) en UNAH Ciudad Universitaria; así mismo se le facilita al cliente realizar pago con una persona autorizada y fiable por la cooperativa, que en el escenario actual otorga el cliente de antigüedad Elminson Bianney Baca Ávila, con identidad número 0801-1998-04726."),
        //("9. Transferencias bancarias a la cuenta designada por la cooperativa:", " NO DISPONIBLES.")
                                        };

                                        foreach (var item in items)
                                        {
                                            colItems.Item()
                                                .PaddingLeft(20)
                                                .Text(text =>
                                                {
                                                    text.Span(item.numeroTitulo).Bold().FontSize(10).LineHeight(1.4f);
                                                    text.Span(item.descripcion).FontSize(10).LineHeight(1.4f);
                                                });
                                        }
                                    });

                                    colContrato.Item().Height(20);

                                    colContrato.Item().Text(
                                        @"Una vez leído el presente acuerdo, yo, __________________________________________________________, con número de identidad ______________________________________ me comprometo a cumplir con todas las condiciones establecidas en este documento."
                                    )
                                    .FontSize(10)
                                    .LineHeight(1.6f)
                                    .AlignLeft();


                                });



                                col.Item().Height(55);

                                // --- FIRMAS ---
                                col.Item().Row(row =>
                                {
                                    row.RelativeColumn().Column(c =>
                                    {
                                        c.Item().Text("__________________________").AlignCenter();
                                        c.Item().Text("Firma del Cliente").AlignCenter().FontSize(9);
                                    });

                                    row.RelativeColumn().Column(c =>
                                    {
                                        c.Item().Text("__________________________").AlignCenter();
                                        c.Item().Text("Representante Legal / Sello").AlignCenter().FontSize(9);
                                    });
                                });
                            });

                            // ======= PIE DE PÁGINA =======
                            page.Footer().Column(col =>
                            {
                                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                                col.Item().AlignCenter().Text("Cooperativa Coopaz © Todos los derechos reservados").FontSize(8);
                                col.Item().AlignCenter().Text("Este documento es válido únicamente en formato original.").FontSize(7);
                            });
                        });
                    });


                    return await Task.FromResult(pdf.GeneratePdf());
                }
            }
        }













    }
}

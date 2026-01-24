using Cooperativa.App.Domain.Data;
using Cooperativa.App.Domain.Enum;
using Cooperativa.App.Domain.Model;
using Cooperativa.App.Domain.Model.Configuraciones;
using Cooperativa.App.Domain.Model.EntidadesUtiles;
using Cooperativa.App.Domain.Model.People;
using Cooperativa.App.Domain.Model.Prestamos;
using Cooperativa.App.Engine;
using Cooperativa.App.Utilidades;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cooperativa.App.Soluciones.Pdf
{
    public interface IPdfPimService
    {
        Task<byte[]> GenerarContratoByPrestamo(Guid prestamoId);
        Task<AppResult> GenerarPdfEjemplo();


        Task<AppResult> GenerarContratoByPrestamoId(Guid prestamoId);   //En este ya uso el encabezado y footer estatico
        Task<AppResult> GenerarEstadoPrestamoByIdVertical(Guid prestamoId);
        Task<AppResult> GenerarEstadoPrestamoById(Guid prestamoId);
        Task<AppResult> GenerarReporteActivosPIM(Guid? empresaId, Guid? responsableId);
        Task<AppResult> GenerarReportePIM(Guid? empresaId, Guid? responsableId, EstadoPrestamo? estado, bool todosLosEstados);
        Task<AppResult> GenerarReportePIMCliente(Guid clienteId, EstadoPrestamo? estado, bool todosLosEstados, bool detalles);
    }

    public class PdfPimService : IPdfPimService
    {
        private readonly CooperativaDbContext _context;
        private readonly IQRServices _iQrServices;
        private readonly IUtilidadesBase iUtilidadesBase;
        private readonly ICalculationService _calculationService;

        public PdfPimService(CooperativaDbContext context, IQRServices qrServices, IUtilidadesBase utilidadesBase, ICalculationService calculationService)
        {
            _context = context;
            _iQrServices = qrServices;
            iUtilidadesBase = utilidadesBase;
            _calculationService = calculationService;
        }


        //Este fue el primer Pdf que desarrolle
        public async Task<byte[]> GenerarContratoByPrestamo(Guid prestamoId)
        {
            try
            {

                var prestamo = await _context.Prestamo.Where(x => x.Id == prestamoId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.CuentaBancaria)
                    .FirstOrDefaultAsync();
                prestamo.ThrowIfNull("Prestamo no existe");

                if (prestamo.Estado != Domain.Enum.EstadoPrestamo.Vigente)
                {
                    throw new Exception($"Prestamo {prestamo.CodigoPrestamo} no esta Vigente");
                }


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
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la tasa de cambio: {ex.Message}");
            }
        }




        public async Task<AppResult> GenerarContratoByPrestamoId(Guid prestamoId)
        {
            try
            {
                var prestamo = await _context.Prestamo
                    .Where(x => x.Id == prestamoId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.CuentaBancaria)
                    .FirstOrDefaultAsync();

                prestamo.ThrowIfNull("Prestamo no existe");

                //if (prestamo.Estado != Domain.Enum.EstadoPrestamo.Vigente)
                //    throw new Exception($"Prestamo {prestamo.CodigoPrestamo} no está Vigente");

                DateTime fecha = DateTime.Now;



                string genero = "";
                if (prestamo.Cliente.Genero.ToLower().Contains("masculino"))
                {
                    genero = "al Sr";
                }
                else if (prestamo.Cliente.Genero.ToLower().Contains("femenino"))
                {
                    genero = "a la Sra";
                }
                else
                {
                    genero = "al Sr";
                }


                var fechaActualDesc = iUtilidadesBase.DescribirFecha(fecha);
                var fechaEntregadoDesc = iUtilidadesBase.DescribirFecha(prestamo.FechaEntragado);
                var fechaPrimerCuotaDesc = iUtilidadesBase.DescribirFecha(prestamo.FechaEntragado.AddMonths(1));
                var cantidadInicialDes = iUtilidadesBase.DescribirCantidad(prestamo.CantidadInicial);
                var cuotaDes = iUtilidadesBase.DescribirCantidad(prestamo.EstimadoAPagarMes);

                var textoQR = $"Codigo={prestamo.CodigoPrestamo}\n" +
                              $"Cliente={prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}\n" +
                              $"Monto=L {prestamo.CantidadInicial:N2}\n" +
                              $"FechaEntrega={fechaEntregadoDesc.Descripcion}\n" +
                              $"Interes={prestamo.InteresPorcentaje:N2}%\n" +
                              $"Cuotas={prestamo.CuotasPagadas}";

                var qrBytes = await _iQrServices.GenerarQR(textoQR);

                // Logo
                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                        // ==== Encabezado usando clase estática ====
                        EncabezadoPIM(page, logoBytes, qrBytes);

                        // ==== Contenido ====
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            col.Item().Text("CONTRATO DE PRÉSTAMO PIM").Bold().FontSize(12).AlignCenter();
                            col.Item().Height(20);

                            // Cliente y préstamo
                            col.Item().Row(row =>
                            {
                                // Datos del Cliente
                                row.RelativeColumn(3).Column(colCliente =>
                                {
                                    colCliente.Item().Text("DATOS DEL CLIENTE").Bold().FontSize(9).AlignCenter();
                                    colCliente.Item().Height(5);

                                    colCliente.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(2);
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
                                        AddRow("Email", prestamo.Cliente.Correo ?? "");
                                        AddRow("Teléfono", prestamo.Cliente.Telefono ?? "");
                                    });
                                });

                                row.ConstantColumn(20);

                                // Datos del Préstamo
                                row.RelativeColumn(2).Column(colPrestamo =>
                                {
                                    colPrestamo.Item().Text("DATOS DEL PRÉSTAMO").Bold().FontSize(9).AlignCenter();
                                    colPrestamo.Item().Height(5);

                                    colPrestamo.Item().Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(2);
                                        });

                                        void AddRow(string label, string value)
                                        {
                                            table.Cell().Border(0.25f).Padding(2).Text(label).FontSize(8).Bold();
                                            table.Cell().Border(0.25f).Padding(2).Text(value).FontSize(8);
                                        }

                                        var fechaEntrega = "";
                                        if(prestamo.FechaEntragado == null || prestamo.FechaEntragado == DateTime.MinValue)
                                        {
                                            fechaEntrega = "Sin fecha de entrega";
                                        }
                                        else
                                        {
                                            fechaEntrega = prestamo.FechaEntragado.ToString("dd-MM-yyyy");
                                        }

                                        AddRow("Documento", prestamo.CodigoPrestamo);
                                        AddRow("Estado", prestamo.Estado_Descripcion);
                                        AddRow("Fecha de Emisión", fecha.ToString("dd-MM-yyyy hh:mm tt"));
                                        AddRow("Monto", $"L {prestamo.CantidadInicial:N2}");
                                        AddRow("Interes PIM", $"{prestamo.InteresPorcentaje}%");
                                        AddRow("Fecha de Entrega", fechaEntrega);
                                    });
                                });
                            });

                            col.Item().Height(20);

                            // Bloque de condiciones del préstamo
                            col.Item().Column(colContrato =>
                            {
                                colContrato.Item().Text(
                                    $@"En la ciudad de Tegucigalpa, a los {fechaActualDesc.DescripcionCompleta}, COOPAZ otorga un préstamo {genero}. {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}, por un monto de L {prestamo.CantidadInicial:N2} ({cantidadInicialDes.Descripcion})."
                                ).FontSize(10).LineHeight(1.4f).AlignLeft();

                                colContrato.Item().Height(10);

                                var items = new (string numeroTitulo, string descripcion)[]
                                {
                            ($"1. Interés: ", $"{prestamo.InteresPorcentaje}% mensual, equivalente a L {prestamo.EstimadoAPagarMes:N2} ({cuotaDes.Descripcion}), calculado de forma diaria."),
                            ($"2. Inicio del interés: ", $"A partir del {fechaPrimerCuotaDesc.Descripcion}"),
                            ($"3. Fecha de entrega: ", $"{fechaEntregadoDesc.Descripcion}"),
                            ($"4. Pago de interés: ", $"Cada día {prestamo.FechaEntragado.Day} de mes, deberá pagarse el interés mensual de L {prestamo.EstimadoAPagarMes:N2}."),
                            ($"5. Pago anticipado: ", "Si el cliente cancela el préstamo total antes de los primeros 15 días, deberá pagar obligatoriamente los 15 días completos de interés."),
                            ($"6. Forma de pago: ", "Libre, permitiendo abonos o cancelación total, respetando la condición anterior."),
                            ($"7. Cálculo de interés tras abonos: ", "Si el cliente realiza abonos al capital, el interés diario se recalcula con base en el nuevo saldo actual, lo cual resulta beneficioso para el cliente.")
                                };

                                foreach (var item in items)
                                {
                                    colContrato.Item()
                                        .PaddingLeft(20)
                                        .Text(text =>
                                        {
                                            text.Span(item.numeroTitulo).Bold().FontSize(10).LineHeight(1.4f);
                                            text.Span(item.descripcion).FontSize(10).LineHeight(1.4f);
                                        });
                                }

                                colContrato.Item().Height(20);

                                colContrato.Item().Text(
                                    @"Una vez leído el presente acuerdo, yo, __________________________________________________________, con número de identidad ______________________________________ me comprometo a cumplir con todas las condiciones establecidas en este documento."
                                ).FontSize(10).LineHeight(1.6f).AlignLeft();
                            });

                            col.Item().Height(55);

                            // Firmas
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

                        // ==== Pie de página usando clase estática ====
                        FooterPIM(page);
                    });
                });

                var pdfBytes = await Task.FromResult(pdf.GeneratePdf());

                return AppResult.New(true, pdfBytes, $"Contrato {prestamo.CodigoPrestamo} - {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar contrato: {ex.Message}");
            }
        }







        public async Task<AppResult> GenerarEstadoPrestamoByIdVertical(Guid prestamoId)
        {
            try
            {
                var prestamo = await _context.Prestamo
                    .Where(x => x.Id == prestamoId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.PrestamoDetalles)
                    .Include(x => x.CuentaBancaria)
                    .FirstOrDefaultAsync();

                prestamo.ThrowIfNull("Préstamo no existe");
                prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled && !x.IsSoftDeleted).ToList();
                var ultimoDetalle = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                var cuentaBancaria = "Efectivo";
                if(prestamo.CuentaBancaria != null)
                {
                    cuentaBancaria = prestamo.CuentaBancaria.NumeroCuenta;
                }

                DateTime fechaActual = DateTime.Now;

                var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);




                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                var textoQR = $"Estado del préstamo {prestamo.CodigoPrestamo}\n" +
                              $"Cliente: {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}\n" +
                              $"Fecha: {fechaActual:dd/MM/yyyy}";

                var qrBytes = await _iQrServices.GenerarQR(textoQR);

                // === Crear PDF ===
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Times New Roman"));

                        // Encabezado reutilizable
                        EncabezadoPIM(page, logoBytes, qrBytes);

                        // === CONTENIDO ===
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // ======= TÍTULO PRINCIPAL =======
                            col.Item().Text($"ESTADO DE PRÉSTAMO {prestamo.CodigoPrestamo}")
                                .Bold().FontSize(12)
                                .AlignCenter()
                                .FontColor(Colors.Blue.Darken3);
                            col.Item().Height(20);



                            // ======= SECCIÓN: INFORMACIÓN DEL CLIENTE =======
                            col.Item().Column(c =>
                            {
                                c.Item().Text("INFORMACIÓN DEL CLIENTE")
                                    .Bold().FontSize(9)
                                    .FontColor(Colors.Blue.Darken3);

                                c.Item().Height(2);

                                // Fondo gris claro y diseño limpio
                                c.Item().Element(e =>
                                {
                                    e.Background(Colors.Grey.Lighten4)
                                     .CornerRadius(2)
                                     .Padding(2)
                                     .PaddingHorizontal(7)
                                     .Column(colCliente =>
                                     {
                                         // ====== Fila 1: Nombre (ocupa 2 columnas) + Identificación + Teléfono ======
                                         colCliente.Item().Row(row =>
                                         {
                                             // Nombre (2 columnas)
                                             row.RelativeColumn(2).Column(cc =>
                                             {
                                                 cc.Item().Text("Nombre").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text($"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}")
                                                     .FontSize(8);
                                             });

                                             // Identificación
                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Codigo Cliente").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.CodigoPersona ?? "—").FontSize(8);
                                             });

                                             // Teléfono
                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Teléfono").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Telefono ?? "—").FontSize(8);
                                             });
                                         });

                                         colCliente.Item().Height(5);

                                         // ====== Fila 2: Correo + Lugar de trabajo + Ciudad + País ======
                                         colCliente.Item().Row(row =>
                                         {
                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Correo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Correo ?? "—").FontSize(8);
                                             });

                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Ciudad").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Ciudad ?? "—").FontSize(8);
                                             });

                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("País").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Pais ?? "—").FontSize(8);
                                             });

                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Lugar de trabajo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.LugarTrabajo ?? "—").FontSize(8);
                                             });
                                         });

                                         colCliente.Item().Height(5);

                                         // ====== Fila 3: Dirección completa (ocupa las 4 columnas) ======
                                         //colCliente.Item().Row(row =>
                                         //{
                                         //    row.RelativeColumn(4).Column(cc =>
                                         //    {
                                         //        cc.Item().Text("Dirección").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                         //        cc.Item().Text($"{prestamo.Cliente.Direccion ?? "—"}")
                                         //            .FontSize(8);
                                         //    });
                                         //});
                                     });
                                });
                            });





                            col.Item().Height(12);




                            // ======= SECCIÓN: INFORMACIÓN DEL PRÉSTAMO =======
                            col.Item().Column(c =>
                            {
                                c.Item().Text("DETALLES DEL PRÉSTAMO")
                                    .Bold().FontSize(9)
                                    .FontColor(Colors.Blue.Darken3);

                                c.Item().Height(2);

                                c.Item().Element(e =>
                                {
                                    e.Background(Colors.Grey.Lighten5)
                                     .CornerRadius(2)
                                     .Padding(2)
                                     .Border(0.3f)
                                     .BorderColor(Colors.Grey.Lighten1)
                                     .Column(colPrestamo =>
                                     {
                                         void Cell(RowDescriptor row, string label, string value, bool alternate = false, string fontColor = null)
                                         {
                                             row.RelativeColumn(1)
                                                .Background(alternate ? Colors.White : Colors.Grey.Lighten4)
                                                .Border(0.25f)
                                                .BorderColor(Colors.Grey.Lighten1)
                                                .PaddingVertical(3)
                                                .PaddingHorizontal(4)
                                                .Column(c2 =>
                                                {
                                                    c2.Item().Text(label)
                                                        .Bold()
                                                        .FontSize(8)
                                                        .FontColor(Colors.Grey.Darken3);

                                                    c2.Item().Text(value ?? "—")
                                                        .FontSize(8)
                                                        .FontColor(fontColor ?? Colors.Grey.Darken4);
                                                });
                                         }

                                         // ====== Fila 1 ======
                                         colPrestamo.Item().Row(row =>
                                         {
                                             Cell(row, "Código", prestamo.CodigoPrestamo);
                                             Cell(row, "Monto inicial", $"L {prestamo.CantidadInicial:N2}");
                                             Cell(row, "Interés mensual", $"{prestamo.InteresPorcentaje:N2}%");
                                             Cell(row, "Fecha de entrega", prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
                                             Cell(row, "Moneda", prestamo.Moneda_Descripcion); 
       
                                         });

                                         // ====== Fila 2 ======
                                         colPrestamo.Item().Row(row =>
                                         {
                                             Cell(row, "Tipo de préstamo", prestamo.TipoPrestamo_Descripcion, true);
                                             Cell(row, "Transacción Cuenta", cuentaBancaria, true);
                                             Cell(row, "Estado", prestamo.Estado_Descripcion, true);
                                             Cell(row, "Cuotas pagadas", prestamo.CuotasPagadas.ToString(), true);
                                             Cell(row, "Monto pagado", $"L {prestamo.MontoPagado:N2}", true);
                                         });

                                         // ====== Fila 3 ======
                                         colPrestamo.Item().Row(row =>
                                         {
                                             Cell(row, "Saldo actual", $"L {prestamo.RestaCapital:N2}");
                                             Cell(row, "Fecha Ultimo Pago", prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
                                             //Cell(row, "Pago Minimo", $"{ultimoDetalle.ProximoPago.ToString():N2}");
                                             Cell(row, "Pago Minimo", $"{prestamo.EstimadoAPagarMes.ToString():N2}");
                                             Cell(row, "Interes Generado", $"{calculo.InteresGenerado:N2}", false, fontColor: calculo.InteresGenerado > prestamo.EstimadoAPagarMes ? Colors.Red.Medium : Colors.Grey.Darken4);
                                             Cell(row, "Deuda Total", calculo.DeudaTotal != 0 ? $"{calculo.DeudaTotal:N2}" : $"{(prestamo.RestaCapital + calculo.InteresGenerado):N2}");
                                         });
                                     });
                                });
                            });

                            

                            col.Item().Height(4);




                            // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                            col.Item().Table(table =>
                            {
                                // ==== Definición de columnas ====
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(0.5f);  // Cuota
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1);
                                });

                                // ==== Encabezado con borde total ====
                                string[] headers = { "#", "Recibido", "Fecha Pago", "Interés",
                                         "Capital Aportado", "Pago Hasta", "Resta Capital",
                                         "F. Próx. Pago", "Próximo Pago" 
                                };

                                foreach (var header in headers)
                                {
                                    table.Cell()
                                        .Background(Colors.Blue.Lighten5)   // azul más suave
                                        .Border(0.5f)                       // borde completo por celda
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(3)
                                        .AlignCenter()
                                        .Text(header)
                                        .Bold()
                                        .FontSize(8)
                                        .FontColor(Colors.Black);           // texto negro
                                }

                                // ==== Filas de datos ====
                                bool alternate = false;
                                foreach (var d in prestamo.PrestamoDetalles.OrderBy(x => x.NumeroCuota))
                                {
                                    var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                    alternate = !alternate;

                                    // Primera celda (# de cuota)
                                    table.Cell()
                                        .Background(Colors.Blue.Lighten5)
                                        .Border(0.25f)
                                        .BorderColor(Colors.Grey.Lighten4)
                                        .Padding(4)
                                        .AlignCenter()
                                        .Text(d.NumeroCuota.ToString())
                                        .FontColor(Colors.Blue.Darken3)
                                        .Bold();

                                    // Resto de las columnas
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{d.TotalAPagar:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.CreatedDate.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoInteres:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoCapital:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaPago.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.RestaCapital:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaProximoPago.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.ProximoPago:N2}");
                                }
                            });




                            col.Item().Height(20);
                            col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


                            col.Item().Height(1);
                            // ======= TEXTO DE NOTA =======
                            col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
                                .Italic()                 // Texto en cursiva
                                .FontSize(8);             // Más pequeño que el texto normal
                                //.FontColor(Colors.Grey.Darken2); // Color gris suave
                        });

                        // Pie reutilizable
                        FooterPIM(page);
                    });
                });

                var pdfBytes = pdf.GeneratePdf();

                return AppResult.New(
                    true,
                    pdfBytes,
                    $"Estado-{prestamo.CodigoPrestamo}-{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el reporte de estado del préstamo: {ex.Message}");
            }
        }



        public async Task<AppResult> GenerarEstadoPrestamoById(Guid prestamoId)
        {
            try
            {
                var prestamo = await _context.Prestamo
                    .Where(x => x.Id == prestamoId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.PrestamoDetalles)
                    .Include(x => x.PrestamoDetalles).ThenInclude(s => s.CuentaBancaria)
                    .Include(x => x.CuentaBancaria)
                    .FirstOrDefaultAsync();

                prestamo.ThrowIfNull("Préstamo no existe");
                prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled && !x.IsSoftDeleted).ToList();
                var ultimoDetalle = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

                var cuentaBancaria = "Efectivo";
                if (prestamo.CuentaBancaria != null)
                {
                    cuentaBancaria = prestamo.CuentaBancaria.NumeroCuenta;
                }

                DateTime fechaActual = DateTime.Now;

                var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);




                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                var textoQR = $"Estado del préstamo {prestamo.CodigoPrestamo}\n" +
                              $"Cliente: {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}\n" +
                              $"Fecha: {fechaActual:dd/MM/yyyy}";

                var qrBytes = await _iQrServices.GenerarQR(textoQR);

                // === Crear PDF ===
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());        //Landscape() => Horizontal
                        page.Margin(20);
                        page.PageColor(Colors.White);
                        //page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Times New Roman"));
                        page.DefaultTextStyle(x => x.FontSize(8));  //La letra es Arial
                        // Encabezado reutilizable
                        EncabezadoPIM(page, logoBytes, qrBytes);

                        // === CONTENIDO ===
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // ======= TÍTULO PRINCIPAL =======
                            col.Item().Text($"ESTADO DE PRÉSTAMO {prestamo.CodigoPrestamo}")
                                .Bold().FontSize(12)
                                .AlignCenter();
                                //.FontColor(Colors.Blue.Darken3);
                            col.Item().Height(20);



                            // ======= SECCIÓN: INFORMACIÓN DEL CLIENTE =======
                            col.Item().Column(c =>
                            {
                                c.Item().Text("INFORMACIÓN DEL CLIENTE")
                                    .Bold().FontSize(9);
                                    //.FontColor(Colors.Blue.Darken3);

                                c.Item().Height(2);

                                // Fondo gris claro y diseño limpio
                                c.Item().Element(e =>
                                {
                                    e.Background(Colors.Grey.Lighten4)
                                     .CornerRadius(2)
                                     .Padding(2)
                                     .PaddingHorizontal(7)
                                     .Column(colCliente =>
                                     {
                                         // ====== Fila 1: Nombre (ocupa 2 columnas) + Identificación + Teléfono ======
                                         colCliente.Item().Row(row =>
                                         {
                                             // Nombre (2 columnas)
                                             row.RelativeColumn(2).Column(cc =>
                                             {
                                                 cc.Item().Text("Nombre").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text($"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}")
                                                     .FontSize(8);
                                             });

                                             // Identificación
                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Codigo Cliente").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.CodigoPersona ?? "—").FontSize(8);
                                             });

                                             // Teléfono
                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Teléfono").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Telefono ?? "—").FontSize(8);
                                             });
                                         });

                                         colCliente.Item().Height(5);

                                         // ====== Fila 2: Correo + Lugar de trabajo + Ciudad + País ======
                                         colCliente.Item().Row(row =>
                                         {
                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Correo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Correo ?? "—").FontSize(8);
                                             });

                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Ciudad").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Ciudad ?? "—").FontSize(8);
                                             });

                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("País").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.Pais ?? "—").FontSize(8);
                                             });

                                             row.RelativeColumn(1).Column(cc =>
                                             {
                                                 cc.Item().Text("Lugar de trabajo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                                 cc.Item().Text(prestamo.Cliente.LugarTrabajo ?? "—").FontSize(8);
                                             });
                                         });

                                         colCliente.Item().Height(5);

                                         // ====== Fila 3: Dirección completa (ocupa las 4 columnas) ======
                                         //colCliente.Item().Row(row =>
                                         //{
                                         //    row.RelativeColumn(4).Column(cc =>
                                         //    {
                                         //        cc.Item().Text("Dirección").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                         //        cc.Item().Text($"{prestamo.Cliente.Direccion ?? "—"}")
                                         //            .FontSize(8);
                                         //    });
                                         //});
                                     });
                                });
                            });





                            col.Item().Height(12);




                            // ======= SECCIÓN: INFORMACIÓN DEL PRÉSTAMO =======
                            col.Item().Column(c =>
                            {
                                c.Item().Text("DETALLES DEL PRÉSTAMO")
                                    .Bold().FontSize(9);
                                    //.FontColor(Colors.Blue.Darken3);

                                c.Item().Height(2);

                                c.Item().Element(e =>
                                {
                                    e.Background(Colors.Grey.Lighten5)
                                     .CornerRadius(2)
                                     .Padding(2)
                                     .Border(0.3f)
                                     .BorderColor(Colors.Grey.Lighten1)
                                     .Column(colPrestamo =>
                                     {
                                         void Cell(RowDescriptor row, string label, string value, bool alternate = false, string fontColor = null)
                                         {
                                             row.RelativeColumn(1)
                                                .Background(alternate ? Colors.White : Colors.Grey.Lighten4)
                                                .Border(0.25f)
                                                .BorderColor(Colors.Grey.Lighten1)
                                                .PaddingVertical(3)
                                                .PaddingHorizontal(4)
                                                .Column(c2 =>
                                                {
                                                    c2.Item().Text(label)
                                                        .Bold()
                                                        .FontSize(8)
                                                        .FontColor(Colors.Grey.Darken3);

                                                    c2.Item().Text(value ?? "—")
                                                        .FontSize(8)
                                                        .FontColor(fontColor ?? Colors.Grey.Darken4);
                                                });
                                         }

                                         // ====== Fila 1 ======
                                         colPrestamo.Item().Row(row =>
                                         {
                                             Cell(row, "Código", prestamo.CodigoPrestamo);
                                             Cell(row, "Monto inicial", $" {prestamo.CantidadInicial:N2}");
                                             Cell(row, "Interés mensual", $"{prestamo.InteresPorcentaje:N2}%");
                                             Cell(row, "Fecha de entrega", prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
                                             Cell(row, "Moneda", prestamo.Moneda_Descripcion);

                                         });

                                         // ====== Fila 2 ======
                                         colPrestamo.Item().Row(row =>
                                         {
                                             Cell(row, "Tipo de préstamo", prestamo.TipoPrestamo_Descripcion, true);
                                             Cell(row, "Transacción Cuenta", cuentaBancaria, true);
                                             Cell(row, "Estado", prestamo.Estado_Descripcion, true);
                                             Cell(row, "Cuotas pagadas", prestamo.CuotasPagadas.ToString(), true);
                                             Cell(row, "Monto pagado", $" {prestamo.MontoPagado:N2}", true);
                                         });

                                         // ====== Fila 3 ======
                                         colPrestamo.Item().Row(row =>
                                         {
                                             Cell(row, "Saldo actual", $" {prestamo.RestaCapital:N2}");
                                             Cell(row, "Fecha Ultimo Pago", prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
                                             //Cell(row, "Pago Minimo", $"{ultimoDetalle.ProximoPago.ToString():N2}");
                                             Cell(row, "Pago Minimo", $" {prestamo.EstimadoAPagarMes:N2}");
                                             Cell(row, "Interes Generado", $"{calculo.InteresGenerado:N2}", false, fontColor: calculo.InteresGenerado > prestamo.EstimadoAPagarMes ? Colors.Red.Medium : Colors.Grey.Darken4);
                                             Cell(row, "Deuda Total", calculo.DeudaTotal != 0 ? $"{calculo.DeudaTotal:N2}" : $"{(prestamo.RestaCapital + calculo.InteresGenerado):N2}");
                                         });
                                     });
                                });
                            });



                            col.Item().Height(4);




                            // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                            col.Item().Table(table =>
                            {
                                // ==== Definición de columnas ====
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(0.4f);  // #
                                    columns.RelativeColumn(0.9f);  // Recibido
                                    columns.RelativeColumn(1.0f);  // Cuenta Origen
                                    columns.RelativeColumn(0.9f);  // Fecha Pago
                                    columns.RelativeColumn(0.9f);  // Interés
                                    columns.RelativeColumn(1.0f);  // Capital
                                    columns.RelativeColumn(0.9f);  // Pago Hasta
                                    columns.RelativeColumn(1.0f);  // Resta Capital
                                    columns.RelativeColumn(0.9f);  // Próx Fecha
                                    columns.RelativeColumn(0.9f);  // Próx Pago
                                    columns.RelativeColumn(2.4f);  // Observación
                                });

                                // ==== Encabezado con borde total ====
                                string[] headers = { "#", "Recibido", "Cuenta Origen", "Fecha Pago", "Interés",
                                         "Capital Aportado", "Pago Hasta", "Resta Capital",
                                         "F. Próx. Pago", "Próximo Pago", "***"
                                };

                                foreach (var header in headers)
                                {
                                    table.Cell()
                                        .Background(Colors.Blue.Lighten5)   // azul más suave
                                        .Border(0.2f)                       // borde completo por celda
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(3)
                                        .AlignCenter()
                                        .Text(header)
                                        .Bold()
                                        .FontSize(8)
                                        .FontColor(Colors.Black);           // texto negro
                                }

                                // ==== Filas de datos ====
                                bool alternate = false;
                                foreach (var d in prestamo.PrestamoDetalles.OrderBy(x => x.NumeroCuota))
                                {
                                    var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                    alternate = !alternate;

                                    string cuentaOrigen = "";

                                    if(d.CuentaBancaria != null)
                                    {
                                        cuentaOrigen = d.CuentaBancaria.NumeroCuenta;
                                    }


                                    // Primera celda (# de cuota)
                                    table.Cell()
                                        //.Background(Colors.Blue.Lighten5)
                                        //.Border(0.25f)
                                        //.BorderColor(Colors.Grey.Lighten4)
                                        .Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4)
                                        .Padding(4)
                                        .AlignCenter()
                                        .Text(d.NumeroCuota.ToString());
                                        //.FontColor(Colors.Blue.Darken3)
                                        //.Bold();

                                    // Resto de las columnas
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{d.TotalAPagar:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{cuentaOrigen}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.CreatedDate.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoInteres:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoCapital:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaPago.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.RestaCapital:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaProximoPago.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.ProximoPago:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.Observacion}");
                                }
                            });




                            col.Item().Height(20);
                            col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


                            col.Item().Height(1);
                            // ======= TEXTO DE NOTA =======
                            col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
                                .Italic()                 // Texto en cursiva
                                .AlignCenter()
                                .FontSize(8);             // Más pequeño que el texto normal
                                                          //.FontColor(Colors.Grey.Darken2); // Color gris suave
                        });

                        // Pie reutilizable
                        FooterPIM(page);
                    });
                });

                var pdfBytes = pdf.GeneratePdf();

                return AppResult.New(
                    true,
                    pdfBytes,
                    $"Estado-{prestamo.CodigoPrestamo}-{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el reporte de estado del préstamo: {ex.Message}");
            }
        }



        //public Task<AppResult> GenerarEstadoPrestamo(Prestamo prestamo, ConfiguracionPrestamo configuracion)
        //{
        //    try
        //    {

        //        prestamo.ThrowIfNull("Préstamo no existe");
        //        prestamo.PrestamoDetalles = prestamo.PrestamoDetalles.Where(x => x.Enabled && !x.IsSoftDeleted).ToList();
        //        var ultimoDetalle = prestamo.PrestamoDetalles.OrderByDescending(x => x.CreatedDate).FirstOrDefault();

        //        var cuentaBancaria = "Efectivo";
        //        if (prestamo.CuentaBancaria != null)
        //        {
        //            cuentaBancaria = prestamo.CuentaBancaria.NumeroCuenta;
        //        }

        //        DateTime fechaActual = DateTime.Now;

        //        var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);




        //        var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
        //        var logoBytes = File.ReadAllBytes(logoPath);

        //        var textoQR = $"Estado del préstamo {prestamo.CodigoPrestamo}\n" +
        //                      $"Cliente: {prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}\n" +
        //                      $"Fecha: {fechaActual:dd/MM/yyyy}";

        //        var qrBytes = await _iQrServices.GenerarQR(textoQR);

        //        // === Crear PDF ===
        //        var pdf = Document.Create(container =>
        //        {
        //            container.Page(page =>
        //            {
        //                page.Size(PageSizes.A4.Landscape());        //Landscape() => Horizontal
        //                page.Margin(20);
        //                page.PageColor(Colors.White);
        //                //page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Times New Roman"));
        //                page.DefaultTextStyle(x => x.FontSize(8));  //La letra es Arial
        //                // Encabezado reutilizable
        //                EncabezadoPIM(page, logoBytes, qrBytes);

        //                // === CONTENIDO ===
        //                page.Content().PaddingVertical(10).Column(col =>
        //                {
        //                    // ======= TÍTULO PRINCIPAL =======
        //                    col.Item().Text($"ESTADO DE PRÉSTAMO {prestamo.CodigoPrestamo}")
        //                        .Bold().FontSize(12)
        //                        .AlignCenter();
        //                    //.FontColor(Colors.Blue.Darken3);
        //                    col.Item().Height(20);



        //                    // ======= SECCIÓN: INFORMACIÓN DEL CLIENTE =======
        //                    col.Item().Column(c =>
        //                    {
        //                        c.Item().Text("INFORMACIÓN DEL CLIENTE")
        //                            .Bold().FontSize(9);
        //                        //.FontColor(Colors.Blue.Darken3);

        //                        c.Item().Height(2);

        //                        // Fondo gris claro y diseño limpio
        //                        c.Item().Element(e =>
        //                        {
        //                            e.Background(Colors.Grey.Lighten4)
        //                             .CornerRadius(2)
        //                             .Padding(2)
        //                             .PaddingHorizontal(7)
        //                             .Column(colCliente =>
        //                             {
        //                                 // ====== Fila 1: Nombre (ocupa 2 columnas) + Identificación + Teléfono ======
        //                                 colCliente.Item().Row(row =>
        //                                 {
        //                                     // Nombre (2 columnas)
        //                                     row.RelativeColumn(2).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("Nombre").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text($"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}")
        //                                             .FontSize(8);
        //                                     });

        //                                     // Identificación
        //                                     row.RelativeColumn(1).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("Codigo Cliente").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text(prestamo.Cliente.CodigoPersona ?? "—").FontSize(8);
        //                                     });

        //                                     // Teléfono
        //                                     row.RelativeColumn(1).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("Teléfono").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text(prestamo.Cliente.Telefono ?? "—").FontSize(8);
        //                                     });
        //                                 });

        //                                 colCliente.Item().Height(5);

        //                                 // ====== Fila 2: Correo + Lugar de trabajo + Ciudad + País ======
        //                                 colCliente.Item().Row(row =>
        //                                 {
        //                                     row.RelativeColumn(1).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("Correo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text(prestamo.Cliente.Correo ?? "—").FontSize(8);
        //                                     });

        //                                     row.RelativeColumn(1).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("Ciudad").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text(prestamo.Cliente.Ciudad ?? "—").FontSize(8);
        //                                     });

        //                                     row.RelativeColumn(1).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("País").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text(prestamo.Cliente.Pais ?? "—").FontSize(8);
        //                                     });

        //                                     row.RelativeColumn(1).Column(cc =>
        //                                     {
        //                                         cc.Item().Text("Lugar de trabajo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                         cc.Item().Text(prestamo.Cliente.LugarTrabajo ?? "—").FontSize(8);
        //                                     });
        //                                 });

        //                                 colCliente.Item().Height(5);

        //                                 // ====== Fila 3: Dirección completa (ocupa las 4 columnas) ======
        //                                 //colCliente.Item().Row(row =>
        //                                 //{
        //                                 //    row.RelativeColumn(4).Column(cc =>
        //                                 //    {
        //                                 //        cc.Item().Text("Dirección").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
        //                                 //        cc.Item().Text($"{prestamo.Cliente.Direccion ?? "—"}")
        //                                 //            .FontSize(8);
        //                                 //    });
        //                                 //});
        //                             });
        //                        });
        //                    });





        //                    col.Item().Height(12);




        //                    // ======= SECCIÓN: INFORMACIÓN DEL PRÉSTAMO =======
        //                    col.Item().Column(c =>
        //                    {
        //                        c.Item().Text("DETALLES DEL PRÉSTAMO")
        //                            .Bold().FontSize(9);
        //                        //.FontColor(Colors.Blue.Darken3);

        //                        c.Item().Height(2);

        //                        c.Item().Element(e =>
        //                        {
        //                            e.Background(Colors.Grey.Lighten5)
        //                             .CornerRadius(2)
        //                             .Padding(2)
        //                             .Border(0.3f)
        //                             .BorderColor(Colors.Grey.Lighten1)
        //                             .Column(colPrestamo =>
        //                             {
        //                                 void Cell(RowDescriptor row, string label, string value, bool alternate = false, string fontColor = null)
        //                                 {
        //                                     row.RelativeColumn(1)
        //                                        .Background(alternate ? Colors.White : Colors.Grey.Lighten4)
        //                                        .Border(0.25f)
        //                                        .BorderColor(Colors.Grey.Lighten1)
        //                                        .PaddingVertical(3)
        //                                        .PaddingHorizontal(4)
        //                                        .Column(c2 =>
        //                                        {
        //                                            c2.Item().Text(label)
        //                                                .Bold()
        //                                                .FontSize(8)
        //                                                .FontColor(Colors.Grey.Darken3);

        //                                            c2.Item().Text(value ?? "—")
        //                                                .FontSize(8)
        //                                                .FontColor(fontColor ?? Colors.Grey.Darken4);
        //                                        });
        //                                 }

        //                                 // ====== Fila 1 ======
        //                                 colPrestamo.Item().Row(row =>
        //                                 {
        //                                     Cell(row, "Código", prestamo.CodigoPrestamo);
        //                                     Cell(row, "Monto inicial", $" {prestamo.CantidadInicial:N2}");
        //                                     Cell(row, "Interés mensual", $"{prestamo.InteresPorcentaje:N2}%");
        //                                     Cell(row, "Fecha de entrega", prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
        //                                     Cell(row, "Moneda", prestamo.Moneda_Descripcion);

        //                                 });

        //                                 // ====== Fila 2 ======
        //                                 colPrestamo.Item().Row(row =>
        //                                 {
        //                                     Cell(row, "Tipo de préstamo", prestamo.TipoPrestamo_Descripcion, true);
        //                                     Cell(row, "Transacción Cuenta", cuentaBancaria, true);
        //                                     Cell(row, "Estado", prestamo.Estado_Descripcion, true);
        //                                     Cell(row, "Cuotas pagadas", prestamo.CuotasPagadas.ToString(), true);
        //                                     Cell(row, "Monto pagado", $" {prestamo.MontoPagado:N2}", true);
        //                                 });

        //                                 // ====== Fila 3 ======
        //                                 colPrestamo.Item().Row(row =>
        //                                 {
        //                                     Cell(row, "Saldo actual", $" {prestamo.RestaCapital:N2}");
        //                                     Cell(row, "Fecha Ultimo Pago", prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
        //                                     //Cell(row, "Pago Minimo", $"{ultimoDetalle.ProximoPago.ToString():N2}");
        //                                     Cell(row, "Pago Minimo", $" {prestamo.EstimadoAPagarMes:N2}");
        //                                     Cell(row, "Interes Generado", $"{calculo.InteresGenerado:N2}", false, fontColor: calculo.InteresGenerado > prestamo.EstimadoAPagarMes ? Colors.Red.Medium : Colors.Grey.Darken4);
        //                                     Cell(row, "Deuda Total", calculo.DeudaTotal != 0 ? $"{calculo.DeudaTotal:N2}" : $"{(prestamo.RestaCapital + calculo.InteresGenerado):N2}");
        //                                 });
        //                             });
        //                        });
        //                    });



        //                    col.Item().Height(4);




        //                    // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
        //                    col.Item().Table(table =>
        //                    {
        //                        // ==== Definición de columnas ====
        //                        table.ColumnsDefinition(columns =>
        //                        {
        //                            columns.RelativeColumn(0.4f);  // #
        //                            columns.RelativeColumn(0.9f);  // Recibido
        //                            columns.RelativeColumn(1.0f);  // Cuenta Origen
        //                            columns.RelativeColumn(0.9f);  // Fecha Pago
        //                            columns.RelativeColumn(0.9f);  // Interés
        //                            columns.RelativeColumn(1.0f);  // Capital
        //                            columns.RelativeColumn(0.9f);  // Pago Hasta
        //                            columns.RelativeColumn(1.0f);  // Resta Capital
        //                            columns.RelativeColumn(0.9f);  // Próx Fecha
        //                            columns.RelativeColumn(0.9f);  // Próx Pago
        //                            columns.RelativeColumn(2.4f);  // Observación
        //                        });

        //                        // ==== Encabezado con borde total ====
        //                        string[] headers = { "#", "Recibido", "Cuenta Origen", "Fecha Pago", "Interés",
        //                                 "Capital Aportado", "Pago Hasta", "Resta Capital",
        //                                 "F. Próx. Pago", "Próximo Pago", "***"
        //                        };

        //                        foreach (var header in headers)
        //                        {
        //                            table.Cell()
        //                                .Background(Colors.Blue.Lighten5)   // azul más suave
        //                                .Border(0.2f)                       // borde completo por celda
        //                                .BorderColor(Colors.Grey.Lighten1)
        //                                .PaddingVertical(4)
        //                                .PaddingHorizontal(3)
        //                                .AlignCenter()
        //                                .Text(header)
        //                                .Bold()
        //                                .FontSize(8)
        //                                .FontColor(Colors.Black);           // texto negro
        //                        }

        //                        // ==== Filas de datos ====
        //                        bool alternate = false;
        //                        foreach (var d in prestamo.PrestamoDetalles.OrderBy(x => x.NumeroCuota))
        //                        {
        //                            var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
        //                            alternate = !alternate;

        //                            string cuentaOrigen = "";

        //                            if (d.CuentaBancaria != null)
        //                            {
        //                                cuentaOrigen = d.CuentaBancaria.NumeroCuenta;
        //                            }


        //                            // Primera celda (# de cuota)
        //                            table.Cell()
        //                                //.Background(Colors.Blue.Lighten5)
        //                                //.Border(0.25f)
        //                                //.BorderColor(Colors.Grey.Lighten4)
        //                                .Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4)
        //                                .Padding(4)
        //                                .AlignCenter()
        //                                .Text(d.NumeroCuota.ToString());
        //                            //.FontColor(Colors.Blue.Darken3)
        //                            //.Bold();

        //                            // Resto de las columnas
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{d.TotalAPagar:N2}");
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{cuentaOrigen}");
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.CreatedDate.ToString("dd-MM-yyyy"));
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoInteres:N2}");
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoCapital:N2}");
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaPago.ToString("dd-MM-yyyy"));
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.RestaCapital:N2}");
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaProximoPago.ToString("dd-MM-yyyy"));
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.ProximoPago:N2}");
        //                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.Observacion}");
        //                        }
        //                    });




        //                    col.Item().Height(20);
        //                    col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


        //                    col.Item().Height(1);
        //                    // ======= TEXTO DE NOTA =======
        //                    col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
        //                        .Italic()                 // Texto en cursiva
        //                        .AlignCenter()
        //                        .FontSize(8);             // Más pequeño que el texto normal
        //                                                  //.FontColor(Colors.Grey.Darken2); // Color gris suave
        //                });

        //                // Pie reutilizable
        //                FooterPIM(page);
        //            });
        //        });

        //        var pdfBytes = pdf.GeneratePdf();

        //        return AppResult.New(
        //            true,
        //            pdfBytes,
        //            $"Estado-{prestamo.CodigoPrestamo}-{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error al generar el reporte de estado del préstamo: {ex.Message}");
        //    }
        //}




        public async Task<AppResult> GenerarReporteActivosPIM(Guid? empresaId, Guid? responsableId)
        {
            try
            {
                //var prestamos = await _context.Prestamo.Where(x =>  !x.IsSoftDeleted && x.Estado == Domain.Enum.EstadoPrestamo.Vigente)
                //    .Include(x => x.Cliente)
                //    .Include(x => x.PrestamoDetalles)
                //    .AsNoTracking()
                //    .ToListAsync();

                var prestamosConsultas =  _context.Prestamo.Where(x => !x.IsSoftDeleted && x.Estado == Domain.Enum.EstadoPrestamo.Vigente)
                    .Include(x => x.Cliente)
                    .Include(x => x.PrestamoDetalles)
                    .AsNoTracking();

                var nombreResponsable = "";
                if(responsableId != null && responsableId != Guid.Empty)
                {
                    var responsable = await _context.Persona.Where(x => x.Id == responsableId).AsNoTracking().FirstOrDefaultAsync();
                    responsable.ThrowIfNull("No existe Responsable");
                    nombreResponsable = $"{responsable.Nombre} {responsable.Apellido}";


                    prestamosConsultas = prestamosConsultas.Where(x => x.Responsable == responsableId);
                }

                var prestamos = await prestamosConsultas.ToListAsync();

                var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                var fechaActual = DateTime.Now;
                var fechaActualString = $"{fechaActual:dd-MM-yyyy hh:mm tt}";






                // ===== CALCULAR TOTALES =====
                decimal totalCapital = 0;
                decimal totalCapitalActivo = 0;
                decimal totalMontoPagado = 0;
                decimal totalEstimadoAPagar = 0;
                decimal totalInteresGenerado = 0;
                decimal porcentajesInteresSuma = 0;
                decimal deudaTotalAll = 0;


                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                var textoQR = $"Reporte de PIM Activos " + fechaActualString;

                var qrBytes = await _iQrServices.GenerarQR(textoQR);

                // === Crear PDF ===
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());        //Landscape() => Horizontal
                        page.Margin(20);
                        page.PageColor(Colors.White);
                        //page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Times New Roman"));
                        page.DefaultTextStyle(x => x.FontSize(8));  //La letra es Arial
                        // Encabezado reutilizable
                        EncabezadoPIM(page, logoBytes, qrBytes);

                        // === CONTENIDO ===
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            // ======= TÍTULO PRINCIPAL =======
                            col.Item().Text($"Reporte Prestamo de Interes Mensual (PIM) Vigentes")
                                .Bold().FontSize(12)
                                .AlignCenter();
                            //.FontColor(Colors.Blue.Darken3);
                            col.Item().Text($"Fecha: {fechaActualString}")
                                //.Bold()
                                .FontSize(11)
                                .AlignCenter();

                            if(nombreResponsable.Length > 0)
                            {
                                col.Item().Text($"Responsable: {nombreResponsable}")
                                    //.Bold()
                                    .FontSize(11)
                                    .AlignCenter();
                            }


                            col.Item().Height(20);



                            // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                            col.Item().Table(table =>
                            {
                                // ==== Definición de columnas ====
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(0.35f); // #
                                    columns.RelativeColumn(2.2f);  // Nombre Cliente
                                    columns.RelativeColumn(1.3f);  // Documento

                                    columns.RelativeColumn(1.1f);  // Capital
                                    columns.RelativeColumn(1.2f);  // Capital Activo
                                    columns.RelativeColumn(0.7f);  // Interes
                                    columns.RelativeColumn(1.2f);  // Monto Pagado
                                    columns.RelativeColumn(1.0f);  // Prox Pago
                                    columns.RelativeColumn(1.2f);  // Interes Generado
                                    //columns.RelativeColumn(0.9f);  // Mora

                                    columns.RelativeColumn(1.2f);  // Deuda Total
                                    columns.RelativeColumn(1.0f);  // Entregado
                                    columns.RelativeColumn(1.0f);  // Ultimo Pago
                                    columns.RelativeColumn(0.5f);  // Hace cuantos dias
                                });

                                // ==== Encabezado con borde total ====
                                string[] headers = { 
                                    "#", 
                                    "Nombre Cliente",
                                    "Prestamo", 
                                    "Capital", 
                                    "Capital Activo",
                                    "Interes",
                                    "Monto Pagado", 
                                    "Prox Pago",
                                    "Interes Generado",
                                    //"Mora",
                                    //"Cuotas",
                                    "Deuda Total",
                                    "Entregado",
                                    "Ultimo Pago",
                                    "Dias"
                                };

                                foreach (var header in headers)
                                {
                                    table.Cell()
                                        .Background(Colors.Blue.Lighten5)   // azul más suave
                                        .Border(0.2f)                       // borde completo por celda
                                        .BorderColor(Colors.Grey.Lighten1)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(3)
                                        .AlignCenter()
                                        .Text(header)
                                        .Bold()
                                        .FontSize(8)
                                        .FontColor(Colors.Black);           // texto negro
                                }

                                // ==== Filas de datos ====
                                bool alternate = false;

                                for(int i=0; i<prestamos.Count(); i++)
                                {
                                    var prestamo = prestamos.ElementAt(i);

                                    var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                    alternate = !alternate;

                                    var item = i + 1;
                                    var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);

                                    var mora = calculo.InteresGenerado - calculo.ProximoPago;

                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{prestamo.ClienteNombre}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.CantidadInicial:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.RestaCapital:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter  ().Text($"{prestamo.InteresPorcentaje}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.MontoPagado:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.EstimadoAPagarMes:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{calculo.InteresGenerado:N2}");
                                    //table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{mora:N2}").FontColor(mora == 0 ? Colors.Red.Medium : Colors.Grey.Darken4);
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{calculo.DeudaTotal:N2}");
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaUltimoPago == DateTime.MinValue ? string.Empty : prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
                                    table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{calculo.DiasGenerados}");


                                    #region Calcular totales
                                    totalCapital += prestamo.CantidadInicial;
                                    totalCapitalActivo += prestamo.RestaCapital;
                                    totalMontoPagado += prestamo.MontoPagado;
                                    totalEstimadoAPagar += prestamo.EstimadoAPagarMes;
                                    totalInteresGenerado += calculo.InteresGenerado;
                                    porcentajesInteresSuma += prestamo.InteresPorcentaje;
                                    deudaTotalAll += calculo.DeudaTotal;
                                    #endregion
                                }




                                // ===== FILA DE TOTALES =====
                                var bgTotal = Colors.Grey.Lighten3;

                                // Ponemos "TOTAL" en la primera celda y dejamos las dos siguientes vacías
                                table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");
                                table.Cell().Background(bgTotal).Padding(4).Text("");// segunda columna
                                table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();  // tercera columna

                                // Columnas de números
                                table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();
                                table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapitalActivo:N2}").Bold();
                                var porcetajeIntereses = porcentajesInteresSuma / prestamos.Count();
                                table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}"); // Interes %
                                table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalMontoPagado:N2}").Bold();
                                table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalEstimadoAPagar:N2}").Bold();
                                table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalInteresGenerado:N2}").Bold();

                                // Columnas vacías
                                table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{deudaTotalAll:N2}").Bold(); // Deuda Total
                                table.Cell().Background(bgTotal).Padding(4).Text(""); // Entregado
                                table.Cell().Background(bgTotal).Padding(4).Text(""); // Ultimo Pago
                                table.Cell().Background(bgTotal).Padding(4).Text(""); // Dias

                            });




                            col.Item().Height(20);
                            col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


                            col.Item().Height(1);
                            // ======= TEXTO DE NOTA =======
                            col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
                                .Italic()                 // Texto en cursiva
                                .AlignCenter()
                                .FontSize(8);             // Más pequeño que el texto normal
                                                          //.FontColor(Colors.Grey.Darken2); // Color gris suave
                        });

                        // Pie reutilizable
                        FooterPIM(page);
                    });
                });

                var pdfBytes = pdf.GeneratePdf();

                return AppResult.New(
                    true,
                    pdfBytes,
                    $"ReportePIM-{fechaActualString}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el reporte: {ex.Message}");
            }
        }





        public async Task<AppResult> GenerarReportePIM(Guid? empresaId, Guid? responsableId, EstadoPrestamo? estado, bool todosLosEstados)
        {
            try
            {

                var prestamosConsultas = _context.Prestamo.Where(x => !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.PrestamoDetalles)
                    .AsNoTracking();

                var nombreResponsable = "";
                if (responsableId != null && responsableId != Guid.Empty)
                {
                    var responsable = await _context.Persona.Where(x => x.Id == responsableId).AsNoTracking().FirstOrDefaultAsync();
                    responsable.ThrowIfNull("No existe Responsable");
                    nombreResponsable = $"{responsable.Nombre} {responsable.Apellido}";


                    prestamosConsultas = prestamosConsultas.Where(x => x.Responsable == responsableId);
                }

                if(todosLosEstados)
                {
                    prestamosConsultas = prestamosConsultas.Where(x => x.Estado != EstadoPrestamo.Cancelado);
                }
                else
                {
                    prestamosConsultas = prestamosConsultas.Where(x => x.Estado == estado);
                }

                var prestamos = await prestamosConsultas.ToListAsync();

                var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                var fechaActual = DateTime.Now;
                var fechaActualString = $"{fechaActual:dd-MM-yyyy hh:mm tt}";




                var responsablesIds = prestamos.Where(x => x.Responsable != Guid.Empty && x.Responsable != Guid.Empty).Select(x => x.Responsable).Distinct().ToList();
                var responsables = await _context.Persona.Where(x => responsablesIds.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();




                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                var textoQR = $"Reporte PIM" + fechaActualString;

                var qrBytes = await _iQrServices.GenerarQR(textoQR);

                // === Crear PDF ===
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());        //Landscape() => Horizontal
                        page.Margin(20);
                        page.PageColor(Colors.White);
                        //page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Times New Roman"));
                        page.DefaultTextStyle(x => x.FontSize(8));  //La letra es Arial
                        // Encabezado reutilizable
                        EncabezadoPIM(page, logoBytes, qrBytes);

                        // === CONTENIDO ===
                        page.Content().PaddingVertical(10).Column(async col =>
                        {
                            // ======= TÍTULO PRINCIPAL =======
                            col.Item().Text($"Reporte Prestamo de Interes Mensual (PIM)")
                                .Bold().FontSize(12)
                                .AlignCenter();
                            //.FontColor(Colors.Blue.Darken3);


                            if (nombreResponsable.Length > 0)
                            {
                                col.Item().Text($"Responsable: {nombreResponsable} | Fecha: {fechaActualString}")
                                    //.Bold()
                                    .FontSize(9)
                                    .AlignCenter();
                            }
                            else
                            {
                                col.Item().Text($"Fecha: {fechaActualString}")
                                    //.Bold()
                                    .FontSize(9)
                                    .AlignCenter();
                            }


                            //col.Item().Height(20);

                            var prestamosVigentes = prestamos.Where(x => x.Estado == EstadoPrestamo.Vigente).ToList();
                            var prestamosPagados = prestamos.Where(x => x.Estado == EstadoPrestamo.Pagado).ToList();
                            var prestamosPendientesAprobacion = prestamos.Where(x => x.Estado == EstadoPrestamo.PendienteAprobacion).ToList();
                            var prestamosRechazados = prestamos.Where(x => x.Estado == EstadoPrestamo.Rechazado).ToList();


                            #region Prestamos Vigentes

                            if (prestamosVigentes.Any())
                            {
                                col.Item().Height(17);

                                col.Item().Text($"Vigentes")
                                    //.Bold()
                                    .FontSize(10)
                                    .AlignCenter();

                                col.Item().Height(4);



                                // ===== CALCULAR TOTALES =====
                                decimal totalCapital = 0;
                                decimal totalCapitalActivo = 0;
                                decimal totalMontoPagado = 0;
                                decimal totalEstimadoAPagar = 0;
                                decimal totalInteresGenerado = 0;
                                decimal porcentajesInteresSuma = 0;
                                decimal deudaTotalAll = 0;


                                // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                                col.Item().Table(table =>
                                {
                                    // ==== Definición de columnas ====
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(0.35f); // #
                                        columns.RelativeColumn(2.2f);  // Nombre Cliente
                                        columns.RelativeColumn(1.3f);  // Documento

                                        columns.RelativeColumn(1.1f);  // Capital
                                        columns.RelativeColumn(1.2f);  // Capital Activo
                                        columns.RelativeColumn(0.7f);  // Interes
                                        columns.RelativeColumn(1.2f);  // Monto Pagado
                                        columns.RelativeColumn(1.0f);  // Prox Pago
                                        columns.RelativeColumn(1.2f);  // Interes Generado
                                                                       //columns.RelativeColumn(0.9f);  // Mora

                                        columns.RelativeColumn(1.2f);  // Deuda Total
                                        columns.RelativeColumn(1.0f);  // Entregado
                                        columns.RelativeColumn(1.0f);  // Ultimo Pago
                                        columns.RelativeColumn(0.5f);  // Hace cuantos dias
                                    });

                                    // ==== Encabezado con borde total ====
                                    string[] headers = {
                                    "#",
                                    "Nombre Cliente",
                                    "Prestamo",
                                    "Capital",
                                    "Capital Activo",
                                    "Interes",
                                    "Monto Pagado",
                                    "Prox Pago",
                                    "Interes Generado",
                                    //"Mora",
                                    //"Cuotas",
                                    "Deuda Total",
                                    "Entregado",
                                    "Ultimo Pago",
                                    "Dias"
                                };

                                    foreach (var header in headers)
                                    {
                                        table.Cell()
                                            .Background(Colors.Blue.Lighten5)   // azul más suave
                                            .Border(0.2f)                       // borde completo por celda
                                            .BorderColor(Colors.Grey.Lighten1)
                                            .PaddingVertical(4)
                                            .PaddingHorizontal(3)
                                            .AlignCenter()
                                            .Text(header)
                                            .Bold()
                                            .FontSize(8)
                                            .FontColor(Colors.Black);           // texto negro
                                    }

                                    // ==== Filas de datos ====
                                    bool alternate = false;

                                    for (int i = 0; i < prestamosVigentes.Count(); i++)
                                    {
                                        var prestamo = prestamosVigentes.ElementAt(i);

                                        var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                        alternate = !alternate;

                                        var item = i + 1;
                                        var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);

                                        var mora = calculo.InteresGenerado - calculo.ProximoPago;

                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{prestamo.ClienteNombre ?? string.Empty}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo ?? string.Empty}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.CantidadInicial:N2}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.RestaCapital:N2}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.InteresPorcentaje}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.MontoPagado:N2}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.EstimadoAPagarMes:N2}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{calculo.InteresGenerado:N2}");
                                        //table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{mora:N2}").FontColor(mora == 0 ? Colors.Red.Medium : Colors.Grey.Darken4);
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{calculo.DeudaTotal:N2}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaUltimoPago == DateTime.MinValue ? string.Empty : prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{calculo.DiasGenerados}");


                                        #region Calcular totales
                                        totalCapital += prestamo.CantidadInicial;
                                        totalCapitalActivo += prestamo.RestaCapital;
                                        totalMontoPagado += prestamo.MontoPagado;
                                        totalEstimadoAPagar += prestamo.EstimadoAPagarMes;
                                        totalInteresGenerado += calculo.InteresGenerado;
                                        porcentajesInteresSuma += prestamo.InteresPorcentaje;
                                        deudaTotalAll += calculo.DeudaTotal;
                                        #endregion
                                    }




                                    // ===== FILA DE TOTALES =====
                                    var bgTotal = Colors.Grey.Lighten3;

                                    // Ponemos "TOTAL" en la primera celda y dejamos las dos siguientes vacías
                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");
                                    table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();// segunda columna
                                    table.Cell().Background(bgTotal).Padding(4).Text("").Bold();  // tercera columna

                                    // Columnas de números
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapitalActivo:N2}").Bold();
                                    var porcetajeIntereses = porcentajesInteresSuma / prestamosVigentes.Count();
                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}"); // Interes %
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalMontoPagado:N2}").Bold();
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalEstimadoAPagar:N2}").Bold();
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalInteresGenerado:N2}").Bold();

                                    // Columnas vacías
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{deudaTotalAll:N2}").Bold(); // Deuda Total
                                    table.Cell().Background(bgTotal).Padding(4).Text(""); // Entregado
                                    table.Cell().Background(bgTotal).Padding(4).Text(""); // Ultimo Pago
                                    table.Cell().Background(bgTotal).Padding(4).Text(""); // Dias

                                });


                            }
                            
                            #endregion



                            #region Prestamos Pagados

                            if (prestamosPagados.Any())
                            {
                                col.Item().Height(17);


                                col.Item().Text($"Pagados")
                                    //.Bold()
                                    .FontSize(10)
                                    .AlignCenter();

                                col.Item().Height(4);


                                //No puedo usar Async dentro al momento de craer pdf, no acepta Async
                                //var responsablesIds = prestamosPagados.Where(x => x.Responsable != Guid.Empty && x.Responsable != Guid.Empty).Select(x => x.Responsable).Distinct().ToList();
                                //var responsables = await _context.Persona.Where(x => responsablesIds.Contains(x.Id) && !x.IsSoftDeleted).AsNoTracking().ToListAsync();


                                // ===== CALCULAR TOTALES =====
                                decimal totalCapital = 0;
                                decimal porcentajesInteresSuma = 0;
                                int cuotasTotal = 0;
                                decimal totalMontoPagado = 0;
                                decimal gananciasTotal = 0;


                                // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                                col.Item().Table(table =>
                                {
                                    // ==== Definición de columnas ====
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(0.35f); // #
                                        columns.RelativeColumn(2.2f);  // Nombre Cliente
                                        columns.RelativeColumn(1.4f);  // Documento

                                        columns.RelativeColumn(1.1f);  // Cantidad
                                        columns.RelativeColumn(1.2f);  // Entregado
                                        columns.RelativeColumn(0.7f);  // Interes
                                        columns.RelativeColumn(0.7f);  // Cuotas
                                        columns.RelativeColumn(1.2f);  // Ultimo Pago
                                        columns.RelativeColumn(1.2f);  // Monto Pagado
                                        columns.RelativeColumn(1.2f);  // Ganancia

                                        columns.RelativeColumn(2.8f);  // Observaciones
                                        columns.RelativeColumn(2.0f);  // Responsable
                                    });

                                    // ==== Encabezado con borde total ====
                                    string[] headers = {
                                        "#",
                                        "Nombre Cliente",
                                        "Prestamo",
                                        "Cantidad",
                                        "Entregado",
                                        "Interes",
                                        "Cuotas",
                                        "Ultimo Pago",
                                        "Monto Pagado",
                                        "Ganancia",
                                        "Observaciones",
                                        "Responsable"
                                    };

                                    foreach (var header in headers)
                                    {
                                        table.Cell()
                                            .Background(Colors.Orange.Lighten5)   // Naranja
                                            .Border(0.2f)                       // borde completo por celda
                                            .BorderColor(Colors.Grey.Lighten1)
                                            .PaddingVertical(4)
                                            .PaddingHorizontal(3)
                                            .AlignCenter()
                                            .Text(header)
                                            .Bold()
                                            .FontSize(8)
                                            .FontColor(Colors.Black);           // texto negro
                                    }

                                    // ==== Filas de datos ====
                                    bool alternate = false;

                                    for (int i = 0; i < prestamosPagados.Count(); i++)
                                    {
                                        var prestamo = prestamosPagados.ElementAt(i);

                                        var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                        alternate = !alternate;

                                        var item = i + 1;
                                   
                                        var nombreResponsable = "";
                                        var resposable = responsables.Where(x => x.Id == prestamo.Responsable).FirstOrDefault();
                                        if(resposable != null)
                                        {
                                            nombreResponsable = resposable.Nombre + " " + resposable.Apellido;
                                        }

                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{prestamo.ClienteNombre ?? string.Empty}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo ?? string.Empty}");
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.CantidadInicial.ToString("N2") ?? "0.00"); //Cantidad
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaEntragado.ToString("dd-MM-yyyy"));   //Entregado
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.InteresPorcentaje.ToString("N2") ?? "0.00"); //Interes
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.CuotasPagadas.ToString() ?? "0.00");     //Cuotas
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaUltimoPago == DateTime.MinValue ? string.Empty : prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));   //UltimoPago
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.MontoPagado.ToString("N2") ?? "0.00");  //Monto Pagado
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.Ganancia?.ToString("N2") ?? "0.00");       //Ganancia
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.Observacion ?? string.Empty}"); //Observaciones
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{nombreResponsable ?? string.Empty}");      //Responsable


                                        #region Calcular totales
                                        totalCapital += prestamo.CantidadInicial;
                                        porcentajesInteresSuma += prestamo.InteresPorcentaje;
                                        cuotasTotal += prestamo.CuotasPagadas;
                                        totalMontoPagado += prestamo.MontoPagado;
                                        gananciasTotal += (decimal)prestamo.Ganancia;

                                        #endregion
                                    }


                                    var porcetajeIntereses = porcentajesInteresSuma / prestamosPagados.Count();

                                    // ===== FILA DE TOTALES =====
                                    var bgTotal = Colors.Grey.Lighten3;

                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");                                     //Item
                                    table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();                                       //ClienteNombre
                                    table.Cell().Background(bgTotal).Padding(4).Text("").Bold();                                            //CodigoPrestamo
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();             //CantidadInicial
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text("").Bold();                               //FechaEntragado
                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}");             //InteresPorcentaje       
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{cuotasTotal}").Bold();                 //CuotasPagadas
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"").Bold();                              //FechaUltimoPago
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalMontoPagado:N2}").Bold();         //MontoPagado
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{gananciasTotal:N2}").Bold();           //Ganancia
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Observacion
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Responsable

                                });


                            }

                            #endregion





                            #region Prestamos Pendientes de Aprobacion

                            if (prestamosPendientesAprobacion.Any())
                            {
                                col.Item().Height(17);


                                col.Item().Text($"Pendientes de Aprobarción")
                                    //.Bold()
                                    .FontSize(10)
                                    .AlignCenter();

                                col.Item().Height(4);

                                // ===== CALCULAR TOTALES =====
                                decimal totalCapital = 0;
                                decimal porcentajesInteresSuma = 0;
                                int cuotasTotal = 0;
                                decimal totalMontoPagado = 0;
                                decimal gananciasTotal = 0;


                                // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                                col.Item().Table(table =>
                                {
                                    // ==== Definición de columnas ====
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(0.35f); // #
                                        columns.RelativeColumn(2.2f);  // Nombre Cliente
                                        columns.RelativeColumn(1.3f);  // Documento
                                        columns.RelativeColumn(1.1f);  // Cantidad
                                        columns.RelativeColumn(0.7f);  // Interes
                                        columns.RelativeColumn(1.6f);  // Estado
                                        columns.RelativeColumn(1.5f);  // Entregado
                                        columns.RelativeColumn(2.8f);  // Observaciones
                                        columns.RelativeColumn(2.0f);  // Responsable
                                    });

                                    // ==== Encabezado con borde total ====
                                    string[] headers = {
                                        "#",
                                        "Nombre Cliente",
                                        "Prestamo",
                                        "Cantidad",
                                        "Interes",
                                        "Estado",
                                        "Entregado",
                                        "Observaciones",
                                        "Responsable"
                                    };

                                    foreach (var header in headers)
                                    {
                                        table.Cell()
                                            .Background(Colors.Purple.Lighten5)   // Purple suave = Pendiente
                                            .Border(0.2f)                       // borde completo por celda
                                            .BorderColor(Colors.Grey.Lighten1)
                                            .PaddingVertical(4)
                                            .PaddingHorizontal(3)
                                            .AlignCenter()
                                            .Text(header)
                                            .Bold()
                                            .FontSize(8)
                                            .FontColor(Colors.Black);           // texto negro
                                    }

                                    // ==== Filas de datos ====
                                    bool alternate = false;

                                    for (int i = 0; i < prestamosPendientesAprobacion.Count(); i++)
                                    {
                                        var prestamo = prestamosPendientesAprobacion.ElementAt(i);

                                        var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                        alternate = !alternate;

                                        var item = i + 1;

                                        var nombreResponsable = "";
                                        var resposable = responsables.Where(x => x.Id == prestamo.Responsable).FirstOrDefault();
                                        if (resposable != null)
                                        {
                                            nombreResponsable = resposable.Nombre + " " + resposable.Apellido;
                                        }

                                        var fechaEntrega = "";
                                        if (prestamo.FechaEntragado == null || prestamo.FechaEntragado == DateTime.MinValue)
                                        {
                                            fechaEntrega = "Sin fecha de entrega";
                                        }
                                        else
                                        {
                                            fechaEntrega = prestamo.FechaEntragado.ToString("dd-MM-yyyy");
                                        }

                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");                                                 //Item
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{prestamo.ClienteNombre ?? string.Empty}");               //Cliente
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo ?? string.Empty}");            //Codigo
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.CantidadInicial.ToString("N2") ?? "0.00");        //Cantidad
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.InteresPorcentaje.ToString("N2") ?? "0.00");     //Interes
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.Estado_Descripcion ?? string.Empty}");        //Estado_Descripcion
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(fechaEntrega);                                            //Entregado
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.Observacion ?? string.Empty}");               //Observaciones
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{nombreResponsable ?? string.Empty}");                  //Responsable


                                        #region Calcular totales
                                        totalCapital += prestamo.CantidadInicial;
                                        porcentajesInteresSuma += prestamo.InteresPorcentaje;
                                        cuotasTotal += prestamo.CuotasPagadas;
                                        totalMontoPagado += prestamo.MontoPagado;
                                        gananciasTotal += (decimal)prestamo.Ganancia;

                                        #endregion
                                    }


                                    var porcetajeIntereses = porcentajesInteresSuma / prestamosPendientesAprobacion.Count();

                                    // ===== FILA DE TOTALES =====
                                    var bgTotal = Colors.Grey.Lighten3;

                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");                                     //Item
                                    table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();                                       //ClienteNombre
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //CodigoPrestamo
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();             //CantidadInicial
                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}");             //InteresPorcentaje   
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Estado
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Entregado
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Observacion
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Responsable

                                });


                            }

                            #endregion





                            #region Prestamos Rechazados

                            if (prestamosRechazados.Any())
                            {
                                col.Item().Height(17);


                                col.Item().Text($"Rechazados")
                                    //.Bold()
                                    .FontSize(10)
                                    .AlignCenter();

                                col.Item().Height(4);

                                // ===== CALCULAR TOTALES =====
                                decimal totalCapital = 0;
                                decimal porcentajesInteresSuma = 0;
                                int cuotasTotal = 0;
                                decimal totalMontoPagado = 0;
                                decimal gananciasTotal = 0;


                                // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                                col.Item().Table(table =>
                                {
                                    // ==== Definición de columnas ====
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(0.35f); // #
                                        columns.RelativeColumn(2.2f);  // Nombre Cliente
                                        columns.RelativeColumn(1.3f);  // Documento
                                        columns.RelativeColumn(1.1f);  // Cantidad
                                        columns.RelativeColumn(0.7f);  // Interes
                                        columns.RelativeColumn(1.6f);  // Estado
                                        columns.RelativeColumn(1.5f);  // Entregado
                                        columns.RelativeColumn(2.8f);  // Observaciones
                                        columns.RelativeColumn(2.0f);  // Responsable
                                    });

                                    // ==== Encabezado con borde total ====
                                    string[] headers = {
                                        "#",
                                        "Nombre Cliente",
                                        "Prestamo",
                                        "Cantidad",
                                        "Interes",
                                        "Estado",
                                        "Entregado",
                                        "Observaciones",
                                        "Responsable"
                                    };

                                    foreach (var header in headers)
                                    {
                                        table.Cell()
                                            .Background(Colors.Red.Lighten5)   // Rojo suave = Rechazado
                                            .Border(0.2f)                       // borde completo por celda
                                            .BorderColor(Colors.Grey.Lighten1)
                                            .PaddingVertical(4)
                                            .PaddingHorizontal(3)
                                            .AlignCenter()
                                            .Text(header)
                                            .Bold()
                                            .FontSize(8)
                                            .FontColor(Colors.Black);           // texto negro
                                    }

                                    // ==== Filas de datos ====
                                    bool alternate = false;

                                    for (int i = 0; i < prestamosRechazados.Count(); i++)
                                    {
                                        var prestamo = prestamosRechazados.ElementAt(i);

                                        var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                                        alternate = !alternate;

                                        var item = i + 1;

                                        var nombreResponsable = "";
                                        var resposable = responsables.Where(x => x.Id == prestamo.Responsable).FirstOrDefault();
                                        if (resposable != null)
                                        {
                                            nombreResponsable = resposable.Nombre + " " + resposable.Apellido;
                                        }

                                        var fechaEntrega = "";
                                        if (prestamo.FechaEntragado == null || prestamo.FechaEntragado == DateTime.MinValue)
                                        {
                                            fechaEntrega = "Sin fecha de entrega";
                                        }
                                        else
                                        {
                                            fechaEntrega = prestamo.FechaEntragado.ToString("dd-MM-yyyy");
                                        }

                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");                                                 //Item
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{prestamo.ClienteNombre ?? string.Empty}");               //Cliente
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo ?? string.Empty}");            //Codigo
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.CantidadInicial.ToString("N2") ?? "0.00");        //Cantidad
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.InteresPorcentaje.ToString("N2") ?? "0.00");     //Interes
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.Estado_Descripcion ?? string.Empty}");        //Estado_Descripcion
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(fechaEntrega);                                            //Entregado
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.Observacion ?? string.Empty}");               //Observaciones
                                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{nombreResponsable ?? string.Empty}");                  //Responsable


                                        #region Calcular totales
                                        totalCapital += prestamo.CantidadInicial;
                                        porcentajesInteresSuma += prestamo.InteresPorcentaje;
                                        cuotasTotal += prestamo.CuotasPagadas;
                                        totalMontoPagado += prestamo.MontoPagado;
                                        gananciasTotal += (decimal)prestamo.Ganancia;

                                        #endregion
                                    }


                                    var porcetajeIntereses = porcentajesInteresSuma / prestamosRechazados.Count();

                                    // ===== FILA DE TOTALES =====
                                    var bgTotal = Colors.Grey.Lighten3;

                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");                                     //Item
                                    table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();                                       //ClienteNombre
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //CodigoPrestamo
                                    table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();             //CantidadInicial
                                    table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}");             //InteresPorcentaje   
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Estado
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Entregado
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Observacion
                                    table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Responsable

                                });


                            }

                            #endregion






                            col.Item().Height(20);
                            col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


                            col.Item().Height(1);
                            // ======= TEXTO DE NOTA =======
                            col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
                                .Italic()                 // Texto en cursiva
                                .AlignCenter()
                                .FontSize(8);             // Más pequeño que el texto normal
                                                          //.FontColor(Colors.Grey.Darken2); // Color gris suave
                        });

                        // Pie reutilizable
                        FooterPIM(page);
                    });
                });

                var pdfBytes = pdf.GeneratePdf();

                return AppResult.New(
                    true,
                    pdfBytes,
                    $"ReportePIM-{fechaActualString}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el reporte: {ex.Message}");
            }
        }





        public async Task<AppResult> GenerarReportePIMCliente(Guid clienteId, EstadoPrestamo? estado, bool todosLosEstados, bool detalles)
        {
            try
            {

                var prestamosConsultas = _context.Prestamo.Where(x => x.ClienteId == clienteId && !x.IsSoftDeleted)
                    .Include(x => x.Cliente)
                    .Include(x => x.PrestamoDetalles)
                    .AsNoTracking();

                if (todosLosEstados)
                {
                    prestamosConsultas = prestamosConsultas.Where(x => x.Estado != EstadoPrestamo.Cancelado);
                }
                else
                {
                    prestamosConsultas = prestamosConsultas.Where(x => x.Estado == estado);
                }

                var prestamos = await prestamosConsultas.ToListAsync();

                var configuracion = await _context.ConfiguracionPrestamo.Where(x => !x.IsSoftDeleted).AsNoTracking().FirstOrDefaultAsync();
                var fechaActual = DateTime.Now;
                var fechaActualString = $"{fechaActual:dd-MM-yyyy hh:mm tt}";

                var prestamosCalculo = new List<InteresDiario_CalcularInteresAndCapitalVm>();

                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                var textoQR = $"Reporte PIM" + fechaActualString;

                var qrBytes = await _iQrServices.GenerarQR(textoQR);

                // === Crear PDF ===
                var pdf = Document.Create(container =>
                {


                    // ===== PÁGINA 1: REPORTE GENERAL =====
                    container.Page(page =>
                    {
                        RenderReporteResumenPIM(
                            page,
                            prestamos,
                            configuracion,
                            fechaActual,
                            logoBytes,
                            qrBytes
                        );
                    });

                    // ===== PÁGINAS 2..N: ESTADO DE CADA PRÉSTAMO =====
                    if (detalles)
                    {
                        prestamos = prestamos.OrderBy(x => x.Estado).ToList();

                        foreach (var prestamo in prestamos)
                        {
                            var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);
                            if (calculo != null)
                            {
                                container.Page(page =>
                                {
                                    RenderEstadoPrestamoPaginaIndpendiente(
                                        page,
                                        prestamo,
                                        calculo,
                                        logoBytes,
                                        qrBytes,
                                        fechaActual
                                    );
                                });
                            }
                        }
                    }


                    // Pie reutilizable
                    //FooterPIM(page);
                    
                });

                var pdfBytes = pdf.GeneratePdf();

                return AppResult.New(
                    true,
                    pdfBytes,
                    $"ReportePIM-{fechaActualString}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el reporte: {ex.Message}");
            }
        }







        private void RenderReporteResumenPIM(PageDescriptor page,List<Prestamo> prestamos, ConfiguracionPrestamo configuracion,  DateTime fechaActual, byte[] logoBytes, byte[] qrBytes)
        {

            var prestamoBase = prestamos.FirstOrDefault();

            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(8));

            EncabezadoPIM(page, logoBytes, qrBytes);

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Item().Text("REPORTE GENERAL DE PRÉSTAMOS")
                    .Bold()
                    .FontSize(12)
                    .AlignCenter();

                col.Item().Height(20);





                // ======= SECCIÓN: INFORMACIÓN DEL CLIENTE =======
                col.Item().Column(c =>
                {
                    c.Item().Text("INFORMACIÓN DEL CLIENTE")
                        .Bold().FontSize(9);
                    //.FontColor(Colors.Blue.Darken3);

                    c.Item().Height(2);

                    // Fondo gris claro y diseño limpio
                    c.Item().Element(e =>
                    {
                        e.Background(Colors.Grey.Lighten4)
                         .CornerRadius(2)
                         .Padding(2)
                         .PaddingHorizontal(7)
                         .Column(colCliente =>
                         {
                             // ====== Fila 1: Nombre (ocupa 2 columnas) + Identificación + Teléfono ======
                             colCliente.Item().Row(row =>
                             {
                                 // Nombre (2 columnas)
                                 row.RelativeColumn(2).Column(cc =>
                                 {
                                     cc.Item().Text("Nombre").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text($"{prestamoBase.Cliente.Nombre} {prestamoBase.Cliente.Apellido}")
                                         .FontSize(8);
                                 });

                                 // Identificación
                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Codigo Cliente").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamoBase.Cliente.CodigoPersona ?? "—").FontSize(8);
                                 });

                                 // Teléfono
                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Teléfono").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamoBase.Cliente.Telefono ?? "—").FontSize(8);
                                 });
                             });

                             colCliente.Item().Height(5);

                             // ====== Fila 2: Correo + Lugar de trabajo + Ciudad + País ======
                             colCliente.Item().Row(row =>
                             {
                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Correo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamoBase.Cliente.Correo ?? "—").FontSize(8);
                                 });

                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Ciudad").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamoBase.Cliente.Ciudad ?? "—").FontSize(8);
                                 });

                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("País").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamoBase.Cliente.Pais ?? "—").FontSize(8);
                                 });

                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Lugar de trabajo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamoBase.Cliente.LugarTrabajo ?? "—").FontSize(8);
                                 });
                             });

                             colCliente.Item().Height(5);

                             // ====== Fila 3: Dirección completa (ocupa las 4 columnas) ======
                             //colCliente.Item().Row(row =>
                             //{
                             //    row.RelativeColumn(4).Column(cc =>
                             //    {
                             //        cc.Item().Text("Dirección").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                             //        cc.Item().Text($"{prestamo.Cliente.Direccion ?? "—"}")
                             //            .FontSize(8);
                             //    });
                             //});
                         });
                    });
                });





                col.Item().Height(12);




                //col.Item().Height(20);

                var prestamosVigentes = prestamos.Where(x => x.Estado == EstadoPrestamo.Vigente).ToList();
                var prestamosPagados = prestamos.Where(x => x.Estado == EstadoPrestamo.Pagado).ToList();


                #region Prestamos Vigentes

                if (prestamosVigentes.Any())
                {
                    col.Item().Height(15);

                    col.Item().Text($"Vigentes")
                        //.Bold()
                        .FontSize(10)
                        .AlignCenter();

                    col.Item().Height(5);



                    // ===== CALCULAR TOTALES =====
                    decimal totalCapital = 0;
                    decimal totalCapitalActivo = 0;
                    decimal totalMontoPagado = 0;
                    decimal totalEstimadoAPagar = 0;
                    decimal totalInteresGenerado = 0;
                    decimal porcentajesInteresSuma = 0;
                    decimal deudaTotalAll = 0;


                    // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                    col.Item().Table(table =>
                    {
                        // ==== Definición de columnas ====
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.35f); // #
                            columns.RelativeColumn(1.3f);  // Documento

                            columns.RelativeColumn(1.1f);  // Capital
                            columns.RelativeColumn(1.2f);  // Capital Activo
                            columns.RelativeColumn(0.7f);  // Interes
                            columns.RelativeColumn(1.2f);  // Monto Pagado
                            columns.RelativeColumn(1.0f);  // Prox Pago
                            columns.RelativeColumn(1.2f);  // Interes Generado
                                                           //columns.RelativeColumn(0.9f);  // Mora

                            columns.RelativeColumn(1.2f);  // Deuda Total
                            columns.RelativeColumn(1.0f);  // Entregado
                            columns.RelativeColumn(1.0f);  // Ultimo Pago
                            columns.RelativeColumn(0.5f);  // Hace cuantos dias
                        });

                        // ==== Encabezado con borde total ====
                        string[] headers = {
                                    "#",
                                    "Prestamo",
                                    "Capital",
                                    "Capital Activo",
                                    "Interes",
                                    "Monto Pagado",
                                    "Prox Pago",
                                    "Interes Generado",
                                    //"Mora",
                                    //"Cuotas",
                                    "Deuda Total",
                                    "Entregado",
                                    "Ultimo Pago",
                                    "Dias"
                                };

                        foreach (var header in headers)
                        {
                            table.Cell()
                                .Background(Colors.Blue.Lighten5)   // azul más suave
                                .Border(0.2f)                       // borde completo por celda
                                .BorderColor(Colors.Grey.Lighten1)
                                .PaddingVertical(4)
                                .PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(header)
                                .Bold()
                                .FontSize(8)
                                .FontColor(Colors.Black);           // texto negro
                        }

                        // ==== Filas de datos ====
                        bool alternate = false;

                        for (int i = 0; i < prestamosVigentes.Count(); i++)
                        {
                            var prestamo = prestamosVigentes.ElementAt(i);

                            var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                            alternate = !alternate;

                            var item = i + 1;
                            var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);
                            //prestamosCalculo.Add(calculo);

                            var mora = calculo.InteresGenerado - calculo.ProximoPago;

                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo ?? string.Empty}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.CantidadInicial:N2}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.RestaCapital:N2}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.InteresPorcentaje}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.MontoPagado:N2}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{prestamo.EstimadoAPagarMes:N2}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{calculo.InteresGenerado:N2}");
                            //table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{mora:N2}").FontColor(mora == 0 ? Colors.Red.Medium : Colors.Grey.Darken4);
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{calculo.DeudaTotal:N2}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaUltimoPago == DateTime.MinValue ? string.Empty : prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{calculo.DiasGenerados}");


                            #region Calcular totales
                            totalCapital += prestamo.CantidadInicial;
                            totalCapitalActivo += prestamo.RestaCapital;
                            totalMontoPagado += prestamo.MontoPagado;
                            totalEstimadoAPagar += prestamo.EstimadoAPagarMes;
                            totalInteresGenerado += calculo.InteresGenerado;
                            porcentajesInteresSuma += prestamo.InteresPorcentaje;
                            deudaTotalAll += calculo.DeudaTotal;
                            #endregion
                        }




                        // ===== FILA DE TOTALES =====
                        var bgTotal = Colors.Grey.Lighten3;

                        // Ponemos "TOTAL" en la primera celda y dejamos las dos siguientes vacías
                        table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");
                        table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();  // tercera columna

                        // Columnas de números
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapitalActivo:N2}").Bold();
                        var porcetajeIntereses = porcentajesInteresSuma / prestamosVigentes.Count();
                        table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}"); // Interes %
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalMontoPagado:N2}").Bold();
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalEstimadoAPagar:N2}").Bold();
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalInteresGenerado:N2}").Bold();

                        // Columnas vacías
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{deudaTotalAll:N2}").Bold(); // Deuda Total
                        table.Cell().Background(bgTotal).Padding(4).Text(""); // Entregado
                        table.Cell().Background(bgTotal).Padding(4).Text(""); // Ultimo Pago
                        table.Cell().Background(bgTotal).Padding(4).Text(""); // Dias

                    });


                }

                #endregion



                #region Prestamos Pagados

                if (prestamosPagados.Any())
                {
                    col.Item().Height(15);


                    col.Item().Text($"Pagados")
                        //.Bold()
                        .FontSize(10)
                        .AlignCenter();

                    col.Item().Height(5);

                    // ===== CALCULAR TOTALES =====
                    decimal totalCapital = 0;
                    decimal porcentajesInteresSuma = 0;
                    int cuotasTotal = 0;
                    decimal totalMontoPagado = 0;
                    decimal gananciasTotal = 0;


                    // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                    col.Item().Table(table =>
                    {
                        // ==== Definición de columnas ====
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(0.35f); // #
                            columns.RelativeColumn(1.4f);  // Documento

                            columns.RelativeColumn(1.1f);  // Cantidad
                            columns.RelativeColumn(1.2f);  // Entregado
                            columns.RelativeColumn(0.7f);  // Interes
                            columns.RelativeColumn(0.7f);  // Cuotas
                            columns.RelativeColumn(1.2f);  // Ultimo Pago
                            columns.RelativeColumn(1.2f);  // Monto Pagado

                            columns.RelativeColumn(3.8f);  // Observaciones
                        });

                        // ==== Encabezado con borde total ====
                        string[] headers = {
                                        "#",
                                        "Prestamo",
                                        "Cantidad",
                                        "Entregado",
                                        "Interes",
                                        "Cuotas",
                                        "Ultimo Pago",
                                        "Monto Pagado",
                                        "Observaciones"
                                    };

                        foreach (var header in headers)
                        {
                            table.Cell()
                                .Background(Colors.Orange.Lighten5)   // Naranja
                                .Border(0.2f)                       // borde completo por celda
                                .BorderColor(Colors.Grey.Lighten1)
                                .PaddingVertical(4)
                                .PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(header)
                                .Bold()
                                .FontSize(8)
                                .FontColor(Colors.Black);           // texto negro
                        }

                        // ==== Filas de datos ====
                        bool alternate = false;

                        for (int i = 0; i < prestamosPagados.Count(); i++)
                        {
                            var prestamo = prestamosPagados.ElementAt(i);

                            var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                            alternate = !alternate;

                            var item = i + 1;
                            var calculo = _calculationService.InteresDiario_CalcularInteresAndCapitalByFechaCotizacion(prestamo, configuracion, fechaActual, 0);
                            //prestamosCalculo.Add(calculo);


                            var nombreResponsable = "";

                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignLeft().Text($"{item}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.CodigoPrestamo ?? string.Empty}");
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.CantidadInicial.ToString("N2") ?? "0.00"); //Cantidad
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaEntragado.ToString("dd-MM-yyyy"));   //Entregado
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.InteresPorcentaje.ToString("N2") ?? "0.00"); //Interes
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.CuotasPagadas.ToString() ?? "0.00");     //Cuotas
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(prestamo.FechaUltimoPago == DateTime.MinValue ? string.Empty : prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));   //UltimoPago
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text(prestamo.MontoPagado.ToString("N2") ?? "0.00");  //Monto Pagado
                            table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text($"{prestamo.Observacion ?? string.Empty}"); //Observaciones


                            #region Calcular totales
                            totalCapital += prestamo.CantidadInicial;
                            porcentajesInteresSuma += prestamo.InteresPorcentaje;
                            cuotasTotal += prestamo.CuotasPagadas;
                            totalMontoPagado += prestamo.MontoPagado;
                            gananciasTotal += (decimal)prestamo.Ganancia;

                            #endregion
                        }


                        var porcetajeIntereses = porcentajesInteresSuma / prestamosPagados.Count();

                        // ===== FILA DE TOTALES =====
                        var bgTotal = Colors.Grey.Lighten3;

                        table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text("");                                     //Item
                        table.Cell().Background(bgTotal).Padding(4).Text("TOTAL").Bold();                                       //CodigoPrestamo
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalCapital:N2}").Bold();             //CantidadInicial
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text("").Bold();                               //FechaEntragado
                        table.Cell().Background(bgTotal).Padding(4).AlignCenter().Text($"{porcetajeIntereses:N2}");             //InteresPorcentaje       
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{cuotasTotal}").Bold();                 //CuotasPagadas
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"").Bold();                              //FechaUltimoPago
                        table.Cell().Background(bgTotal).Padding(4).AlignRight().Text($"{totalMontoPagado:N2}").Bold();         //MontoPagado
                        table.Cell().Background(bgTotal).Padding(4).Text("");                                                   //Observacion

                    });


                }

                #endregion



                col.Item().Height(20);
                col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


                col.Item().Height(1);
                // ======= TEXTO DE NOTA =======
                col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
                    .Italic()                 // Texto en cursiva
                    .AlignCenter()
                    .FontSize(8);             // Más pequeño que el texto normal
                                              //.FontColor(Colors.Grey.Darken2); // Color gris suave
            });

            FooterPIM(page);
        }


        private void RenderEstadoPrestamoPaginaIndpendiente(PageDescriptor page, Prestamo prestamo, InteresDiario_CalcularInteresAndCapitalVm calculo, byte[] logoBytes, byte[] qrBytes, DateTime fechaActual)
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(8));

            EncabezadoPIM(page, logoBytes, qrBytes);

            page.Content().PaddingVertical(10).Column(col =>
            {
                col.Item().Text($"ESTADO DE PRÉSTAMO {prestamo.CodigoPrestamo}")
                    .Bold().FontSize(12).AlignCenter();

                col.Item().Height(20);

                // ======= SECCIÓN: INFORMACIÓN DEL CLIENTE =======
                col.Item().Column(c =>
                {
                    c.Item().Text("INFORMACIÓN DEL CLIENTE")
                        .Bold().FontSize(9);
                    //.FontColor(Colors.Blue.Darken3);

                    c.Item().Height(2);

                    // Fondo gris claro y diseño limpio
                    c.Item().Element(e =>
                    {
                        e.Background(Colors.Grey.Lighten4)
                         .CornerRadius(2)
                         .Padding(2)
                         .PaddingHorizontal(7)
                         .Column(colCliente =>
                         {
                             // ====== Fila 1: Nombre (ocupa 2 columnas) + Identificación + Teléfono ======
                             colCliente.Item().Row(row =>
                             {
                                 // Nombre (2 columnas)
                                 row.RelativeColumn(2).Column(cc =>
                                 {
                                     cc.Item().Text("Nombre").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text($"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}")
                                         .FontSize(8);
                                 });

                                 // Identificación
                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Codigo Cliente").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamo.Cliente.CodigoPersona ?? "—").FontSize(8);
                                 });

                                 // Teléfono
                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Teléfono").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamo.Cliente.Telefono ?? "—").FontSize(8);
                                 });
                             });

                             colCliente.Item().Height(5);

                             // ====== Fila 2: Correo + Lugar de trabajo + Ciudad + País ======
                             colCliente.Item().Row(row =>
                             {
                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Correo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamo.Cliente.Correo ?? "—").FontSize(8);
                                 });

                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Ciudad").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamo.Cliente.Ciudad ?? "—").FontSize(8);
                                 });

                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("País").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamo.Cliente.Pais ?? "—").FontSize(8);
                                 });

                                 row.RelativeColumn(1).Column(cc =>
                                 {
                                     cc.Item().Text("Lugar de trabajo").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                                     cc.Item().Text(prestamo.Cliente.LugarTrabajo ?? "—").FontSize(8);
                                 });
                             });

                             colCliente.Item().Height(5);

                             // ====== Fila 3: Dirección completa (ocupa las 4 columnas) ======
                             //colCliente.Item().Row(row =>
                             //{
                             //    row.RelativeColumn(4).Column(cc =>
                             //    {
                             //        cc.Item().Text("Dirección").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                             //        cc.Item().Text($"{prestamo.Cliente.Direccion ?? "—"}")
                             //            .FontSize(8);
                             //    });
                             //});
                         });
                    });
                });





                col.Item().Height(12);




                // ======= SECCIÓN: INFORMACIÓN DEL PRÉSTAMO =======
                col.Item().Column(c =>
                {
                    c.Item().Text("DETALLES DEL PRÉSTAMO")
                        .Bold().FontSize(9);
                    //.FontColor(Colors.Blue.Darken3);

                    c.Item().Height(2);

                    c.Item().Element(e =>
                    {
                        e.Background(Colors.Grey.Lighten5)
                         .CornerRadius(2)
                         .Padding(2)
                         .Border(0.3f)
                         .BorderColor(Colors.Grey.Lighten1)
                         .Column(colPrestamo =>
                         {
                             void Cell(RowDescriptor row, string label, string value, bool alternate = false, string fontColor = null)
                             {
                                 row.RelativeColumn(1)
                                    .Background(alternate ? Colors.White : Colors.Grey.Lighten4)
                                    .Border(0.25f)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(3)
                                    .PaddingHorizontal(4)
                                    .Column(c2 =>
                                    {
                                        c2.Item().Text(label)
                                            .Bold()
                                            .FontSize(8)
                                            .FontColor(Colors.Grey.Darken3);

                                        c2.Item().Text(value ?? "—")
                                            .FontSize(8)
                                            .FontColor(fontColor ?? Colors.Grey.Darken4);
                                    });
                             }

                             // ====== Fila 1 ======
                             colPrestamo.Item().Row(row =>
                             {
                                 Cell(row, "Código", prestamo.CodigoPrestamo);
                                 Cell(row, "Monto inicial", $" {prestamo.CantidadInicial:N2}");
                                 Cell(row, "Interés mensual", $"{prestamo.InteresPorcentaje:N2}%");
                                 Cell(row, "Fecha de entrega", prestamo.FechaEntragado.ToString("dd-MM-yyyy"));
                                 Cell(row, "Moneda", prestamo.Moneda_Descripcion);

                             });

                             // ====== Fila 2 ======
                             colPrestamo.Item().Row(row =>
                             {
                                 Cell(row, "Tipo de préstamo", prestamo.TipoPrestamo_Descripcion, true);
                                 Cell(row, "Transacción Cuenta", /*cuentaBancaria*/ "Loko", true);
                                 Cell(row, "Estado", prestamo.Estado_Descripcion, true);
                                 Cell(row, "Cuotas pagadas", prestamo.CuotasPagadas.ToString(), true);
                                 Cell(row, "Monto pagado", $" {prestamo.MontoPagado:N2}", true);
                             });

                             // ====== Fila 3 ======
                             colPrestamo.Item().Row(row =>
                             {
                                 Cell(row, "Saldo actual", $" {prestamo.RestaCapital:N2}");
                                 Cell(row, "Fecha Ultimo Pago", prestamo.FechaUltimoPago.ToString("dd-MM-yyyy"));
                                 //Cell(row, "Pago Minimo", $"{ultimoDetalle.ProximoPago.ToString():N2}");
                                 Cell(row, "Pago Minimo", $" {prestamo.EstimadoAPagarMes:N2}");
                                 Cell(row, "Interes Generado", $"{calculo.InteresGenerado:N2}", false, fontColor: calculo.InteresGenerado > prestamo.EstimadoAPagarMes ? Colors.Red.Medium : Colors.Grey.Darken4);
                                 Cell(row, "Deuda Total", calculo.DeudaTotal != 0 ? $"{calculo.DeudaTotal:N2}" : $"{(prestamo.RestaCapital + calculo.InteresGenerado):N2}");
                             });
                         });
                    });
                });



                col.Item().Height(4);




                // ======= TABLA DE DETALLES DEL PRÉSTAMO =======
                col.Item().Table(table =>
                {
                    // ==== Definición de columnas ====
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(0.4f);  // #
                        columns.RelativeColumn(0.9f);  // Recibido
                        columns.RelativeColumn(1.0f);  // Cuenta Origen
                        columns.RelativeColumn(0.9f);  // Fecha Pago
                        columns.RelativeColumn(0.9f);  // Interés
                        columns.RelativeColumn(1.0f);  // Capital
                        columns.RelativeColumn(0.9f);  // Pago Hasta
                        columns.RelativeColumn(1.0f);  // Resta Capital
                        columns.RelativeColumn(0.9f);  // Próx Fecha
                        columns.RelativeColumn(0.9f);  // Próx Pago
                        columns.RelativeColumn(2.4f);  // Observación
                    });

                    // ==== Encabezado con borde total ====
                    string[] headers = { "#", "Recibido", "Cuenta Origen", "Fecha Pago", "Interés",
                                         "Capital Aportado", "Pago Hasta", "Resta Capital",
                                         "F. Próx. Pago", "Próximo Pago", "***"
                                };

                    foreach (var header in headers)
                    {



                        if(prestamo.Estado == EstadoPrestamo.Vigente)
                        {
                            table.Cell()
                                .Background(Colors.Blue.Lighten5)   // azul suave
                                .Border(0.2f)                       // borde completo por celda
                                .BorderColor(Colors.Grey.Lighten1)
                                .PaddingVertical(4)
                                .PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(header)
                                .Bold()
                                .FontSize(8)
                                .FontColor(Colors.Black);           // texto negro
                        }
                        else
                        {
                            table.Cell()
                                .Background(Colors.Orange.Lighten5)   // naranja suave
                                .Border(0.2f)                       // borde completo por celda
                                .BorderColor(Colors.Grey.Lighten1)
                                .PaddingVertical(4)
                                .PaddingHorizontal(3)
                                .AlignCenter()
                                .Text(header)
                                .Bold()
                                .FontSize(8)
                                .FontColor(Colors.Black);           // texto negro
                        }
                    }

                    // ==== Filas de datos ====
                    bool alternate = false;
                    foreach (var d in prestamo.PrestamoDetalles.OrderBy(x => x.NumeroCuota))
                    {
                        var bg = alternate ? Colors.Grey.Lighten5 : Colors.White;
                        alternate = !alternate;

                        string cuentaOrigen = "";

                        if (d.CuentaBancaria != null)
                        {
                            cuentaOrigen = d.CuentaBancaria.NumeroCuenta;
                        }


                        // Primera celda (# de cuota)
                        table.Cell()
                            //.Background(Colors.Blue.Lighten5)
                            //.Border(0.25f)
                            //.BorderColor(Colors.Grey.Lighten4)
                            .Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4)
                            .Padding(4)
                            .AlignCenter()
                            .Text(d.NumeroCuota.ToString());
                        //.FontColor(Colors.Blue.Darken3)
                        //.Bold();

                        // Resto de las columnas
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{d.TotalAPagar:N2}");
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{cuentaOrigen}");
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.CreatedDate.ToString("dd-MM-yyyy"));
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoInteres:N2}");
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.MontoCapital:N2}");
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaPago.ToString("dd-MM-yyyy"));
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.RestaCapital:N2}");
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignCenter().Text(d.FechaProximoPago.ToString("dd-MM-yyyy"));
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.ProximoPago:N2}");
                        table.Cell().Background(bg).Border(0.25f).BorderColor(Colors.Grey.Lighten4).Padding(4).AlignRight().Text($"{d.Observacion}");
                    }
                });




                col.Item().Height(20);
                col.Item().Text($"Reporte generado el {fechaActual:dd-MM-yyyy hh:mm:ss tt}.").FontSize(8).Italic().AlignCenter().FontColor(Colors.Grey.Darken1);


                col.Item().Height(1);
                // ======= TEXTO DE NOTA =======
                col.Item().Text("NOTA: La deuda total está calculada de acuerdo a la fecha en que se generó este documento y no representa la deuda total oficial si la fecha de hoy es distinta.")
                    .Italic()                 // Texto en cursiva
                    .AlignCenter()
                    .FontSize(8);             // Más pequeño que el texto normal
                                              //.FontColor(Colors.Grey.Darken2); // Color gris suave
            });

            FooterPIM(page);
        }














        public async Task<AppResult> GenerarPdfEjemplo()
        {
            try
            {
                DateTime fecha = DateTime.Now;

                // ==== Datos en duro de ejemplo ====
                string textoContenido = "Este es un PDF de ejemplo utilizando los métodos Header y Footer.\n" +
                                        "Aquí puedes colocar cualquier contenido dinámico, como contratos, tablas o reportes.";

                // Cargar logo de la misma ruta que usas
                var logoPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Imagenes", "logoCoopaz.png");
                var logoBytes = File.ReadAllBytes(logoPath);

                // Generar QR de ejemplo
                var qrBytes = await _iQrServices.GenerarQR("PDF de ejemplo - Cooperativa Coopaz");

                // Crear PDF
                var pdf = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                        // === Encabezado reutilizando tu clase estática ===
                        EncabezadoPIM(page, logoBytes, qrBytes);

                        // === Contenido principal ===
                        page.Content().PaddingVertical(10).Column(col =>
                        {
                            col.Item().Text("CONTRATO DE PRUEBA").Bold().FontSize(12).AlignCenter();
                            col.Item().Height(20);

                            // Texto de ejemplo
                            col.Item().Text(textoContenido)
                                .FontSize(10)
                                .LineHeight(1.4f)
                                .AlignLeft();

                            col.Item().Height(30);

                            // Firma de ejemplo
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

                        // === Pie de página reutilizando tu clase estática ===
                        FooterPIM(page);
                    });
                });

                // Generar PDF y devolver bytes
                //return await Task.FromResult(pdf.GeneratePdf());
                var generarpdf = await Task.FromResult(pdf.GeneratePdf());

                return AppResult.New(true, generarpdf, "Pdf de ejemplo");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar PDF de ejemplo: {ex.Message}");
            }
        }
















        public static void EncabezadoPIM(PageDescriptor page, byte[] logoBytes, byte[] qrBytes)
        {

            var fechaActual = DateTime.Now;

            page.Header().Row(row =>
            {
                // Logo
                row.ConstantColumn(100).Element(e => e.AlignLeft().Height(80).Width(100).Image(logoBytes).FitArea());

                // Texto
                row.RelativeColumn().Column(col =>
                {
                    col.Item().AlignCenter().Text("COOPERATIVA COOPAZ").Bold().FontSize(14);
                    col.Item().AlignCenter().Text("Francisco Morazán, Honduras").Bold().FontSize(10);
                    col.Item().AlignCenter().Text($"Fecha: {fechaActual:dd-MM-yyyy}").FontSize(9);
                });

                // QR
                row.ConstantColumn(90).Height(60).Element(e => e.AlignRight().Height(60).Width(80).Image(qrBytes).FitArea());
            });
        }

        public static void FooterPIM(PageDescriptor page)
        {
            var fechaActual = DateTime.Now;

            page.Footer().Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().AlignCenter().Text($"Copyright {fechaActual.Year}. Cooperativa Coopaz © Todos los derechos reservados").FontSize(8);
                col.Item().AlignCenter().Text("Este documento es válido únicamente en formato original.").FontSize(7);
            });
        }









    }
}

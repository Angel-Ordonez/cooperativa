using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class socioclientes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ClienteBueno",
                table: "Persona",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoPersona",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Edad",
                table: "Persona",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Persona",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaDeIngreso",
                table: "Persona",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Persona",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimaInversion",
                table: "Persona",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identidad",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LugarTrabajo",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nota",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ocupacion",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrestamoActivo",
                table: "Persona",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecomendadoPor",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Socio_Estado",
                table: "Persona",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalMontoInvertidoAnio",
                table: "Persona",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Prestamo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPrestamo = table.Column<int>(type: "int", nullable: false),
                    Responsable = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoPrestamo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CantidadInicial = table.Column<double>(type: "float", nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    InteresPorcentaje = table.Column<double>(type: "float", nullable: false),
                    CantidadMeses = table.Column<int>(type: "int", nullable: false),
                    EstimadoAPagarMes = table.Column<double>(type: "float", nullable: false),
                    Garantia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaEntragado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEstimadoFinPrestamo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaUltimoPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MesesPagados = table.Column<int>(type: "int", nullable: false),
                    MontoPagado = table.Column<double>(type: "float", nullable: false),
                    RestaCapital = table.Column<double>(type: "float", nullable: false),
                    DebeMeses = table.Column<int>(type: "int", nullable: false),
                    Mora = table.Column<double>(type: "float", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestamo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prestamo_Persona_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocioInversion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CodigoInversion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cantidad = table.Column<double>(type: "float", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    SocioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SocioNombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocioInversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocioInversion_Persona_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Egreso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroIngreso = table.Column<int>(type: "int", nullable: false),
                    Correlativo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<double>(type: "float", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Egreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Egreso_Prestamo_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrestamoDetalle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroCuota = table.Column<int>(type: "int", nullable: false),
                    MontoCapital = table.Column<double>(type: "float", nullable: false),
                    MontoInteres = table.Column<double>(type: "float", nullable: false),
                    TotalAPagar = table.Column<double>(type: "float", nullable: false),
                    Moneda = table.Column<int>(type: "int", nullable: false),
                    RestaCapital = table.Column<double>(type: "float", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProximoPago = table.Column<double>(type: "float", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrestamoDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrestamoDetalle_Persona_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrestamoDetalle_Prestamo_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ingreso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroIngreso = table.Column<int>(type: "int", nullable: false),
                    Correlativo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<double>(type: "float", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SocioInversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrestamoDetalleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingreso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingreso_PrestamoDetalle_PrestamoDetalleId",
                        column: x => x.PrestamoDetalleId,
                        principalTable: "PrestamoDetalle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingreso_SocioInversion_SocioInversionId",
                        column: x => x.SocioInversionId,
                        principalTable: "SocioInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Caja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaldoAnterior = table.Column<double>(type: "float", nullable: false),
                    SaldoActual = table.Column<double>(type: "float", nullable: false),
                    IngresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EgresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Caja_Egreso_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "Egreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Caja_Ingreso_IngresoId",
                        column: x => x.IngresoId,
                        principalTable: "Ingreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroTransaccion = table.Column<int>(type: "int", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoTransaccion = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<double>(type: "float", nullable: false),
                    IngresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EgresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaccion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaccion_Egreso_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "Egreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transaccion_Ingreso_IngresoId",
                        column: x => x.IngresoId,
                        principalTable: "Ingreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Caja_EgresoId",
                table: "Caja",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_Caja_IngresoId",
                table: "Caja",
                column: "IngresoId");

            migrationBuilder.CreateIndex(
                name: "IX_Egreso_PrestamoId",
                table: "Egreso",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingreso_PrestamoDetalleId",
                table: "Ingreso",
                column: "PrestamoDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingreso_SocioInversionId",
                table: "Ingreso",
                column: "SocioInversionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_ClienteId",
                table: "Prestamo",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PrestamoDetalle_ClienteId",
                table: "PrestamoDetalle",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PrestamoDetalle_PrestamoId",
                table: "PrestamoDetalle",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_SocioInversion_SocioId",
                table: "SocioInversion",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_EgresoId",
                table: "Transaccion",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_IngresoId",
                table: "Transaccion",
                column: "IngresoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Caja");

            migrationBuilder.DropTable(
                name: "Transaccion");

            migrationBuilder.DropTable(
                name: "Egreso");

            migrationBuilder.DropTable(
                name: "Ingreso");

            migrationBuilder.DropTable(
                name: "PrestamoDetalle");

            migrationBuilder.DropTable(
                name: "SocioInversion");

            migrationBuilder.DropTable(
                name: "Prestamo");

            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "ClienteBueno",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "CodigoPersona",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Edad",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "FechaDeIngreso",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "FechaUltimaInversion",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Identidad",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "LugarTrabajo",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Nota",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Ocupacion",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "PrestamoActivo",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "RecomendadoPor",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Socio_Estado",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "TotalMontoInvertidoAnio",
                table: "Persona");
        }
    }
}

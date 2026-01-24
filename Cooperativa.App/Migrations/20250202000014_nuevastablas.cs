using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class nuevastablas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadEnDolar",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadEnEuro",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "HistorialCambioMonedaId",
                table: "SocioInversion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadEnDolar",
                table: "Prestamo",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadEnEuro",
                table: "Prestamo",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "HistorialCambioMonedaId",
                table: "Prestamo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialCambioMoneda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonedaBase = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Euro = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    Dolar = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    Lempira = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    Quetzal = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    MXN = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    CordobaNicaragua = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    ColonCostaRica = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCambioMoneda", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Retiro",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroRetiro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    Moneda_Descripcion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CantidadEnEuro = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    CantidadEnDolar = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoRetiro = table.Column<int>(type: "int", nullable: false),
                    TipoRetiro_Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Estado_Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EgresoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SocioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsableAprobacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HistorialCambioMonedaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Retiro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Retiro_Caja_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Caja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retiro_Egreso_EgresoId",
                        column: x => x.EgresoId,
                        principalTable: "Egreso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Retiro_HistorialCambioMoneda_HistorialCambioMonedaId",
                        column: x => x.HistorialCambioMonedaId,
                        principalTable: "HistorialCambioMoneda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retiro_Persona_ResponsableAprobacionId",
                        column: x => x.ResponsableAprobacionId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Retiro_Persona_SocioId",
                        column: x => x.SocioId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SocioInversionRetiro",
                columns: table => new
                {
                    SocioInversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RetiroId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocioInversionRetiro", x => new { x.SocioInversionId, x.RetiroId });
                    table.ForeignKey(
                        name: "FK_SocioInversionRetiro_Retiro_RetiroId",
                        column: x => x.RetiroId,
                        principalTable: "Retiro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocioInversionRetiro_SocioInversion_SocioInversionId",
                        column: x => x.SocioInversionId,
                        principalTable: "SocioInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocioInversion_HistorialCambioMonedaId",
                table: "SocioInversion",
                column: "HistorialCambioMonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_HistorialCambioMonedaId",
                table: "Prestamo",
                column: "HistorialCambioMonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_CajaId",
                table: "Retiro",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_EgresoId",
                table: "Retiro",
                column: "EgresoId");

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_HistorialCambioMonedaId",
                table: "Retiro",
                column: "HistorialCambioMonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_ResponsableAprobacionId",
                table: "Retiro",
                column: "ResponsableAprobacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_SocioId",
                table: "Retiro",
                column: "SocioId");

            migrationBuilder.CreateIndex(
                name: "IX_SocioInversionRetiro_RetiroId",
                table: "SocioInversionRetiro",
                column: "RetiroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamo_HistorialCambioMoneda_HistorialCambioMonedaId",
                table: "Prestamo",
                column: "HistorialCambioMonedaId",
                principalTable: "HistorialCambioMoneda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SocioInversion_HistorialCambioMoneda_HistorialCambioMonedaId",
                table: "SocioInversion",
                column: "HistorialCambioMonedaId",
                principalTable: "HistorialCambioMoneda",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prestamo_HistorialCambioMoneda_HistorialCambioMonedaId",
                table: "Prestamo");

            migrationBuilder.DropForeignKey(
                name: "FK_SocioInversion_HistorialCambioMoneda_HistorialCambioMonedaId",
                table: "SocioInversion");

            migrationBuilder.DropTable(
                name: "SocioInversionRetiro");

            migrationBuilder.DropTable(
                name: "Retiro");

            migrationBuilder.DropTable(
                name: "HistorialCambioMoneda");

            migrationBuilder.DropIndex(
                name: "IX_SocioInversion_HistorialCambioMonedaId",
                table: "SocioInversion");

            migrationBuilder.DropIndex(
                name: "IX_Prestamo_HistorialCambioMonedaId",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "CantidadEnDolar",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "CantidadEnEuro",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "HistorialCambioMonedaId",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "CantidadEnDolar",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "CantidadEnEuro",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "HistorialCambioMonedaId",
                table: "Prestamo");
        }
    }
}

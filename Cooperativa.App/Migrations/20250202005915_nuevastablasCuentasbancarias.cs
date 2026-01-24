using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class nuevastablasCuentasbancarias : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "SocioInversion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "Retiro",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "PrestamoDetalle",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "Prestamo",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstitucionBancaria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pais = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direccion2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefono2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SitioWeb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoInstitucion = table.Column<int>(type: "int", nullable: false),
                    TipoInstitucion_Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitucionBancaria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentaBancaria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroCuenta = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroTarjeta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoCuenta = table.Column<int>(type: "int", nullable: false),
                    TipoCuenta_Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaldoAnterior = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    UltimoMovimiento = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    CantidadMovimiento = table.Column<int>(type: "int", nullable: false),
                    FechaUltimoMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstitucionBancariaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentaBancaria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentaBancaria_InstitucionBancaria_InstitucionBancariaId",
                        column: x => x.InstitucionBancariaId,
                        principalTable: "InstitucionBancaria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentaBancaria_Persona_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocioInversion_CuentaBancariaId",
                table: "SocioInversion",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_CuentaBancariaId",
                table: "Retiro",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_PrestamoDetalle_CuentaBancariaId",
                table: "PrestamoDetalle",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prestamo_CuentaBancariaId",
                table: "Prestamo",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaBancaria_InstitucionBancariaId",
                table: "CuentaBancaria",
                column: "InstitucionBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaBancaria_PersonaId",
                table: "CuentaBancaria",
                column: "PersonaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamo_CuentaBancaria_CuentaBancariaId",
                table: "Prestamo",
                column: "CuentaBancariaId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrestamoDetalle_CuentaBancaria_CuentaBancariaId",
                table: "PrestamoDetalle",
                column: "CuentaBancariaId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Retiro_CuentaBancaria_CuentaBancariaId",
                table: "Retiro",
                column: "CuentaBancariaId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SocioInversion_CuentaBancaria_CuentaBancariaId",
                table: "SocioInversion",
                column: "CuentaBancariaId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prestamo_CuentaBancaria_CuentaBancariaId",
                table: "Prestamo");

            migrationBuilder.DropForeignKey(
                name: "FK_PrestamoDetalle_CuentaBancaria_CuentaBancariaId",
                table: "PrestamoDetalle");

            migrationBuilder.DropForeignKey(
                name: "FK_Retiro_CuentaBancaria_CuentaBancariaId",
                table: "Retiro");

            migrationBuilder.DropForeignKey(
                name: "FK_SocioInversion_CuentaBancaria_CuentaBancariaId",
                table: "SocioInversion");

            migrationBuilder.DropTable(
                name: "CuentaBancaria");

            migrationBuilder.DropTable(
                name: "InstitucionBancaria");

            migrationBuilder.DropIndex(
                name: "IX_SocioInversion_CuentaBancariaId",
                table: "SocioInversion");

            migrationBuilder.DropIndex(
                name: "IX_Retiro_CuentaBancariaId",
                table: "Retiro");

            migrationBuilder.DropIndex(
                name: "IX_PrestamoDetalle_CuentaBancariaId",
                table: "PrestamoDetalle");

            migrationBuilder.DropIndex(
                name: "IX_Prestamo_CuentaBancariaId",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "Retiro");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "PrestamoDetalle");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "Prestamo");
        }
    }
}

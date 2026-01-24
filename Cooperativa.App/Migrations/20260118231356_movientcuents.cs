using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class movientcuents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MovimientoEntreCuentaId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MovimientoEntreCuenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroMovimiento = table.Column<int>(type: "int", nullable: false),
                    Referencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CajaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaBancariaOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CuentaBancariaDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientoEntreCuenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientoEntreCuenta_Caja_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Caja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientoEntreCuenta_CuentaBancaria_CuentaDestinoId",
                        column: x => x.CuentaDestinoId,
                        principalTable: "CuentaBancaria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientoEntreCuenta_CuentaBancaria_CuentaOrigenId",
                        column: x => x.CuentaOrigenId,
                        principalTable: "CuentaBancaria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_MovimientoEntreCuentaId",
                table: "Transaccion",
                column: "MovimientoEntreCuentaId",
                unique: true,
                filter: "[MovimientoEntreCuentaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoEntreCuenta_CajaId",
                table: "MovimientoEntreCuenta",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoEntreCuenta_CuentaDestinoId",
                table: "MovimientoEntreCuenta",
                column: "CuentaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoEntreCuenta_CuentaOrigenId",
                table: "MovimientoEntreCuenta",
                column: "CuentaOrigenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_MovimientoEntreCuenta_MovimientoEntreCuentaId",
                table: "Transaccion",
                column: "MovimientoEntreCuentaId",
                principalTable: "MovimientoEntreCuenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_MovimientoEntreCuenta_MovimientoEntreCuentaId",
                table: "Transaccion");

            migrationBuilder.DropTable(
                name: "MovimientoEntreCuenta");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_MovimientoEntreCuentaId",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "MovimientoEntreCuentaId",
                table: "Transaccion");
        }
    }
}

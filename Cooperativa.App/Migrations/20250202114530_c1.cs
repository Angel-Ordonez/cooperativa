using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class c1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CantidadMovimientoS",
                table: "CuentaBancaria",
                newName: "CantidadMovimientos");

            migrationBuilder.RenameColumn(
                name: "SaldoTotalTransfeido",
                table: "CuentaBancaria",
                newName: "SaldoTotalTransferido");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CantidadMovimientos",
                table: "CuentaBancaria",
                newName: "CantidadMovimientoS");

            migrationBuilder.RenameColumn(
                name: "SaldoTotalTransferido",
                table: "CuentaBancaria",
                newName: "SaldoTotalTransfeido");
        }
    }
}

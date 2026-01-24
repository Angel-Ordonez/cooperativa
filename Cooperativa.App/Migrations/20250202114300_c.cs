using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class c : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SaldoActual",
                table: "CuentaBancaria",
                newName: "SaldoTotalTransfeido");

            migrationBuilder.RenameColumn(
                name: "CantidadMovimiento",
                table: "CuentaBancaria",
                newName: "CantidadMovimientoS");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SaldoTotalTransfeido",
                table: "CuentaBancaria",
                newName: "SaldoActual");

            migrationBuilder.RenameColumn(
                name: "CantidadMovimientoS",
                table: "CuentaBancaria",
                newName: "CantidadMovimiento");
        }
    }
}

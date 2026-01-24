using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class nsssn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaldoActual",
                table: "CuentaBancaria",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoActual",
                table: "CuentaBancaria");
        }
    }
}

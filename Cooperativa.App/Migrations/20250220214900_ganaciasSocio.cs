using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class ganaciasSocio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GananciaTotal",
                table: "Persona",
                type: "decimal(9,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoDisponibleARetirar",
                table: "Persona",
                type: "decimal(9,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRetirado",
                table: "Persona",
                type: "decimal(9,3)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GananciaTotal",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "SaldoDisponibleARetirar",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "TotalRetirado",
                table: "Persona");
        }
    }
}

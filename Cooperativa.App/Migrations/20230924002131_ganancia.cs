using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class ganancia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Ganancia",
                table: "Prestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CantidadPrestamos",
                table: "Persona",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ganancia",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "CantidadPrestamos",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "Persona");
        }
    }
}

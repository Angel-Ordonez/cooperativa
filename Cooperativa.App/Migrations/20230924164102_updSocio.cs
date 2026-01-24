using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class updSocio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalMontoInvertidoAnio",
                table: "Persona");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ganancia",
                table: "Prestamo",
                type: "decimal(9,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AddColumn<int>(
                name: "CantidadInvensiones",
                table: "Persona",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalMontoInvertido",
                table: "Persona",
                type: "decimal(9,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadInvensiones",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "TotalMontoInvertido",
                table: "Persona");

            migrationBuilder.AlterColumn<decimal>(
                name: "Ganancia",
                table: "Prestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalMontoInvertidoAnio",
                table: "Persona",
                type: "float",
                nullable: true);
        }
    }
}

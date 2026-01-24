using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class configpre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiasDeGracias",
                table: "ConfiguracionPrestamo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HistoricoCambios",
                table: "ConfiguracionPrestamo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InteresAnual",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InteresPIM",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InteresPorMora",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MonedaPorDefecto",
                table: "ConfiguracionPrestamo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "ConfiguracionPrestamo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoMaximo",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoMinimo",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "ConfiguracionPrestamo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlazoMesesMaximo",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PlazoMesesMinimo",
                table: "ConfiguracionPrestamo",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasDeGracias",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "HistoricoCambios",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "InteresAnual",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "InteresPIM",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "InteresPorMora",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "MonedaPorDefecto",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "MontoMaximo",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "MontoMinimo",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "PlazoMesesMaximo",
                table: "ConfiguracionPrestamo");

            migrationBuilder.DropColumn(
                name: "PlazoMesesMinimo",
                table: "ConfiguracionPrestamo");
        }
    }
}

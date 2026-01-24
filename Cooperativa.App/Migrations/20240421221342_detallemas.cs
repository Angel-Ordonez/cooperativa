using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class detallemas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado_Descripcion",
                table: "SocioInversion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Ganancia",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Movimientos",
                table: "SocioInversion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CuotasPagadas",
                table: "Prestamo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadPagadaDePrestamo",
                table: "DetalleSocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Ganancia",
                table: "DetalleSocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcentajeEnPrestamo",
                table: "DetalleSocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado_Descripcion",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "Ganancia",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "Movimientos",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "CuotasPagadas",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "CantidadPagadaDePrestamo",
                table: "DetalleSocioInversion");

            migrationBuilder.DropColumn(
                name: "Ganancia",
                table: "DetalleSocioInversion");

            migrationBuilder.DropColumn(
                name: "PorcentajeEnPrestamo",
                table: "DetalleSocioInversion");
        }
    }
}

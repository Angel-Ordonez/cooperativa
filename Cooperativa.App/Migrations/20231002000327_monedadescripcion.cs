using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class monedadescripcion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "Transaccion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "SocioInversion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "PrestamoDetalle",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "Prestamo",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "Ingreso",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "Egreso",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda_Descripcion",
                table: "Caja",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "PrestamoDetalle");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "Prestamo");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "Ingreso");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "Egreso");

            migrationBuilder.DropColumn(
                name: "Moneda_Descripcion",
                table: "Caja");
        }
    }
}

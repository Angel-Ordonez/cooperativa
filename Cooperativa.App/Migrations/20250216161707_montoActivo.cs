using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class montoActivo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PorcetajePrestado",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NoPrestado",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadPrestada",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "MontoActivo",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Retirado",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoActivo",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "Retirado",
                table: "SocioInversion");

            migrationBuilder.AlterColumn<decimal>(
                name: "PorcetajePrestado",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "NoPrestado",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CantidadPrestada",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cantidad",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");
        }
    }
}

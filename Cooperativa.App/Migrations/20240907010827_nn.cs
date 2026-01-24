using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class nn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimoMovimiento",
                table: "DetalleSocioInversion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UltimoMovimiento",
                table: "DetalleSocioInversion",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

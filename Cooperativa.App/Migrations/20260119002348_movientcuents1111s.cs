using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class movientcuents1111s : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroMovimiento",
                table: "MovimientoEntreCuenta");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroMovimiento",
                table: "MovimientoEntreCuenta",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

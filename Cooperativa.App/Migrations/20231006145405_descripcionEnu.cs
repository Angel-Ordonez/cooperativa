using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class descripcionEnu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Estado_DisplayName",
                table: "Prestamo",
                newName: "Estado_Descripcion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Estado_Descripcion",
                table: "Prestamo",
                newName: "Estado_DisplayName");
        }
    }
}

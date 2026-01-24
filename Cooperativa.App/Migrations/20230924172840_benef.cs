using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class benef : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Beneficiario_Identidad",
                table: "Persona",
                newName: "Beneficiario_Identificacion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Beneficiario_Identificacion",
                table: "Persona",
                newName: "Beneficiario_Identidad");
        }
    }
}

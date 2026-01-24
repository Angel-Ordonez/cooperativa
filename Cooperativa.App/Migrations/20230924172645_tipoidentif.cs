using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class tipoidentif : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Identidad",
                table: "Persona",
                newName: "TipoIdentificacion");

            migrationBuilder.AddColumn<string>(
                name: "Beneficiario_Identidad",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Beneficiario_Nombre",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Persona",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Beneficiario_Identidad",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Beneficiario_Nombre",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Persona");

            migrationBuilder.RenameColumn(
                name: "TipoIdentificacion",
                table: "Persona",
                newName: "Identidad");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class c3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retiro_Persona_SocioId",
                table: "Retiro");

            migrationBuilder.RenameColumn(
                name: "SocioId",
                table: "Retiro",
                newName: "SolicitanteId");

            migrationBuilder.RenameIndex(
                name: "IX_Retiro_SocioId",
                table: "Retiro",
                newName: "IX_Retiro_SolicitanteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Retiro_Persona_SolicitanteId",
                table: "Retiro",
                column: "SolicitanteId",
                principalTable: "Persona",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retiro_Persona_SolicitanteId",
                table: "Retiro");

            migrationBuilder.RenameColumn(
                name: "SolicitanteId",
                table: "Retiro",
                newName: "SocioId");

            migrationBuilder.RenameIndex(
                name: "IX_Retiro_SolicitanteId",
                table: "Retiro",
                newName: "IX_Retiro_SocioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Retiro_Persona_SocioId",
                table: "Retiro",
                column: "SocioId",
                principalTable: "Persona",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

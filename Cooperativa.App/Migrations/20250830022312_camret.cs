using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class camret : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retiro_Persona_ResponsableAprobacionId",
                table: "Retiro");

            migrationBuilder.RenameColumn(
                name: "ResponsableAprobacionId",
                table: "Retiro",
                newName: "ResponsableAtendioId");

            migrationBuilder.RenameIndex(
                name: "IX_Retiro_ResponsableAprobacionId",
                table: "Retiro",
                newName: "IX_Retiro_ResponsableAtendioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Retiro_Persona_ResponsableAtendioId",
                table: "Retiro",
                column: "ResponsableAtendioId",
                principalTable: "Persona",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retiro_Persona_ResponsableAtendioId",
                table: "Retiro");

            migrationBuilder.RenameColumn(
                name: "ResponsableAtendioId",
                table: "Retiro",
                newName: "ResponsableAprobacionId");

            migrationBuilder.RenameIndex(
                name: "IX_Retiro_ResponsableAtendioId",
                table: "Retiro",
                newName: "IX_Retiro_ResponsableAprobacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Retiro_Persona_ResponsableAprobacionId",
                table: "Retiro",
                column: "ResponsableAprobacionId",
                principalTable: "Persona",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

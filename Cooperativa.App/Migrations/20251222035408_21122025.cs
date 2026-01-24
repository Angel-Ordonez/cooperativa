using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class _21122025 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DetalleSocioInversionAQuienSustituyoId",
                table: "DetalleSocioInversion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "DetalleSocioInversion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSocioInversion_DetalleSocioInversionAQuienSustituyoId",
                table: "DetalleSocioInversion",
                column: "DetalleSocioInversionAQuienSustituyoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleSocioInversion_DetalleSocioInversion_DetalleSocioInversionAQuienSustituyoId",
                table: "DetalleSocioInversion",
                column: "DetalleSocioInversionAQuienSustituyoId",
                principalTable: "DetalleSocioInversion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleSocioInversion_DetalleSocioInversion_DetalleSocioInversionAQuienSustituyoId",
                table: "DetalleSocioInversion");

            migrationBuilder.DropIndex(
                name: "IX_DetalleSocioInversion_DetalleSocioInversionAQuienSustituyoId",
                table: "DetalleSocioInversion");

            migrationBuilder.DropColumn(
                name: "DetalleSocioInversionAQuienSustituyoId",
                table: "DetalleSocioInversion");

            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "DetalleSocioInversion");
        }
    }
}

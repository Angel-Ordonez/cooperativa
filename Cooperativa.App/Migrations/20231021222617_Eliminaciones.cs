using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class Eliminaciones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egreso_Prestamo_PrestamoId",
                table: "Egreso");

            migrationBuilder.AddColumn<Guid>(
                name: "PrestamoId",
                table: "Ingreso",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrestamoId",
                table: "Egreso",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "SocioInversionId",
                table: "Egreso",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ingreso_PrestamoId",
                table: "Ingreso",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_Egreso_SocioInversionId",
                table: "Egreso",
                column: "SocioInversionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egreso_Prestamo_PrestamoId",
                table: "Egreso",
                column: "PrestamoId",
                principalTable: "Prestamo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Egreso_SocioInversion_SocioInversionId",
                table: "Egreso",
                column: "SocioInversionId",
                principalTable: "SocioInversion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingreso_Prestamo_PrestamoId",
                table: "Ingreso",
                column: "PrestamoId",
                principalTable: "Prestamo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egreso_Prestamo_PrestamoId",
                table: "Egreso");

            migrationBuilder.DropForeignKey(
                name: "FK_Egreso_SocioInversion_SocioInversionId",
                table: "Egreso");

            migrationBuilder.DropForeignKey(
                name: "FK_Ingreso_Prestamo_PrestamoId",
                table: "Ingreso");

            migrationBuilder.DropIndex(
                name: "IX_Ingreso_PrestamoId",
                table: "Ingreso");

            migrationBuilder.DropIndex(
                name: "IX_Egreso_SocioInversionId",
                table: "Egreso");

            migrationBuilder.DropColumn(
                name: "PrestamoId",
                table: "Ingreso");

            migrationBuilder.DropColumn(
                name: "SocioInversionId",
                table: "Egreso");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrestamoId",
                table: "Egreso",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Egreso_Prestamo_PrestamoId",
                table: "Egreso",
                column: "PrestamoId",
                principalTable: "Prestamo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

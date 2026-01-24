using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class retirosocio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MontoActivo",
                table: "SocioInversion",
                newName: "GananciaDisponible");

            migrationBuilder.AddColumn<decimal>(
                name: "Cantidad",
                table: "SocioInversionRetiro",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadActiva",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CantidadDisponibleRetirar",
                table: "SocioInversion",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PrestamoId",
                table: "Retiro",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Retiro_PrestamoId",
                table: "Retiro",
                column: "PrestamoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Retiro_Prestamo_PrestamoId",
                table: "Retiro",
                column: "PrestamoId",
                principalTable: "Prestamo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retiro_Prestamo_PrestamoId",
                table: "Retiro");

            migrationBuilder.DropIndex(
                name: "IX_Retiro_PrestamoId",
                table: "Retiro");

            migrationBuilder.DropColumn(
                name: "Cantidad",
                table: "SocioInversionRetiro");

            migrationBuilder.DropColumn(
                name: "CantidadActiva",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "CantidadDisponibleRetirar",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "PrestamoId",
                table: "Retiro");

            migrationBuilder.RenameColumn(
                name: "GananciaDisponible",
                table: "SocioInversion",
                newName: "MontoActivo");
        }
    }
}

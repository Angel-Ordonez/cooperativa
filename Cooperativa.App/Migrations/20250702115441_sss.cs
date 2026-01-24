using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class sss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadRetiros",
                table: "Persona",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RetiroId",
                table: "Egreso",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RetiroId1",
                table: "Egreso",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonaId",
                table: "CuentaBancaria",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CajaId",
                table: "CuentaBancaria",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacion",
                table: "CuentaBancaria",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Egreso_RetiroId1",
                table: "Egreso",
                column: "RetiroId1");

            migrationBuilder.CreateIndex(
                name: "IX_CuentaBancaria_CajaId",
                table: "CuentaBancaria",
                column: "CajaId");

            migrationBuilder.AddForeignKey(
                name: "FK_CuentaBancaria_Caja_CajaId",
                table: "CuentaBancaria",
                column: "CajaId",
                principalTable: "Caja",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Egreso_Retiro_RetiroId1",
                table: "Egreso",
                column: "RetiroId1",
                principalTable: "Retiro",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CuentaBancaria_Caja_CajaId",
                table: "CuentaBancaria");

            migrationBuilder.DropForeignKey(
                name: "FK_Egreso_Retiro_RetiroId1",
                table: "Egreso");

            migrationBuilder.DropIndex(
                name: "IX_Egreso_RetiroId1",
                table: "Egreso");

            migrationBuilder.DropIndex(
                name: "IX_CuentaBancaria_CajaId",
                table: "CuentaBancaria");

            migrationBuilder.DropColumn(
                name: "CantidadRetiros",
                table: "Persona");

            migrationBuilder.DropColumn(
                name: "RetiroId",
                table: "Egreso");

            migrationBuilder.DropColumn(
                name: "RetiroId1",
                table: "Egreso");

            migrationBuilder.DropColumn(
                name: "CajaId",
                table: "CuentaBancaria");

            migrationBuilder.DropColumn(
                name: "Observacion",
                table: "CuentaBancaria");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonaId",
                table: "CuentaBancaria",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}

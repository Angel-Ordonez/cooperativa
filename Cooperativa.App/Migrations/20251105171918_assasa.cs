using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class assasa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaId",
                table: "Transaccion");

            migrationBuilder.RenameColumn(
                name: "CuentaBancariaId",
                table: "Transaccion",
                newName: "CuentaBancariaOrigenId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaccion_CuentaBancariaId",
                table: "Transaccion",
                newName: "IX_Transaccion_CuentaBancariaOrigenId");

            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaDestinoId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_CuentaBancariaDestinoId",
                table: "Transaccion",
                column: "CuentaBancariaDestinoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaDestinoId",
                table: "Transaccion",
                column: "CuentaBancariaDestinoId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaOrigenId",
                table: "Transaccion",
                column: "CuentaBancariaOrigenId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaDestinoId",
                table: "Transaccion");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaOrigenId",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_CuentaBancariaDestinoId",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaDestinoId",
                table: "Transaccion");

            migrationBuilder.RenameColumn(
                name: "CuentaBancariaOrigenId",
                table: "Transaccion",
                newName: "CuentaBancariaId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaccion_CuentaBancariaOrigenId",
                table: "Transaccion",
                newName: "IX_Transaccion_CuentaBancariaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaId",
                table: "Transaccion",
                column: "CuentaBancariaId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

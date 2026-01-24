using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class transcuen : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CuentaBancariaId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_CuentaBancariaId",
                table: "Transaccion",
                column: "CuentaBancariaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaId",
                table: "Transaccion",
                column: "CuentaBancariaId",
                principalTable: "CuentaBancaria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_CuentaBancaria_CuentaBancariaId",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_CuentaBancariaId",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "CuentaBancariaId",
                table: "Transaccion");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class updacaj : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CajaId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SaldoAnterior",
                table: "Caja",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SaldoActual",
                table: "Caja",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");

            migrationBuilder.CreateIndex(
                name: "IX_Transaccion_CajaId",
                table: "Transaccion",
                column: "CajaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Caja_CajaId",
                table: "Transaccion",
                column: "CajaId",
                principalTable: "Caja",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Caja_CajaId",
                table: "Transaccion");

            migrationBuilder.DropIndex(
                name: "IX_Transaccion_CajaId",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "CajaId",
                table: "Transaccion");

            migrationBuilder.AlterColumn<decimal>(
                name: "SaldoAnterior",
                table: "Caja",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "SaldoActual",
                table: "Caja",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class correpcion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Caja_Egreso_EgresoId",
                table: "Caja");

            migrationBuilder.DropForeignKey(
                name: "FK_Caja_Ingreso_IngresoId",
                table: "Caja");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Egreso_EgresoId",
                table: "Transaccion");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Ingreso_IngresoId",
                table: "Transaccion");

            migrationBuilder.AlterColumn<Guid>(
                name: "IngresoId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EgresoId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SocioInversionId",
                table: "Ingreso",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrestamoDetalleId",
                table: "Ingreso",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "IngresoId",
                table: "Caja",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "EgresoId",
                table: "Caja",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Caja_Egreso_EgresoId",
                table: "Caja",
                column: "EgresoId",
                principalTable: "Egreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Caja_Ingreso_IngresoId",
                table: "Caja",
                column: "IngresoId",
                principalTable: "Ingreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Egreso_EgresoId",
                table: "Transaccion",
                column: "EgresoId",
                principalTable: "Egreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Ingreso_IngresoId",
                table: "Transaccion",
                column: "IngresoId",
                principalTable: "Ingreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Caja_Egreso_EgresoId",
                table: "Caja");

            migrationBuilder.DropForeignKey(
                name: "FK_Caja_Ingreso_IngresoId",
                table: "Caja");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Egreso_EgresoId",
                table: "Transaccion");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaccion_Ingreso_IngresoId",
                table: "Transaccion");

            migrationBuilder.AlterColumn<Guid>(
                name: "IngresoId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EgresoId",
                table: "Transaccion",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SocioInversionId",
                table: "Ingreso",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrestamoDetalleId",
                table: "Ingreso",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "IngresoId",
                table: "Caja",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EgresoId",
                table: "Caja",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Caja_Egreso_EgresoId",
                table: "Caja",
                column: "EgresoId",
                principalTable: "Egreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Caja_Ingreso_IngresoId",
                table: "Caja",
                column: "IngresoId",
                principalTable: "Ingreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Egreso_EgresoId",
                table: "Transaccion",
                column: "EgresoId",
                principalTable: "Egreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaccion_Ingreso_IngresoId",
                table: "Transaccion",
                column: "IngresoId",
                principalTable: "Ingreso",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

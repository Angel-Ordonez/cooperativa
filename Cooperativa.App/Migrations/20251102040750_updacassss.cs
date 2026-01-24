using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class updacassss : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<Guid>(
            //    name: "UltimaTransaccionId",
            //    table: "Caja",
            //    type: "uniqueidentifier",
            //    nullable: true);

            //migrationBuilder.AddColumn<Guid>(
            //    name: "UltimaTransaccionId1",
            //    table: "Caja",
            //    type: "uniqueidentifier",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Caja_UltimaTransaccionId1",
            //    table: "Caja",
            //    column: "UltimaTransaccionId1");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Caja_Transaccion_UltimaTransaccionId1",
            //    table: "Caja",
            //    column: "UltimaTransaccionId1",
            //    principalTable: "Transaccion",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Caja_Transaccion_UltimaTransaccionId1",
            //    table: "Caja");

            //migrationBuilder.DropIndex(
            //    name: "IX_Caja_UltimaTransaccionId1",
            //    table: "Caja");

            //migrationBuilder.DropColumn(
            //    name: "UltimaTransaccionId",
            //    table: "Caja");

            //migrationBuilder.DropColumn(
            //    name: "UltimaTransaccionId1",
            //    table: "Caja");
        }
    }
}

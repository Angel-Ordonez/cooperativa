using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class dsassa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaldoCajaEnElMomento",
                table: "Transaccion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Modulo",
                table: "Caja",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Modulo_Descripcion",
                table: "Caja",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoCajaEnElMomento",
                table: "Transaccion");

            migrationBuilder.DropColumn(
                name: "Modulo",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "Modulo_Descripcion",
                table: "Caja");
        }
    }
}

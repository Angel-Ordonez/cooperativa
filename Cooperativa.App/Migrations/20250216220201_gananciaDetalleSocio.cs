using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class gananciaDetalleSocio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GananciaDetalleSocioId",
                table: "DetalleSocioInversion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GananciaDetalleSocioId1",
                table: "DetalleSocioInversion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GananciaDetalleSocio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeCantidad = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    Ganancia = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    Retirado = table.Column<bool>(type: "bit", nullable: false),
                    CantidadRetirada = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    CantidadDisponibleARetirar = table.Column<decimal>(type: "decimal(9,3)", nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetalleSocioInversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GananciaDetalleSocioAnteriorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GananciaDetalleSocio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GananciaDetalleSocio_DetalleSocioInversion_DetalleSocioInversionId",
                        column: x => x.DetalleSocioInversionId,
                        principalTable: "DetalleSocioInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GananciaDetalleSocio_GananciaDetalleSocio_GananciaDetalleSocioAnteriorId",
                        column: x => x.GananciaDetalleSocioAnteriorId,
                        principalTable: "GananciaDetalleSocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSocioInversion_GananciaDetalleSocioId1",
                table: "DetalleSocioInversion",
                column: "GananciaDetalleSocioId1");

            migrationBuilder.CreateIndex(
                name: "IX_GananciaDetalleSocio_DetalleSocioInversionId",
                table: "GananciaDetalleSocio",
                column: "DetalleSocioInversionId");

            migrationBuilder.CreateIndex(
                name: "IX_GananciaDetalleSocio_GananciaDetalleSocioAnteriorId",
                table: "GananciaDetalleSocio",
                column: "GananciaDetalleSocioAnteriorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleSocioInversion_GananciaDetalleSocio_GananciaDetalleSocioId1",
                table: "DetalleSocioInversion",
                column: "GananciaDetalleSocioId1",
                principalTable: "GananciaDetalleSocio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleSocioInversion_GananciaDetalleSocio_GananciaDetalleSocioId1",
                table: "DetalleSocioInversion");

            migrationBuilder.DropTable(
                name: "GananciaDetalleSocio");

            migrationBuilder.DropIndex(
                name: "IX_DetalleSocioInversion_GananciaDetalleSocioId1",
                table: "DetalleSocioInversion");

            migrationBuilder.DropColumn(
                name: "GananciaDetalleSocioId",
                table: "DetalleSocioInversion");

            migrationBuilder.DropColumn(
                name: "GananciaDetalleSocioId1",
                table: "DetalleSocioInversion");
        }
    }
}

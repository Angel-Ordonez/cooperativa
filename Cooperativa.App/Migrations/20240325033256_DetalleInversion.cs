using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class DetalleInversion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CantidadPrestada",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NoPrestado",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PorcetajePrestado",
                table: "SocioInversion",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "DetalleInversion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SocioInversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Habia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SePresto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quedan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SocioInversionId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleInversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleInversion_Prestamo_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleInversion_SocioInversion_SocioInversionId",
                        column: x => x.SocioInversionId,
                        principalTable: "SocioInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleInversion_SocioInversion_SocioInversionId1",
                        column: x => x.SocioInversionId1,
                        principalTable: "SocioInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleInversion_PrestamoId",
                table: "DetalleInversion",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleInversion_SocioInversionId",
                table: "DetalleInversion",
                column: "SocioInversionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleInversion_SocioInversionId1",
                table: "DetalleInversion",
                column: "SocioInversionId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleInversion");

            migrationBuilder.DropColumn(
                name: "CantidadPrestada",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "NoPrestado",
                table: "SocioInversion");

            migrationBuilder.DropColumn(
                name: "PorcetajePrestado",
                table: "SocioInversion");
        }
    }
}

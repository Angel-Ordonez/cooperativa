using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cooperativa.App.Migrations
{
    public partial class confPre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleInversion");

            migrationBuilder.CreateTable(
                name: "ConfiguracionPrestamo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TomarSocioInversion = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionPrestamo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DetalleSocioInversion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SocioInversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Habia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SePresto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quedan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleSocioInversion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleSocioInversion_Prestamo_PrestamoId",
                        column: x => x.PrestamoId,
                        principalTable: "Prestamo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleSocioInversion_SocioInversion_SocioInversionId",
                        column: x => x.SocioInversionId,
                        principalTable: "SocioInversion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSocioInversion_PrestamoId",
                table: "DetalleSocioInversion",
                column: "PrestamoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleSocioInversion_SocioInversionId",
                table: "DetalleSocioInversion",
                column: "SocioInversionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionPrestamo");

            migrationBuilder.DropTable(
                name: "DetalleSocioInversion");

            migrationBuilder.CreateTable(
                name: "DetalleInversion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Habia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSoftDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrestamoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quedan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SePresto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SocioInversionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SocioInversionId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
    }
}

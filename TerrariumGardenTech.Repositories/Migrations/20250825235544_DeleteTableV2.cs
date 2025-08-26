using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTableV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AISuggestLayout");

            migrationBuilder.DropTable(
                name: "LayoutTerrarium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AISuggestLayout",
                columns: table => new
                {
                    layoutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    layoutData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AISugges__023A37EFFA219D01", x => x.layoutId);
                    table.ForeignKey(
                        name: "FK_AISuggestLayout_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "LayoutTerrarium",
                columns: table => new
                {
                    layoutTerrariumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    terrariumVariantId = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    layoutData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LayoutTe__ED2AF5EA0034035F", x => x.layoutTerrariumId);
                    table.ForeignKey(
                        name: "FK_LayoutTerrarium_TerrariumVariant",
                        column: x => x.terrariumVariantId,
                        principalTable: "TerrariumVariant",
                        principalColumn: "terrariumVariantId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AISuggestLayout_userId",
                table: "AISuggestLayout",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_LayoutTerrarium_terrariumVariantId",
                table: "LayoutTerrarium",
                column: "terrariumVariantId");
        }
    }
}

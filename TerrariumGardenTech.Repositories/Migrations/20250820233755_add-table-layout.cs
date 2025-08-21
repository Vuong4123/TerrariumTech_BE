using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addtablelayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeneratedByAI",
                table: "Terrariums",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TerrariumLayouts",
                columns: table => new
                {
                    LayoutId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LayoutName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerrariumId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumLayouts", x => x.LayoutId);
                    table.ForeignKey(
                        name: "FK_TerrariumLayout_Reviewer",
                        column: x => x.ReviewedBy,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TerrariumLayout_Terrarium",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumLayout_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumLayouts_ReviewedBy",
                table: "TerrariumLayouts",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumLayouts_TerrariumId",
                table: "TerrariumLayouts",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumLayouts_UserId",
                table: "TerrariumLayouts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerrariumLayouts");

            migrationBuilder.DropColumn(
                name: "GeneratedByAI",
                table: "Terrariums");
        }
    }
}

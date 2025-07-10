using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB872025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Terrariums_Accessory_AccessoryId",
                table: "Terrariums");

            migrationBuilder.DropIndex(
                name: "IX_Terrariums_AccessoryId",
                table: "Terrariums");

            migrationBuilder.DropColumn(
                name: "AccessoryId",
                table: "Terrariums");

            migrationBuilder.AddColumn<string>(
                name: "bodyHTML",
                table: "Blog",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bodyHTML",
                table: "Blog");

            migrationBuilder.AddColumn<int>(
                name: "AccessoryId",
                table: "Terrariums",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Terrariums_AccessoryId",
                table: "Terrariums",
                column: "AccessoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Terrariums_Accessory_AccessoryId",
                table: "Terrariums",
                column: "AccessoryId",
                principalTable: "Accessory",
                principalColumn: "accessoryId");
        }
    }
}

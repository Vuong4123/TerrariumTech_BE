using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB187 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "altText",
                table: "TerrariumImage");

            migrationBuilder.DropColumn(
                name: "isPrimary",
                table: "TerrariumImage");

            migrationBuilder.DropColumn(
                name: "altText",
                table: "AccessoryImage");

            migrationBuilder.DropColumn(
                name: "isPrimary",
                table: "AccessoryImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "altText",
                table: "TerrariumImage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isPrimary",
                table: "TerrariumImage",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "altText",
                table: "AccessoryImage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isPrimary",
                table: "AccessoryImage",
                type: "bit",
                nullable: true,
                defaultValue: false);
        }
    }
}

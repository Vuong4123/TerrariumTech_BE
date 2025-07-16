using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB167V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShapeHeight",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "ShapeLength",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "ShapeSize",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "ShapeVolume",
                table: "Shapes");

            migrationBuilder.DropColumn(
                name: "ShapeWidth",
                table: "Shapes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShapeHeight",
                table: "Shapes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShapeLength",
                table: "Shapes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShapeSize",
                table: "Shapes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "ShapeVolume",
                table: "Shapes",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "ShapeWidth",
                table: "Shapes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB247 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Shape",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "TankMethod",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Personalizes");

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentId",
                table: "Personalizes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShapeId",
                table: "Personalizes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TankMethodId",
                table: "Personalizes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Personalizes_EnvironmentId",
                table: "Personalizes",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Personalizes_ShapeId",
                table: "Personalizes",
                column: "ShapeId");

            migrationBuilder.CreateIndex(
                name: "IX_Personalizes_TankMethodId",
                table: "Personalizes",
                column: "TankMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personalizes_Environments_EnvironmentId",
                table: "Personalizes",
                column: "EnvironmentId",
                principalTable: "Environments",
                principalColumn: "EnvironmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Personalizes_Shapes_ShapeId",
                table: "Personalizes",
                column: "ShapeId",
                principalTable: "Shapes",
                principalColumn: "ShapeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Personalizes_TankMethods_TankMethodId",
                table: "Personalizes",
                column: "TankMethodId",
                principalTable: "TankMethods",
                principalColumn: "TankMethodId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personalizes_Environments_EnvironmentId",
                table: "Personalizes");

            migrationBuilder.DropForeignKey(
                name: "FK_Personalizes_Shapes_ShapeId",
                table: "Personalizes");

            migrationBuilder.DropForeignKey(
                name: "FK_Personalizes_TankMethods_TankMethodId",
                table: "Personalizes");

            migrationBuilder.DropIndex(
                name: "IX_Personalizes_EnvironmentId",
                table: "Personalizes");

            migrationBuilder.DropIndex(
                name: "IX_Personalizes_ShapeId",
                table: "Personalizes");

            migrationBuilder.DropIndex(
                name: "IX_Personalizes_TankMethodId",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "EnvironmentId",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "ShapeId",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "TankMethodId",
                table: "Personalizes");

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TankMethod",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

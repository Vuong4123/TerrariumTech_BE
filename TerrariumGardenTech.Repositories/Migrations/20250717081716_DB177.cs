using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB177 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerrariumEnvironments");

            migrationBuilder.DropTable(
                name: "TerrariumShapes");

            migrationBuilder.DropTable(
                name: "TerrariumTankMethods");

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentId",
                table: "Terrariums",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShapeId",
                table: "Terrariums",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TankMethodId",
                table: "Terrariums",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Terrariums_EnvironmentId",
                table: "Terrariums",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Terrariums_ShapeId",
                table: "Terrariums",
                column: "ShapeId");

            migrationBuilder.CreateIndex(
                name: "IX_Terrariums_TankMethodId",
                table: "Terrariums",
                column: "TankMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Terrariums_Environments_EnvironmentId",
                table: "Terrariums",
                column: "EnvironmentId",
                principalTable: "Environments",
                principalColumn: "EnvironmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Terrariums_Shapes_ShapeId",
                table: "Terrariums",
                column: "ShapeId",
                principalTable: "Shapes",
                principalColumn: "ShapeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Terrariums_TankMethods_TankMethodId",
                table: "Terrariums",
                column: "TankMethodId",
                principalTable: "TankMethods",
                principalColumn: "TankMethodId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Terrariums_Environments_EnvironmentId",
                table: "Terrariums");

            migrationBuilder.DropForeignKey(
                name: "FK_Terrariums_Shapes_ShapeId",
                table: "Terrariums");

            migrationBuilder.DropForeignKey(
                name: "FK_Terrariums_TankMethods_TankMethodId",
                table: "Terrariums");

            migrationBuilder.DropIndex(
                name: "IX_Terrariums_EnvironmentId",
                table: "Terrariums");

            migrationBuilder.DropIndex(
                name: "IX_Terrariums_ShapeId",
                table: "Terrariums");

            migrationBuilder.DropIndex(
                name: "IX_Terrariums_TankMethodId",
                table: "Terrariums");

            migrationBuilder.DropColumn(
                name: "EnvironmentId",
                table: "Terrariums");

            migrationBuilder.DropColumn(
                name: "ShapeId",
                table: "Terrariums");

            migrationBuilder.DropColumn(
                name: "TankMethodId",
                table: "Terrariums");

            migrationBuilder.CreateTable(
                name: "TerrariumEnvironments",
                columns: table => new
                {
                    TerrariumEnvironmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvironmentId = table.Column<int>(type: "int", nullable: false),
                    TerrariumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumEnvironments", x => x.TerrariumEnvironmentId);
                    table.ForeignKey(
                        name: "FK_TerrariumEnvironments_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "EnvironmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumEnvironments_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerrariumShapes",
                columns: table => new
                {
                    TerrariumShapeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShapeId = table.Column<int>(type: "int", nullable: false),
                    TerrariumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumShapes", x => x.TerrariumShapeId);
                    table.ForeignKey(
                        name: "FK_TerrariumShapes_Shapes_ShapeId",
                        column: x => x.ShapeId,
                        principalTable: "Shapes",
                        principalColumn: "ShapeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumShapes_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerrariumTankMethods",
                columns: table => new
                {
                    TerrariumTankMethodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TankMethodId = table.Column<int>(type: "int", nullable: false),
                    TerrariumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerrariumTankMethods", x => x.TerrariumTankMethodId);
                    table.ForeignKey(
                        name: "FK_TerrariumTankMethods_TankMethods_TankMethodId",
                        column: x => x.TankMethodId,
                        principalTable: "TankMethods",
                        principalColumn: "TankMethodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerrariumTankMethods_Terrariums_TerrariumId",
                        column: x => x.TerrariumId,
                        principalTable: "Terrariums",
                        principalColumn: "TerrariumId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumEnvironments_EnvironmentId",
                table: "TerrariumEnvironments",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumEnvironments_TerrariumId",
                table: "TerrariumEnvironments",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumShapes_ShapeId",
                table: "TerrariumShapes",
                column: "ShapeId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumShapes_TerrariumId",
                table: "TerrariumShapes",
                column: "TerrariumId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumTankMethods_TankMethodId",
                table: "TerrariumTankMethods",
                column: "TankMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_TerrariumTankMethods_TerrariumId",
                table: "TerrariumTankMethods",
                column: "TerrariumId");
        }
    }
}

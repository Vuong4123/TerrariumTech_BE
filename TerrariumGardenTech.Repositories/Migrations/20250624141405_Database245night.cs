using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Database245night : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personalize_User",
                table: "Personalize");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Personal__8032E374C56CF71E",
                table: "Personalize");

            migrationBuilder.RenameTable(
                name: "Personalize",
                newName: "Personalizes");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Personalizes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "theme",
                table: "Personalizes",
                newName: "Theme");

            migrationBuilder.RenameColumn(
                name: "language",
                table: "Personalizes",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "personalizeId",
                table: "Personalizes",
                newName: "PersonalizeId");

            migrationBuilder.RenameColumn(
                name: "preferences",
                table: "Personalizes",
                newName: "Type");

            migrationBuilder.RenameIndex(
                name: "IX_Personalize_userId",
                table: "Personalizes",
                newName: "IX_Personalizes_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Theme",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Language",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shape",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TankMethod",
                table: "Personalizes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Personalizes",
                table: "Personalizes",
                column: "PersonalizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personalizes_User_UserId",
                table: "Personalizes",
                column: "UserId",
                principalTable: "User",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personalizes_User_UserId",
                table: "Personalizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Personalizes",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "Shape",
                table: "Personalizes");

            migrationBuilder.DropColumn(
                name: "TankMethod",
                table: "Personalizes");

            migrationBuilder.RenameTable(
                name: "Personalizes",
                newName: "Personalize");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Personalize",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "Theme",
                table: "Personalize",
                newName: "theme");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "Personalize",
                newName: "language");

            migrationBuilder.RenameColumn(
                name: "PersonalizeId",
                table: "Personalize",
                newName: "personalizeId");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Personalize",
                newName: "preferences");

            migrationBuilder.RenameIndex(
                name: "IX_Personalizes_UserId",
                table: "Personalize",
                newName: "IX_Personalize_userId");

            migrationBuilder.AlterColumn<string>(
                name: "theme",
                table: "Personalize",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "language",
                table: "Personalize",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Personal__8032E374C56CF71E",
                table: "Personalize",
                column: "personalizeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Personalize_User",
                table: "Personalize",
                column: "userId",
                principalTable: "User",
                principalColumn: "userId");
        }
    }
}

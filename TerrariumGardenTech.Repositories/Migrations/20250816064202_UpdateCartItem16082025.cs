using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartItem16082025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "isRead",
                table: "Notification",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentCartItemId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ParentCartItemId",
                table: "CartItems",
                column: "ParentCartItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_CartItems_ParentCartItemId",
                table: "CartItems",
                column: "ParentCartItemId",
                principalTable: "CartItems",
                principalColumn: "CartItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_CartItems_ParentCartItemId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ParentCartItemId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ParentCartItemId",
                table: "CartItems");

            migrationBuilder.AlterColumn<bool>(
                name: "isRead",
                table: "Notification",
                type: "bit",
                nullable: true,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}

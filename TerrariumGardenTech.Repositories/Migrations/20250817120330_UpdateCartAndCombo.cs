using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartAndCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "OrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComboSnapshot",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComboCategory",
                columns: table => new
                {
                    ComboCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboCategory", x => x.ComboCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Combo",
                columns: table => new
                {
                    ComboId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComboPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    SoldQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combo", x => x.ComboId);
                    table.ForeignKey(
                        name: "FK_Combo_ComboCategory_ComboCategoryId",
                        column: x => x.ComboCategoryId,
                        principalTable: "ComboCategory",
                        principalColumn: "ComboCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComboItem",
                columns: table => new
                {
                    ComboItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComboId = table.Column<int>(type: "int", nullable: false),
                    TerrariumVariantId = table.Column<int>(type: "int", nullable: true),
                    AccessoryId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboItem", x => x.ComboItemId);
                    table.ForeignKey(
                        name: "FK_ComboItem_Accessory_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessory",
                        principalColumn: "accessoryId");
                    table.ForeignKey(
                        name: "FK_ComboItem_Combo_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combo",
                        principalColumn: "ComboId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboItem_TerrariumVariant_TerrariumVariantId",
                        column: x => x.TerrariumVariantId,
                        principalTable: "TerrariumVariant",
                        principalColumn: "terrariumVariantId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_ComboId",
                table: "OrderItem",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ComboId",
                table: "Carts",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ComboId",
                table: "CartItems",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_Combo_ComboCategoryId",
                table: "Combo",
                column: "ComboCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboItem_AccessoryId",
                table: "ComboItem",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboItem_ComboId",
                table: "ComboItem",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboItem_TerrariumVariantId",
                table: "ComboItem",
                column: "TerrariumVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Combo_ComboId",
                table: "CartItems",
                column: "ComboId",
                principalTable: "Combo",
                principalColumn: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Combo_ComboId",
                table: "Carts",
                column: "ComboId",
                principalTable: "Combo",
                principalColumn: "ComboId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Combo_ComboId",
                table: "OrderItem",
                column: "ComboId",
                principalTable: "Combo",
                principalColumn: "ComboId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Combo_ComboId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Combo_ComboId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Combo_ComboId",
                table: "OrderItem");

            migrationBuilder.DropTable(
                name: "ComboItem");

            migrationBuilder.DropTable(
                name: "Combo");

            migrationBuilder.DropTable(
                name: "ComboCategory");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_ComboId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_Carts_ComboId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ComboId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ComboSnapshot",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "CartItems");
        }
    }
}

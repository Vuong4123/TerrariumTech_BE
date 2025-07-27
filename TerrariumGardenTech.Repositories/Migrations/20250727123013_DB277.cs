using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB277 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressDeliveryId",
                table: "ShippingDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingStatus",
                table: "ShippingDetail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessoryQuantity",
                table: "OrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TerrariumVariantQuantity",
                table: "OrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingDetail_AddressDeliveryId",
                table: "ShippingDetail",
                column: "AddressDeliveryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingDetail_AddressDelivery_AddressDeliveryId",
                table: "ShippingDetail",
                column: "AddressDeliveryId",
                principalTable: "AddressDelivery",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingDetail_AddressDelivery_AddressDeliveryId",
                table: "ShippingDetail");

            migrationBuilder.DropIndex(
                name: "IX_ShippingDetail_AddressDeliveryId",
                table: "ShippingDetail");

            migrationBuilder.DropColumn(
                name: "AddressDeliveryId",
                table: "ShippingDetail");

            migrationBuilder.DropColumn(
                name: "ShippingStatus",
                table: "ShippingDetail");

            migrationBuilder.DropColumn(
                name: "AccessoryQuantity",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "TerrariumVariantQuantity",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Order");
        }
    }
}

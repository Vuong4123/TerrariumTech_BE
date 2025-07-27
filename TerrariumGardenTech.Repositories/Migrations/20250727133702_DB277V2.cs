using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB277V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingDetail");

            migrationBuilder.DropTable(
                name: "AddressDelivery");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressDelivery",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    createdOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    modifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    receiverAddress = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    receiverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    receiverPhone = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    userId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDelivery", x => x.id);
                    table.ForeignKey(
                        name: "FK_AddressDelivery_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShippingDetail",
                columns: table => new
                {
                    shippingDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressDeliveryId = table.Column<int>(type: "int", nullable: true),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    estimatedDeliveryDate = table.Column<DateTime>(type: "date", nullable: true),
                    shippingCost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    shippingMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShippingStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    trackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Shipping__DDF63975E50C620D", x => x.shippingDetailId);
                    table.ForeignKey(
                        name: "FK_ShippingDetail_AddressDelivery_AddressDeliveryId",
                        column: x => x.AddressDeliveryId,
                        principalTable: "AddressDelivery",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ShippingDetail_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressDelivery_orderId",
                table: "AddressDelivery",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingDetail_AddressDeliveryId",
                table: "ShippingDetail",
                column: "AddressDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingDetail_orderId",
                table: "ShippingDetail",
                column: "orderId");
        }
    }
}

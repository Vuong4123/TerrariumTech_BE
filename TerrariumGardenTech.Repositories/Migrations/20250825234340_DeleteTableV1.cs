using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTableV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnExchangeRequestItem");

            migrationBuilder.DropTable(
                name: "ReturnExchangeRequest");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnExchangeRequest",
                columns: table => new
                {
                    requestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderId = table.Column<int>(type: "int", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requestDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(sysutcdatetime())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReturnEx__E3C5DE3141AB3D28", x => x.requestId);
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequest_Order",
                        column: x => x.orderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequest_User",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                });

            migrationBuilder.CreateTable(
                name: "ReturnExchangeRequestItem",
                columns: table => new
                {
                    requestItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderItemId = table.Column<int>(type: "int", nullable: false),
                    requestId = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true, defaultValue: "pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ReturnEx__FDD6A58FF74806E9", x => x.requestItemId);
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequestItem_OrderItem",
                        column: x => x.orderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "orderItemId");
                    table.ForeignKey(
                        name: "FK_ReturnExchangeRequestItem_Request",
                        column: x => x.requestId,
                        principalTable: "ReturnExchangeRequest",
                        principalColumn: "requestId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequest_orderId",
                table: "ReturnExchangeRequest",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequest_userId",
                table: "ReturnExchangeRequest",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequestItem_orderItemId",
                table: "ReturnExchangeRequestItem",
                column: "orderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnExchangeRequestItem_requestId",
                table: "ReturnExchangeRequestItem",
                column: "requestId");
        }
    }
}

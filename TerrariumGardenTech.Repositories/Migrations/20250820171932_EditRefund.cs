using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EditRefund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPoint",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "ReasonModified",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "OrderRequestRefund");

            migrationBuilder.CreateTable(
                name: "OrderRefundItemId",
                columns: table => new
                {
                    OrderRefundItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RefundPoint = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReasonModified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserModified = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true),
                    OrderRefundId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRefundItemId", x => x.OrderRefundItemId);
                    table.ForeignKey(
                        name: "FK_OrderRefundItem_OrderRefundId",
                        column: x => x.OrderRefundId,
                        principalTable: "OrderRequestRefund",
                        principalColumn: "RequestRefundId");
                });

            migrationBuilder.CreateTable(
                name: "OrderTransportItem",
                columns: table => new
                {
                    TransportItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TransportId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTransportItem", x => x.TransportItemId);
                    table.ForeignKey(
                        name: "FK_OrderTransportItem_OrderTransport",
                        column: x => x.TransportId,
                        principalTable: "OrderTransport",
                        principalColumn: "TransportId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderRefundItemId_OrderRefundId",
                table: "OrderRefundItemId",
                column: "OrderRefundId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransportItem_TransportId",
                table: "OrderTransportItem",
                column: "TransportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderRefundItemId");

            migrationBuilder.DropTable(
                name: "OrderTransportItem");

            migrationBuilder.AddColumn<bool>(
                name: "IsPoint",
                table: "OrderRequestRefund",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "OrderRequestRefund",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonModified",
                table: "OrderRequestRefund",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "OrderRequestRefund",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserModified",
                table: "OrderRequestRefund",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

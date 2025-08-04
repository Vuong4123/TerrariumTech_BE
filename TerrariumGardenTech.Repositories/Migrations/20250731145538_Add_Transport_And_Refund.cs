using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Add_Transport_And_Refund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shippingStatus",
                table: "Order");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Order",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "pending");

            migrationBuilder.CreateTable(
                name: "OrderRequestRefund",
                columns: table => new
                {
                    RequestRefundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonModified = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserModified = table.Column<int>(type: "int", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPoint = table.Column<bool>(type: "bit", nullable: false),
                    TransportId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRequestRefund", x => x.RequestRefundId);
                    table.ForeignKey(
                        name: "FK_OrderRequestRefund_Order",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateTable(
                name: "OrderTransport",
                columns: table => new
                {
                    TransportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstimateCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastConfirmFailed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContactFailNumber = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRefund = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AssignShipperTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTransport", x => x.TransportId);
                    table.ForeignKey(
                        name: "FK_OrderTransport_Order",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "orderId");
                });

            migrationBuilder.CreateTable(
                name: "TransportLog",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderTransportId = table.Column<int>(type: "int", nullable: false),
                    LastStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldUser = table.Column<int>(type: "int", nullable: true),
                    CurrentUser = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckinImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportLog", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_TransportLog_OrderTransport",
                        column: x => x.OrderTransportId,
                        principalTable: "OrderTransport",
                        principalColumn: "TransportId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderRequestRefund_OrderId",
                table: "OrderRequestRefund",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTransport_OrderId",
                table: "OrderTransport",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportLog_OrderTransportId",
                table: "TransportLog",
                column: "OrderTransportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderRequestRefund");

            migrationBuilder.DropTable(
                name: "TransportLog");

            migrationBuilder.DropTable(
                name: "OrderTransport");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "shippingStatus",
                table: "Order",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "pending");
        }
    }
}

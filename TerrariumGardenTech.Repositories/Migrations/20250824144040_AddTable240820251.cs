using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddTable240820251 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "OrderRequestRefund",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestRefundId",
                table: "OrderRequestRefund",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "IsPoint",
                table: "OrderRequestRefund",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "OrderRequestRefund",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "OrderRequestRefund",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "OrderRequestRefund",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderRequestRefund",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TransportId",
                table: "OrderRequestRefund",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserModified",
                table: "OrderRequestRefund",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderRequestRefund",
                table: "OrderRequestRefund",
                column: "RequestRefundId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderRequestRefund_OrderId",
                table: "OrderRequestRefund",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderRequestRefund",
                table: "OrderRequestRefund");

            migrationBuilder.DropIndex(
                name: "IX_OrderRequestRefund_OrderId",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "RequestRefundId",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "IsPoint",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "Notes",
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
                name: "RequestDate",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "TransportId",
                table: "OrderRequestRefund");

            migrationBuilder.DropColumn(
                name: "UserModified",
                table: "OrderRequestRefund");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "OrderRequestRefund",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

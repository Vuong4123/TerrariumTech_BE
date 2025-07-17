using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DeletedOrderItemDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItemDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderItemDetail",
                columns: table => new
                {
                    orderItemDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderItemId = table.Column<int>(type: "int", nullable: false),
                    detailKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    detailValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OrderIte__098BB1314F361970", x => x.orderItemDetailId);
                    table.ForeignKey(
                        name: "FK_OrderItemDetail_OrderItem",
                        column: x => x.orderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "orderItemId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemDetail_orderItemId",
                table: "OrderItemDetail",
                column: "orderItemId");
        }
    }
}

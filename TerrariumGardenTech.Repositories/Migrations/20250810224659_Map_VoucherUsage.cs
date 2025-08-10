using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Map_VoucherUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VoucherUsage",
                columns: table => new
                {
                    VoucherId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherUsage", x => new { x.VoucherId, x.UserId });
                    table.ForeignKey(
                        name: "FK_VoucherUsage_Voucher_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "Voucher",
                        principalColumn: "voucherId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoucherUsage");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class Add_VoucherUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tạo bảng VoucherUsage nếu chưa có
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[VoucherUsage]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[VoucherUsage](
        [VoucherId] int NOT NULL,
        [UserId]    nvarchar(64) NOT NULL,
        [UsedCount] int NOT NULL CONSTRAINT [DF_VoucherUsage_UsedCount] DEFAULT(0),
        CONSTRAINT [PK_VoucherUsage] PRIMARY KEY ([VoucherId],[UserId]),
        CONSTRAINT [FK_VoucherUsage_Voucher_VoucherId]
            FOREIGN KEY([VoucherId]) REFERENCES [dbo].[Voucher]([VoucherId]) ON DELETE CASCADE
    );

    -- (tuỳ chọn) index phụ để tra cứu theo User nhanh hơn trong hệ lớn
    CREATE INDEX [IX_VoucherUsage_UserId] ON [dbo].[VoucherUsage]([UserId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xoá bảng an toàn nếu tồn tại
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[VoucherUsage]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[VoucherUsage];
END
");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== Voucher: thêm cột =====
            migrationBuilder.AddColumn<bool>(
                name: "IsPersonal",
                table: "Voucher",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PerUserUsageLimit",
                table: "Voucher",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemainingUsage",
                table: "Voucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TargetUserId",
                table: "Voucher",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalUsage",
                table: "Voucher",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // ===== Index cho Voucher (nếu schema có các cột này) =====
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.[Voucher]', 'Status') IS NOT NULL
AND COL_LENGTH('dbo.[Voucher]', 'ValidFrom') IS NOT NULL
AND COL_LENGTH('dbo.[Voucher]', 'ValidTo') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Voucher_Status_ValidFrom_ValidTo' AND object_id = OBJECT_ID('dbo.[Voucher]'))
BEGIN
    CREATE INDEX [IX_Voucher_Status_ValidFrom_ValidTo]
    ON [dbo].[Voucher]([Status],[ValidFrom],[ValidTo]);
END
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Voucher_IsPersonal_TargetUserId' AND object_id = OBJECT_ID('dbo.[Voucher]'))
BEGIN
    CREATE INDEX [IX_Voucher_IsPersonal_TargetUserId]
    ON [dbo].[Voucher]([IsPersonal],[TargetUserId]);
END
");

            // ===== CHECK constraints (idempotent) =====
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Voucher_TotalUsage_NonNegative')
    ALTER TABLE [dbo].[Voucher] ADD CONSTRAINT [CK_Voucher_TotalUsage_NonNegative] CHECK ([TotalUsage] >= 0);
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Voucher_RemainingUsage_NonNegative')
    ALTER TABLE [dbo].[Voucher] ADD CONSTRAINT [CK_Voucher_RemainingUsage_NonNegative] CHECK ([RemainingUsage] >= 0);
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Voucher_Remaining_LTE_Total')
    ALTER TABLE [dbo].[Voucher] ADD CONSTRAINT [CK_Voucher_Remaining_LTE_Total] CHECK ([RemainingUsage] <= [TotalUsage]);
");

            // (Tuỳ chọn) Backfill RemainingUsage = TotalUsage khi hợp lý
            migrationBuilder.Sql(@"
UPDATE V
SET RemainingUsage = TotalUsage
FROM [dbo].[Voucher] V
WHERE V.TotalUsage > 0 AND (V.RemainingUsage = 0 OR V.RemainingUsage IS NULL);
");

            // ===== WALLETs: idempotent =====
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Wallets]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Wallets](
        [WalletId]   int            NOT NULL IDENTITY(1,1),
        [UserId]     int            NOT NULL,
        [Balance]    decimal(18,2)  NOT NULL,
        [WalletType] nvarchar(max)  NOT NULL,
        CONSTRAINT [PK_Wallets] PRIMARY KEY ([WalletId])
    );
END
");

            // ===== WALLET TRANSACTION: idempotent =====
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[WalletTransaction]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[WalletTransaction](
        [TransactionId] int           NOT NULL IDENTITY(1,1),
        [WalletId]      int           NOT NULL,
        [Amount]        decimal(18,2) NOT NULL,
        [Type]          nvarchar(max) NOT NULL,
        [CreatedDate]   datetime2     NOT NULL,
        [OrderId]       int           NULL,
        CONSTRAINT [PK_WalletTransaction] PRIMARY KEY ([TransactionId]),
        CONSTRAINT [FK_WalletTransaction_Wallets_WalletId]
            FOREIGN KEY([WalletId]) REFERENCES [dbo].[Wallets]([WalletId]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_WalletTransaction_WalletId]
        ON [dbo].[WalletTransaction]([WalletId]);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // WalletTransaction & Wallets: drop an toàn
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[WalletTransaction]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[WalletTransaction];
END
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Wallets]', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Wallets];
END
");

            // Xoá index Voucher nếu tồn tại
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Voucher_Status_ValidFrom_ValidTo' AND object_id = OBJECT_ID('dbo.[Voucher]'))
    DROP INDEX [IX_Voucher_Status_ValidFrom_ValidTo] ON [dbo].[Voucher];
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Voucher_IsPersonal_TargetUserId' AND object_id = OBJECT_ID('dbo.[Voucher]'))
    DROP INDEX [IX_Voucher_IsPersonal_TargetUserId] ON [dbo].[Voucher];
");

            // Bỏ CHECK constraints nếu tồn tại
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Voucher_TotalUsage_NonNegative') ALTER TABLE [dbo].[Voucher] DROP CONSTRAINT [CK_Voucher_TotalUsage_NonNegative];");
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Voucher_RemainingUsage_NonNegative') ALTER TABLE [dbo].[Voucher] DROP CONSTRAINT [CK_Voucher_RemainingUsage_NonNegative];");
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Voucher_Remaining_LTE_Total') ALTER TABLE [dbo].[Voucher] DROP CONSTRAINT [CK_Voucher_Remaining_LTE_Total];");

            // Gỡ cột Voucher (nếu tồn tại)
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.[Voucher]', 'IsPersonal') IS NOT NULL ALTER TABLE [dbo].[Voucher] DROP COLUMN [IsPersonal];");
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.[Voucher]', 'PerUserUsageLimit') IS NOT NULL ALTER TABLE [dbo].[Voucher] DROP COLUMN [PerUserUsageLimit];");
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.[Voucher]', 'RemainingUsage') IS NOT NULL ALTER TABLE [dbo].[Voucher] DROP COLUMN [RemainingUsage];");
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.[Voucher]', 'TargetUserId') IS NOT NULL ALTER TABLE [dbo].[Voucher] DROP COLUMN [TargetUserId];");
            migrationBuilder.Sql(@"IF COL_LENGTH('dbo.[Voucher]', 'TotalUsage') IS NOT NULL ALTER TABLE [dbo].[Voucher] DROP COLUMN [TotalUsage];");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RestructureMembershipToUsePackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Membership_User",
                table: "Membership");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Membersh__86AA3B174168331C",
                table: "Membership");

            migrationBuilder.DropColumn(
                name: "membershipType",
                table: "Membership");

            migrationBuilder.RenameTable(
                name: "Membership",
                newName: "Memberships");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Memberships",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Memberships",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "startDate",
                table: "Memberships",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "endDate",
                table: "Memberships",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "membershipId",
                table: "Memberships",
                newName: "MembershipId");

            migrationBuilder.RenameIndex(
                name: "IX_Membership_userId",
                table: "Memberships",
                newName: "IX_Memberships_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldDefaultValue: "active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Memberships",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Memberships",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Memberships",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships",
                column: "MembershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_User_UserId",
                table: "Memberships",
                column: "UserId",
                principalTable: "User",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_User_UserId",
                table: "Memberships");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Memberships",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Memberships");

            migrationBuilder.RenameTable(
                name: "Memberships",
                newName: "Membership");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Membership",
                newName: "userId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Membership",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Membership",
                newName: "startDate");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Membership",
                newName: "endDate");

            migrationBuilder.RenameColumn(
                name: "MembershipId",
                table: "Membership",
                newName: "membershipId");

            migrationBuilder.RenameIndex(
                name: "IX_Memberships_UserId",
                table: "Membership",
                newName: "IX_Membership_userId");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Membership",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "active",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "startDate",
                table: "Membership",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "endDate",
                table: "Membership",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "membershipType",
                table: "Membership",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Membersh__86AA3B174168331C",
                table: "Membership",
                column: "membershipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Membership_User",
                table: "Membership",
                column: "userId",
                principalTable: "User",
                principalColumn: "userId");
        }
    }
}

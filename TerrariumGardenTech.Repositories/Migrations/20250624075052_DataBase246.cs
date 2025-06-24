using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DataBase246 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiredDays",
                table: "UserMemberShips");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Memberships");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpriedAt",
                table: "UserMemberShips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartAt",
                table: "UserMemberShips",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Memberships",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpriedAt",
                table: "UserMemberShips");

            migrationBuilder.DropColumn(
                name: "StartAt",
                table: "UserMemberShips");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Memberships");

            migrationBuilder.AddColumn<int>(
                name: "ExpiredDays",
                table: "UserMemberShips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Memberships",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Memberships",
                type: "datetime2",
                nullable: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddOtpResendFieldsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OtpResendCount",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtpSentAt",
                table: "User",
                type: "datetime2",
                nullable: true);

            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "OtpResendCount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "OtpSentAt",
                table: "User");
        }
    }
}

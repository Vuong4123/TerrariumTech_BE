using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB227 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Terrariums",
                newName: "MinPrice");

            migrationBuilder.AlterColumn<int>(
                name: "stockQuantity",
                table: "TerrariumVariant",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TerrariumVariant",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TerrariumVariant",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPrice",
                table: "Terrariums",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TerrariumVariant");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TerrariumVariant");

            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "Terrariums");

            migrationBuilder.RenameColumn(
                name: "MinPrice",
                table: "Terrariums",
                newName: "Price");

            migrationBuilder.AlterColumn<int>(
                name: "stockQuantity",
                table: "TerrariumVariant",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

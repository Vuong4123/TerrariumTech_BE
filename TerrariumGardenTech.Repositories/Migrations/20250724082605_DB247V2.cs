using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariumGardenTech.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DB247V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "altText",
                table: "FeedbackImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "altText",
                table: "FeedbackImage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}

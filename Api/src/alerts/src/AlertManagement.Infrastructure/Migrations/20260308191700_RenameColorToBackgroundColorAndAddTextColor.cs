using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameColorToBackgroundColorAndAddTextColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Color",
                table: "AlertTypes",
                newName: "TextColor");

            migrationBuilder.RenameColumn(
                name: "Color",
                table: "AlertStatusLevels",
                newName: "TextColor");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "AlertTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "AlertStatusLevels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "AlertTypes");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "AlertStatusLevels");

            migrationBuilder.RenameColumn(
                name: "TextColor",
                table: "AlertTypes",
                newName: "Color");

            migrationBuilder.RenameColumn(
                name: "TextColor",
                table: "AlertStatusLevels",
                newName: "Color");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertStatusLevelFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "AlertStatusLevels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "AlertStatusLevels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "AlertStatusLevels");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "AlertStatusLevels");
        }
    }
}

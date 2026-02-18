using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPushNotificationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PushNotificationToken",
                table: "UserDevices",
                type: "TEXT",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushNotificationToken",
                table: "UserDevices");
        }
    }
}

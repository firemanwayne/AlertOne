using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    DeviceType = table.Column<int>(type: "INTEGER", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AppVersion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RefreshToken = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsTrusted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    FirstLoginAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_DeviceId",
                table: "UserDevices",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_RefreshToken",
                table: "UserDevices",
                column: "RefreshToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }
    }
}

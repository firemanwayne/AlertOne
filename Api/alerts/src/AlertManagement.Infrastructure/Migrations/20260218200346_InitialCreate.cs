using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertStatusLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SendNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertStatusLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlertTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlertStatusLevelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RescindedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RescindedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertStatusLevels_AlertStatusLevelId",
                        column: x => x.AlertStatusLevelId,
                        principalTable: "AlertStatusLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertTypes_AlertTypeId",
                        column: x => x.AlertTypeId,
                        principalTable: "AlertTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AlertTypeUsers",
                columns: table => new
                {
                    AlertTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertTypeUsers", x => new { x.AlertTypeId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AlertTypeUsers_AlertTypes_AlertTypeId",
                        column: x => x.AlertTypeId,
                        principalTable: "AlertTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertStatusLevelId",
                table: "Alerts",
                column: "AlertStatusLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertTypeId",
                table: "Alerts",
                column: "AlertTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertStatusLevels_Name",
                table: "AlertStatusLevels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertTypes_Name",
                table: "AlertTypes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "AlertTypeUsers");

            migrationBuilder.DropTable(
                name: "AlertStatusLevels");

            migrationBuilder.DropTable(
                name: "AlertTypes");
        }
    }
}

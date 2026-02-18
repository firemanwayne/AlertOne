using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlertId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ChangedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertHistories_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertHistories_AlertId",
                table: "AlertHistories",
                column: "AlertId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertHistories");
        }
    }
}

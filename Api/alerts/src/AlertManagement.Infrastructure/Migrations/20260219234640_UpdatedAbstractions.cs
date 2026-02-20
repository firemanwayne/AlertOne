using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlertManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedAbstractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "AlertTypes",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AlertTypes",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "AlertStatusLevels",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AlertStatusLevels",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "Alerts",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Alerts",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "ChangedBy",
                table: "AlertHistories",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                table: "AlertHistories",
                newName: "UpdatedDate");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AlertTypeUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AlertTypeUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "AlertTypeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AlertTypeUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AlertTypeUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AlertTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AlertTypes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AlertStatusLevels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AlertStatusLevels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Alerts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AlertHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AlertHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AlertTypeUsers");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AlertTypeUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AlertTypeUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AlertTypeUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AlertTypeUsers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AlertTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AlertTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AlertStatusLevels");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AlertStatusLevels");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AlertHistories");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AlertHistories");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "AlertTypes",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AlertTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "AlertStatusLevels",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "AlertStatusLevels",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Alerts",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Alerts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "AlertHistories",
                newName: "ChangedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AlertHistories",
                newName: "ChangedBy");

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Alerts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }
    }
}

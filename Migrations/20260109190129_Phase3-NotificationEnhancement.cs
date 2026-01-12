using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class Phase3NotificationEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "NotificationMessage",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Recipient",
                table: "NotificationMessage",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "NotificationMessage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSent",
                table: "NotificationMessage",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "NotificationMessage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "NotificationMessage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "NotificationMessage",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessage_UserId",
                table: "NotificationMessage",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessage_Users_UserId",
                table: "NotificationMessage",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessage_Users_UserId",
                table: "NotificationMessage");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessage_UserId",
                table: "NotificationMessage");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "NotificationMessage");

            migrationBuilder.DropColumn(
                name: "IsSent",
                table: "NotificationMessage");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "NotificationMessage");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "NotificationMessage");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "NotificationMessage");

            migrationBuilder.AlterColumn<string>(
                name: "Subject",
                table: "NotificationMessage",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Recipient",
                table: "NotificationMessage",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}

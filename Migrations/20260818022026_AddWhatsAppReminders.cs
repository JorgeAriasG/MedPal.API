using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "NotificationMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryStatus",
                table: "NotificationMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorDetail",
                table: "NotificationMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderMessageId",
                table: "NotificationMessages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_AppointmentId",
                table: "NotificationMessages",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_Appointments_AppointmentId",
                table: "NotificationMessages",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_Appointments_AppointmentId",
                table: "NotificationMessages");

            migrationBuilder.DropIndex(
                name: "IX_NotificationMessages_AppointmentId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "ErrorDetail",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "ProviderMessageId",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Appointments");
        }
    }
}

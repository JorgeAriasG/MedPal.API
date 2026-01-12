using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumnsToAppointmentsAndPatients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessage_Users_UserId",
                table: "NotificationMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationMessage",
                table: "NotificationMessage");

            migrationBuilder.RenameTable(
                name: "NotificationMessage",
                newName: "NotificationMessages");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationMessage_UserId",
                table: "NotificationMessages",
                newName: "IX_NotificationMessages_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "EmergencyContacts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "EmergencyContacts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmergencyContacts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Add audit columns to Appointments
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Appointments",
                type: "int",
                nullable: true);

            // Add audit columns to Patients
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationMessages",
                table: "NotificationMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessages_Users_UserId",
                table: "NotificationMessages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationMessages_Users_UserId",
                table: "NotificationMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NotificationMessages",
                table: "NotificationMessages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "EmergencyContacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmergencyContacts");

            // Drop audit columns from Appointments
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Appointments");

            // Drop audit columns from Patients
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Patients");

            migrationBuilder.RenameTable(
                name: "NotificationMessages",
                newName: "NotificationMessage");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationMessages_UserId",
                table: "NotificationMessage",
                newName: "IX_NotificationMessage_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NotificationMessage",
                table: "NotificationMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationMessage_Users_UserId",
                table: "NotificationMessage",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditColumnsToPrescriptionItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "PrescriptionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "PrescriptionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "PrescriptionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "PrescriptionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_OriginalAppointmentId",
                table: "Appointments",
                column: "OriginalAppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Appointments_OriginalAppointmentId",
                table: "Appointments",
                column: "OriginalAppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Appointments_OriginalAppointmentId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_OriginalAppointmentId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PrescriptionItems");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldDefaultValue: "");
        }
    }
}

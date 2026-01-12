using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_SoftDelete_To_Missing_Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Settings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Settings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Settings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PrescriptionItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PrescriptionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "PrescriptionItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PrescriptionItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PrescriptionItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PatientInsurances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "PatientInsurances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PatientInsurances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PatientDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "PatientDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PatientDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MedicalHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "MedicalHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ArcoRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "ArcoRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ArcoRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PrescriptionItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PatientInsurances");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PatientInsurances");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PatientInsurances");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PatientDetails");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "PatientDetails");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PatientDetails");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ArcoRequests");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "ArcoRequests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ArcoRequests");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedMedicalHistoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Treatment",
                table: "MedicalHistories",
                newName: "TreatmentStatus");

            migrationBuilder.RenameColumn(
                name: "Medications",
                table: "MedicalHistories",
                newName: "TreatmentPlan");

            migrationBuilder.RenameColumn(
                name: "DoctorNotes",
                table: "MedicalHistories",
                newName: "SpecialtyType");

            migrationBuilder.RenameColumn(
                name: "ConditionName",
                table: "MedicalHistories",
                newName: "PrescribedMedications");

            migrationBuilder.AddColumn<string>(
                name: "ClinicalNotes",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FollowUpDate",
                table: "MedicalHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthcareProfessionalId",
                table: "MedicalHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfidential",
                table: "MedicalHistories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "MedicalHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Appointments",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_HealthcareProfessionalId",
                table: "MedicalHistories",
                column: "HealthcareProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_LastModifiedByUserId",
                table: "MedicalHistories",
                column: "LastModifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Users_HealthcareProfessionalId",
                table: "MedicalHistories",
                column: "HealthcareProfessionalId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Users_LastModifiedByUserId",
                table: "MedicalHistories",
                column: "LastModifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Users_HealthcareProfessionalId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Users_LastModifiedByUserId",
                table: "MedicalHistories");

            migrationBuilder.DropIndex(
                name: "IX_MedicalHistories_HealthcareProfessionalId",
                table: "MedicalHistories");

            migrationBuilder.DropIndex(
                name: "IX_MedicalHistories_LastModifiedByUserId",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "ClinicalNotes",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "HealthcareProfessionalId",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "IsConfidential",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "MedicalHistories");

            migrationBuilder.RenameColumn(
                name: "TreatmentStatus",
                table: "MedicalHistories",
                newName: "Treatment");

            migrationBuilder.RenameColumn(
                name: "TreatmentPlan",
                table: "MedicalHistories",
                newName: "Medications");

            migrationBuilder.RenameColumn(
                name: "SpecialtyType",
                table: "MedicalHistories",
                newName: "DoctorNotes");

            migrationBuilder.RenameColumn(
                name: "PrescribedMedications",
                table: "MedicalHistories",
                newName: "ConditionName");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Appointments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}

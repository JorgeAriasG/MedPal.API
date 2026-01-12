using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantMedicalHistoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrescribedMedications",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "TreatmentPlan",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "TreatmentStatus",
                table: "MedicalHistories");

            migrationBuilder.AddColumn<int>(
                name: "MedicalHistoryId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescriptionId",
                table: "MedicalHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_MedicalHistoryId",
                table: "Prescriptions",
                column: "MedicalHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PrescriptionId",
                table: "MedicalHistories",
                column: "PrescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Prescriptions_PrescriptionId",
                table: "MedicalHistories",
                column: "PrescriptionId",
                principalTable: "Prescriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_MedicalHistories_MedicalHistoryId",
                table: "Prescriptions",
                column: "MedicalHistoryId",
                principalTable: "MedicalHistories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Prescriptions_PrescriptionId",
                table: "MedicalHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_MedicalHistories_MedicalHistoryId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_MedicalHistoryId",
                table: "Prescriptions");

            migrationBuilder.DropIndex(
                name: "IX_MedicalHistories_PrescriptionId",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "MedicalHistoryId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "MedicalHistories");

            migrationBuilder.AddColumn<string>(
                name: "PrescribedMedications",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TreatmentPlan",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TreatmentStatus",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

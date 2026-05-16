using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetDoctorIdToPatientConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetDoctorId",
                table: "PatientConsents",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsents_TargetDoctorId",
                table: "PatientConsents",
                column: "TargetDoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientConsents_Users_TargetDoctorId",
                table: "PatientConsents",
                column: "TargetDoctorId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientConsents_Users_TargetDoctorId",
                table: "PatientConsents");

            migrationBuilder.DropIndex(
                name: "IX_PatientConsents_TargetDoctorId",
                table: "PatientConsents");

            migrationBuilder.DropColumn(
                name: "TargetDoctorId",
                table: "PatientConsents");
        }
    }
}

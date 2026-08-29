using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientRegistrationTokenConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PatientRegistrationTokens_PatientId",
                table: "PatientRegistrationTokens",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientRegistrationTokens_PatientId_Status_ExpiresAt",
                table: "PatientRegistrationTokens",
                columns: new[] { "PatientId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientRegistrationTokens_TokenHash",
                table: "PatientRegistrationTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientRegistrationTokens_Patients_PatientId",
                table: "PatientRegistrationTokens",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientRegistrationTokens_Patients_PatientId",
                table: "PatientRegistrationTokens");

            migrationBuilder.DropIndex(
                name: "IX_PatientRegistrationTokens_PatientId",
                table: "PatientRegistrationTokens");

            migrationBuilder.DropIndex(
                name: "IX_PatientRegistrationTokens_PatientId_Status_ExpiresAt",
                table: "PatientRegistrationTokens");

            migrationBuilder.DropIndex(
                name: "IX_PatientRegistrationTokens_TokenHash",
                table: "PatientRegistrationTokens");
        }
    }
}

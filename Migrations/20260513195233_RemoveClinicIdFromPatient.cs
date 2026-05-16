using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClinicIdFromPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migrar datos existentes: copiar ClinicId de cada paciente a PatientClinics
            migrationBuilder.Sql(@"
                INSERT INTO PatientClinics (PatientId, ClinicId, IsDeleted, CreatedAt)
                SELECT Id, ClinicId, 0, GETUTCDATE()
                FROM Patients
                WHERE ClinicId IS NOT NULL
                  AND NOT EXISTS (
                    SELECT 1 FROM PatientClinics pc
                    WHERE pc.PatientId = Patients.Id AND pc.ClinicId = Patients.ClinicId
                  )
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Clinics_ClinicId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_ClinicId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Patients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Patients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_ClinicId",
                table: "Patients",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Clinics_ClinicId",
                table: "Patients",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

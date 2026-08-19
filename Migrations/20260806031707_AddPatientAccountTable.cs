using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientAccounts",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    IsPrimaryAccount = table.Column<bool>(type: "bit", nullable: false),
                    IsVerifiedByPatient = table.Column<bool>(type: "bit", nullable: false),
                    ConsentToShareProfile = table.Column<bool>(type: "bit", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAccounts", x => new { x.PatientId, x.AccountId });
                    table.ForeignKey(
                        name: "FK_PatientAccounts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientAccounts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAccounts_AccountId",
                table: "PatientAccounts",
                column: "AccountId");

            // Backfill: copiar vínculos existentes PatientClinics -> Clinics.AccountId a PatientAccounts
            migrationBuilder.Sql(@"
                INSERT INTO PatientAccounts (PatientId, AccountId, IsPrimaryAccount, IsVerifiedByPatient, ConsentToShareProfile, IsDeleted, CreatedAt)
                SELECT pc.PatientId, c.AccountId,
                       CASE WHEN p.AccountId = c.AccountId THEN 1 ELSE 0 END,
                       1, 1, 0, GETUTCDATE()
                FROM PatientClinics pc
                INNER JOIN Clinics c ON c.Id = pc.ClinicId
                INNER JOIN Patients p ON p.Id = pc.PatientId
                WHERE pc.IsDeleted = 0 AND c.IsDeleted = 0 AND p.IsDeleted = 0
                  AND NOT EXISTS (
                    SELECT 1 FROM PatientAccounts pa
                    WHERE pa.PatientId = pc.PatientId AND pa.AccountId = c.AccountId
                  )
            ");

            // Backfill: garantizar membership primaria para pacientes con AccountId sin fila en PatientClinics
            migrationBuilder.Sql(@"
                INSERT INTO PatientAccounts (PatientId, AccountId, IsPrimaryAccount, IsVerifiedByPatient, ConsentToShareProfile, IsDeleted, CreatedAt)
                SELECT Id, AccountId, 1, 1, 1, 0, GETUTCDATE()
                FROM Patients
                WHERE AccountId IS NOT NULL AND IsDeleted = 0
                  AND NOT EXISTS (
                    SELECT 1 FROM PatientAccounts pa
                    WHERE pa.PatientId = Patients.Id AND pa.AccountId = Patients.AccountId
                  )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientAccounts");
        }
    }
}

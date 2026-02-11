using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_ConsentAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerClinicId",
                table: "MedicalHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MedicalRecordAccessLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MedicalHistoryId = table.Column<int>(type: "int", nullable: true),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    AccessTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccessingClinicId = table.Column<int>(type: "int", nullable: false),
                    MedicalRecordOwnerClinicId = table.Column<int>(type: "int", nullable: false),
                    HadValidConsent = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecordAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecordAccessLogs_Clinics_AccessingClinicId",
                        column: x => x.AccessingClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalRecordAccessLogs_Clinics_MedicalRecordOwnerClinicId",
                        column: x => x.MedicalRecordOwnerClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalRecordAccessLogs_MedicalHistories_MedicalHistoryId",
                        column: x => x.MedicalHistoryId,
                        principalTable: "MedicalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalRecordAccessLogs_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MedicalRecordAccessLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientConsents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    RequestingClinicId = table.Column<int>(type: "int", nullable: false),
                    OwnerClinicId = table.Column<int>(type: "int", nullable: false),
                    ConsentScope = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ConsentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PatientConsents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientConsents_Clinics_OwnerClinicId",
                        column: x => x.OwnerClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientConsents_Clinics_RequestingClinicId",
                        column: x => x.RequestingClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientConsents_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientConsents_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_OwnerClinicId",
                table: "MedicalHistories",
                column: "OwnerClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_AccessTime",
                table: "MedicalRecordAccessLogs",
                column: "AccessTime");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_ClinicTime",
                table: "MedicalRecordAccessLogs",
                columns: new[] { "AccessingClinicId", "AccessTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_Consent",
                table: "MedicalRecordAccessLogs",
                column: "HadValidConsent");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_HistoryTime",
                table: "MedicalRecordAccessLogs",
                columns: new[] { "MedicalHistoryId", "AccessTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_PatientTime",
                table: "MedicalRecordAccessLogs",
                columns: new[] { "PatientDetailsId", "AccessTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLog_UserTime",
                table: "MedicalRecordAccessLogs",
                columns: new[] { "UserId", "AccessTime" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordAccessLogs_MedicalRecordOwnerClinicId",
                table: "MedicalRecordAccessLogs",
                column: "MedicalRecordOwnerClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsent_CreatedAt",
                table: "PatientConsents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsent_ExpiryDate",
                table: "PatientConsents",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsent_PatientApproved",
                table: "PatientConsents",
                columns: new[] { "PatientDetailsId", "IsApproved" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsent_RequestingClinic",
                table: "PatientConsents",
                columns: new[] { "RequestingClinicId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsent_Unique",
                table: "PatientConsents",
                columns: new[] { "PatientDetailsId", "RequestingClinicId", "OwnerClinicId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsents_ApprovedByUserId",
                table: "PatientConsents",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConsents_OwnerClinicId",
                table: "PatientConsents",
                column: "OwnerClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalHistories_Clinics_OwnerClinicId",
                table: "MedicalHistories",
                column: "OwnerClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalHistories_Clinics_OwnerClinicId",
                table: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "MedicalRecordAccessLogs");

            migrationBuilder.DropTable(
                name: "PatientConsents");

            migrationBuilder.DropIndex(
                name: "IX_MedicalHistories_OwnerClinicId",
                table: "MedicalHistories");

            migrationBuilder.DropColumn(
                name: "OwnerClinicId",
                table: "MedicalHistories");
        }
    }
}

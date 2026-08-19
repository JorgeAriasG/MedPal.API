using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalAttachmentsAndTreatments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Treatments",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicalAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalHistoryId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerClinicId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalAttachments_Clinics_OwnerClinicId",
                        column: x => x.OwnerClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClinicalAttachments_MedicalHistories_MedicalHistoryId",
                        column: x => x.MedicalHistoryId,
                        principalTable: "MedicalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalAttachments_MedicalHistoryId_IsDeleted",
                table: "ClinicalAttachments",
                columns: new[] { "MedicalHistoryId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalAttachments_OwnerClinicId",
                table: "ClinicalAttachments",
                column: "OwnerClinicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalAttachments");

            migrationBuilder.DropColumn(
                name: "Treatments",
                table: "MedicalHistories");
        }
    }
}

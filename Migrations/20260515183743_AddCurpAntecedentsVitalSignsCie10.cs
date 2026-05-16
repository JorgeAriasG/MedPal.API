using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCurpAntecedentsVitalSignsCie10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Curp",
                table: "Patients",
                type: "nvarchar(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AntecedentsData",
                table: "PatientDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cie10Codes",
                table: "MedicalHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cie10Codes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cie10Codes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VitalSigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientDetailsId = table.Column<int>(type: "int", nullable: false),
                    SystolicBP = table.Column<int>(type: "int", nullable: true),
                    DiastolicBP = table.Column<int>(type: "int", nullable: true),
                    HeartRate = table.Column<int>(type: "int", nullable: true),
                    Temperature = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "int", nullable: true),
                    OxygenSaturation = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Bmi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BloodGlucose = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VitalSigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VitalSigns_PatientDetails_PatientDetailsId",
                        column: x => x.PatientDetailsId,
                        principalTable: "PatientDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_PatientDetailsId",
                table: "VitalSigns",
                column: "PatientDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cie10Codes");

            migrationBuilder.DropTable(
                name: "VitalSigns");

            migrationBuilder.DropColumn(
                name: "Curp",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AntecedentsData",
                table: "PatientDetails");

            migrationBuilder.DropColumn(
                name: "Cie10Codes",
                table: "MedicalHistories");
        }
    }
}

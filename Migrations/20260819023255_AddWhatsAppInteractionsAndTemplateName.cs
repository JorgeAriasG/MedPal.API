using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppInteractionsAndTemplateName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWhatsAppConsented",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TemplateName",
                table: "NotificationMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WhatsAppInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    NotificationMessageId = table.Column<int>(type: "int", nullable: true),
                    ButtonId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Wamid = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MetaTimestamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionTaken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FollowUpMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppInteractions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WhatsAppInteractions_NotificationMessages_NotificationMessageId",
                        column: x => x.NotificationMessageId,
                        principalTable: "NotificationMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WhatsAppInteractions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppInteractions_AppointmentId_ReceivedAt",
                table: "WhatsAppInteractions",
                columns: new[] { "AppointmentId", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppInteractions_NotificationMessageId",
                table: "WhatsAppInteractions",
                column: "NotificationMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppInteractions_PatientId",
                table: "WhatsAppInteractions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppInteractions_PatientPhone_ReceivedAt",
                table: "WhatsAppInteractions",
                columns: new[] { "PatientPhone", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppInteractions_Wamid",
                table: "WhatsAppInteractions",
                column: "Wamid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WhatsAppInteractions");

            migrationBuilder.DropColumn(
                name: "IsWhatsAppConsented",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "TemplateName",
                table: "NotificationMessages");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultClinicIdFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clinics_DefaultClinicId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DefaultClinicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DefaultClinicId",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultClinicId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DefaultClinicId",
                table: "Users",
                column: "DefaultClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Clinics_DefaultClinicId",
                table: "Users",
                column: "DefaultClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");
        }
    }
}

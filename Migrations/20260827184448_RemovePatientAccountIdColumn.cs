using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class RemovePatientAccountIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Accounts_AccountId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_AccountId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Patients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Patients",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AccountId",
                table: "Patients",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Accounts_AccountId",
                table: "Patients",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

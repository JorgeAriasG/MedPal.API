using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Audit_Columns_To_Prescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "Prescriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Prescriptions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Prescriptions");
        }
    }
}

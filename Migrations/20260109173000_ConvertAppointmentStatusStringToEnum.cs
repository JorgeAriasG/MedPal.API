using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class ConvertAppointmentStatusStringToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Paso 1: Agregar columna temporal para el nuevo enum
            migrationBuilder.AddColumn<int>(
                name: "StatusTemp",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);  // Default a Scheduled

            // Paso 2: Convertir datos existentes de string a enum int
            // Mapeo de valores string a valores enum:
            // "Scheduled" -> 0 (AppointmentStatus.Scheduled)
            // "InProgress" -> 1 (AppointmentStatus.InProgress)
            // "Completed" -> 2 (AppointmentStatus.Completed)
            // "Cancelled" -> 3 (AppointmentStatus.Cancelled)
            // "NoShow" -> 4 (AppointmentStatus.NoShow)
            // "Rescheduled" -> 5 (AppointmentStatus.Rescheduled)
            // Para valores no mapeados, usar "Scheduled" (0) por defecto

            migrationBuilder.Sql(@"
                UPDATE [Appointments]
                SET [StatusTemp] = CASE
                    WHEN [Status] = 'Scheduled' THEN 0
                    WHEN [Status] = 'scheduled' THEN 0
                    WHEN [Status] = 'InProgress' THEN 1
                    WHEN [Status] = 'in-progress' THEN 1
                    WHEN [Status] = 'inprogress' THEN 1
                    WHEN [Status] = 'Completed' THEN 2
                    WHEN [Status] = 'completed' THEN 2
                    WHEN [Status] = 'Cancelled' THEN 3
                    WHEN [Status] = 'cancelled' THEN 3
                    WHEN [Status] = 'NoShow' THEN 4
                    WHEN [Status] = 'no-show' THEN 4
                    WHEN [Status] = 'noshow' THEN 4
                    WHEN [Status] = 'Rescheduled' THEN 5
                    WHEN [Status] = 'rescheduled' THEN 5
                    WHEN [Status] = 'Pending' THEN 0  -- Pending mapea a Scheduled
                    WHEN [Status] = 'pending' THEN 0
                    ELSE 0  -- Default para valores desconocidos
                END
                WHERE [Status] IS NOT NULL;
            ");

            // Paso 3: Remover la columna original
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            // Paso 4: Renombrar columna temporal a Status
            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "Appointments",
                newName: "Status");

            // Paso 5: Hacer Status no nullable (si no lo era)
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir: Convertir enum int back to string
            migrationBuilder.AddColumn<string>(
                name: "StatusTemp",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Scheduled");

            migrationBuilder.Sql(@"
                UPDATE [Appointments]
                SET [StatusTemp] = CASE
                    WHEN [Status] = 0 THEN 'Scheduled'
                    WHEN [Status] = 1 THEN 'InProgress'
                    WHEN [Status] = 2 THEN 'Completed'
                    WHEN [Status] = 3 THEN 'Cancelled'
                    WHEN [Status] = 4 THEN 'NoShow'
                    WHEN [Status] = 5 THEN 'Rescheduled'
                    ELSE 'Scheduled'
                END;
            ");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "StatusTemp",
                table: "Appointments",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}

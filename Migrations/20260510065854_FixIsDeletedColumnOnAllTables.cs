using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class FixIsDeletedColumnOnAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @tables TABLE (TableName NVARCHAR(128));
                INSERT INTO @tables (TableName) VALUES 
                ('UserClinics'), ('Clinics'), ('Settings'), ('Reports'), ('PrescriptionItems'), 
                ('Prescriptions'), ('Payments'), ('PatientInsurances'), ('PatientDetails'), 
                ('PatientConsents'), ('Patients'), ('NotificationMessages'), ('MedicalHistories'), 
                ('Invoices'), ('InsuranceProviders'), ('EmergencyContacts'), ('ArcoRequests'), 
                ('Appointments'), ('Allergies'), ('Users'), ('UserRoles'), ('Roles');

                DECLARE @tableName NVARCHAR(128);
                DECLARE @sql NVARCHAR(MAX);

                DECLARE cur CURSOR FOR SELECT TableName FROM @tables;
                OPEN cur;
                FETCH NEXT FROM cur INTO @tableName;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    -- Add IsDeleted if it does not exist
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[' + @tableName + ']') AND name = 'IsDeleted')
                    BEGIN
                        SET @sql = 'ALTER TABLE [dbo].[' + @tableName + '] ADD [IsDeleted] bit NOT NULL DEFAULT 0;';
                        EXEC sp_executesql @sql;
                    END

                    FETCH NEXT FROM cur INTO @tableName;
                END

                CLOSE cur;
                DEALLOCATE cur;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

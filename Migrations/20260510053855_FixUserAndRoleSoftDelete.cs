using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class FixUserAndRoleSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = 'DeletedAt')
                BEGIN
                    ALTER TABLE [dbo].[Users] ADD [DeletedAt] datetime2 NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Users') AND name = 'DeletedByUserId')
                BEGIN
                    ALTER TABLE [dbo].[Users] ADD [DeletedByUserId] int NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.UserRoles') AND name = 'DeletedAt')
                BEGIN
                    ALTER TABLE [dbo].[UserRoles] ADD [DeletedAt] datetime2 NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.UserRoles') AND name = 'DeletedByUserId')
                BEGIN
                    ALTER TABLE [dbo].[UserRoles] ADD [DeletedByUserId] int NULL;
                END
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Roles') AND name = 'DeletedAt')
                BEGIN
                    ALTER TABLE [dbo].[Roles] ADD [DeletedAt] datetime2 NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}

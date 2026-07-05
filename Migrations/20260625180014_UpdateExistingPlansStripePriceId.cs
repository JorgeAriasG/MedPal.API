using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateExistingPlansStripePriceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var schema = "dbo";
            migrationBuilder.Sql($@"
                UPDATE [{schema}].[SubscriptionPlans]
                SET StripePriceId = 'price_1TmEdwCdrBEjQEKTLhMAXTJF'
                WHERE Name = 'SOLO' AND (StripePriceId IS NULL OR StripePriceId = '')
            ");
            migrationBuilder.Sql($@"
                UPDATE [{schema}].[SubscriptionPlans]
                SET StripePriceId = 'price_1TmEeECdrBEjQEKTCYEDov6j'
                WHERE Name = 'CONSULTORIO' AND (StripePriceId IS NULL OR StripePriceId = '')
            ");
            migrationBuilder.Sql($@"
                UPDATE [{schema}].[SubscriptionPlans]
                SET StripePriceId = 'price_1TmEePCdrBEjQEKTgrEjJqsV'
                WHERE Name = 'CLINICA' AND (StripePriceId IS NULL OR StripePriceId = '')
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = NULL
                WHERE Name IN ('SOLO', 'CONSULTORIO', 'CLINICA')
            ");
        }
    }
}

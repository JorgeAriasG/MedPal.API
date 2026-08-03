using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedPal.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStripePriceIdsToNewAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = 'price_1U0TzNCssaSAK7S4u7swZfFi'
                WHERE Name = 'SOLO' AND StripePriceId = 'price_1TmEdwCdrBEjQEKTLhMAXTJF'
            ");
            migrationBuilder.Sql(@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = 'price_1U0TzsCssaSAK7S4bkc4XHvd'
                WHERE Name = 'CONSULTORIO' AND StripePriceId = 'price_1TmEeECdrBEjQEKTCYEDov6j'
            ");
            migrationBuilder.Sql(@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = 'price_1U0U0gCssaSAK7S4Th0P7SC7'
                WHERE Name = 'CLINICA' AND StripePriceId = 'price_1TmEePCdrBEjQEKTgrEjJqsV'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = 'price_1TmEdwCdrBEjQEKTLhMAXTJF'
                WHERE Name = 'SOLO' AND StripePriceId = 'price_1U0TzNCssaSAK7S4u7swZfFi'
            ");
            migrationBuilder.Sql(@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = 'price_1TmEeECdrBEjQEKTCYEDov6j'
                WHERE Name = 'CONSULTORIO' AND StripePriceId = 'price_1U0TzsCssaSAK7S4bkc4XHvd'
            ");
            migrationBuilder.Sql(@"
                UPDATE [dbo].[SubscriptionPlans]
                SET StripePriceId = 'price_1TmEePCdrBEjQEKTgrEjJqsV'
                WHERE Name = 'CLINICA' AND StripePriceId = 'price_1U0U0gCssaSAK7S4Th0P7SC7'
            ");
        }
    }
}

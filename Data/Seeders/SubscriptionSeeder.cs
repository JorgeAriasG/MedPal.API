using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;

namespace MedPal.API.Data.Seeders
{
    public static class SubscriptionSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await SeedPlansAsync(context);

            await SeedAccountSubscriptionsAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedPlansAsync(AppDbContext context)
        {
            if (await context.Set<SubscriptionPlan>().AnyAsync())
                return;

            var plans = new[]
            {
                new SubscriptionPlan
                {
                    Name = "SOLO",
                    Description = "Para especialistas independientes",
                    Price = 399m,
                    MaxTeamMembers = 1,
                    MaxClinics = 1,
                    MaxActiveCalendars = 1,
                    TrialDays = 30,
                    StripePriceId = "price_1TmEdwCdrBEjQEKTLhMAXTJF",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "CONSULTORIO",
                    Description = "Para equipos pequeños",
                    Price = 799m,
                    MaxTeamMembers = 4,
                    MaxClinics = 1,
                    MaxActiveCalendars = 4,
                    TrialDays = 0,
                    StripePriceId = "price_1TmEeECdrBEjQEKTCYEDov6j",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "CLINICA",
                    Description = "Para clínicas pequeñas",
                    Price = 1499m,
                    MaxTeamMembers = 10,
                    MaxClinics = 3,
                    MaxActiveCalendars = 10,
                    TrialDays = 0,
                    StripePriceId = "price_1TmEePCdrBEjQEKTgrEjJqsV",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Set<SubscriptionPlan>().AddRangeAsync(plans);
        }

        private static async Task SeedAccountSubscriptionsAsync(AppDbContext context)
        {
            var clinicaPlan = await context.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.Name == "CLINICA");

            if (clinicaPlan == null)
                return;

            var accounts = await context.Accounts
                .Where(a => !context.Set<Subscription>().Any(s => s.AccountId == a.Id && s.IsActive))
                .ToListAsync();

            foreach (var account in accounts)
            {
                var userCount = await context.Users
                    .CountAsync(u => u.AccountId == account.Id && !u.IsDeleted);
                var clinicCount = await context.Clinics
                    .CountAsync(c => c.AccountId == account.Id && !c.IsDeleted);

                SubscriptionPlan plan;
                if (clinicCount >= 2 || userCount >= 5)
                {
                    plan = clinicaPlan;
                }
                else if (userCount >= 2)
                {
                    plan = await context.Set<SubscriptionPlan>()
                        .FirstOrDefaultAsync(p => p.Name == "CONSULTORIO") ?? clinicaPlan;
                }
                else
                {
                    plan = await context.Set<SubscriptionPlan>()
                        .FirstOrDefaultAsync(p => p.Name == "SOLO") ?? clinicaPlan;
                }

                context.Set<Subscription>().Add(new Subscription
                {
                    AccountId = account.Id,
                    SubscriptionPlanId = plan.Id,
                    Status = "Active",
                    CurrentPeriodStart = DateTime.UtcNow,
                    CurrentPeriodEnd = DateTime.UtcNow.AddYears(1),
                    MaxTeamMembers = plan.MaxTeamMembers,
                    MaxClinics = plan.MaxClinics,
                    MaxActiveCalendars = plan.MaxActiveCalendars,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
    }
}

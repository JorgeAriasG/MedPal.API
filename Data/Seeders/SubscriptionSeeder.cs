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
            var existing = await context.Set<SubscriptionPlan>().ToListAsync();

            var plans = new[]
            {
                new SubscriptionPlan
                {
                    Name = "SOLO",
                    Description = "Para especialistas independientes",
                    Price = 699m,
                    MaxTeamMembers = 1,
                    MaxClinics = 1,
                    MaxActiveCalendars = 1,
                    TrialDays = 7,
                    StripePriceId = "price_1U0TzNCssaSAK7S4u7swZfFi",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "CONSULTORIO",
                    Description = "Para equipos pequeños",
                    Price = 1399m,
                    MaxTeamMembers = 4,
                    MaxClinics = 1,
                    MaxActiveCalendars = 4,
                    TrialDays = 7,
                    StripePriceId = "price_1U0TzsCssaSAK7S4bkc4XHvd",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "CLINICA",
                    Description = "Para clínicas pequeñas",
                    Price = 2499m,
                    MaxTeamMembers = 10,
                    MaxClinics = 3,
                    MaxActiveCalendars = 10,
                    TrialDays = 7,
                    StripePriceId = "price_1U0U0gCssaSAK7S4Th0P7SC7",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new SubscriptionPlan
                {
                    Name = "DUO",
                    Description = "Para parejas de profesionales",
                    Price = 679m,
                    MaxTeamMembers = 2,
                    MaxClinics = 1,
                    MaxActiveCalendars = 2,
                    TrialDays = 7,
                    StripePriceId = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            foreach (var plan in plans)
            {
                var existingPlan = existing.FirstOrDefault(p => p.Name == plan.Name);

                if (existingPlan == null)
                {
                    context.Set<SubscriptionPlan>().Add(plan);
                    continue;
                }

                existingPlan.Description = plan.Description;
                existingPlan.Price = plan.Price;
                existingPlan.MaxTeamMembers = plan.MaxTeamMembers;
                existingPlan.MaxClinics = plan.MaxClinics;
                existingPlan.MaxActiveCalendars = plan.MaxActiveCalendars;
                existingPlan.TrialDays = plan.TrialDays;
                existingPlan.IsActive = plan.IsActive;
                if (!string.IsNullOrEmpty(plan.StripePriceId))
                {
                    existingPlan.StripePriceId = plan.StripePriceId;
                }
                existingPlan.UpdatedAt = DateTime.UtcNow;
            }
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

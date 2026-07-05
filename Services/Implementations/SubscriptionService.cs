using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;

namespace MedPal.API.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IStripeService _stripeService;
        private readonly IMapper _mapper;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            IStripeService stripeService,
            IMapper mapper)
        {
            _subscriptionRepository = subscriptionRepository;
            _stripeService = stripeService;
            _mapper = mapper;
        }

        public async Task<List<SubscriptionPlanReadDTO>> GetPlansAsync()
        {
            var plans = await _subscriptionRepository.GetAllPlansAsync();
            return _mapper.Map<List<SubscriptionPlanReadDTO>>(plans);
        }

        public async Task<SubscriptionReadDTO?> GetCurrentSubscriptionAsync(int accountId)
        {
            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (subscription == null) return null;

            var dto = _mapper.Map<SubscriptionReadDTO>(subscription);
            dto.CurrentTeamMembers = await _subscriptionRepository.GetTeamMemberCountAsync(accountId);
            dto.CurrentClinics = await _subscriptionRepository.GetClinicCountAsync(accountId);
            return dto;
        }

        public async Task<bool> CanAddUserAsync(int accountId)
        {
            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (subscription == null) return false;

            var current = await _subscriptionRepository.GetTeamMemberCountAsync(accountId);
            return current < subscription.MaxTeamMembers;
        }

        public async Task<bool> CanAddClinicAsync(int accountId)
        {
            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (subscription == null) return false;

            var current = await _subscriptionRepository.GetClinicCountAsync(accountId);
            return current < subscription.MaxClinics;
        }

        public async Task AssignPlanAsync(int accountId, string planName = "SOLO")
        {
            var plan = await _subscriptionRepository.GetPlanByNameAsync(planName);
            if (plan == null)
            {
                plan = await _subscriptionRepository.GetPlanByNameAsync("SOLO");
                if (plan == null) return;
            }

            var exists = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (exists != null) return;

            var trialEnd = plan.TrialDays > 0
                ? DateTime.UtcNow.AddDays(plan.TrialDays)
                : (DateTime?)null;

            var subscription = new Subscription
            {
                AccountId = accountId,
                SubscriptionPlanId = plan.Id,
                Status = "pending_payment",
                CurrentPeriodStart = DateTime.UtcNow,
                CurrentPeriodEnd = trialEnd ?? DateTime.UtcNow.AddDays(30),
                IsActive = true,
                TrialEndsAt = trialEnd,
                MaxTeamMembers = plan.MaxTeamMembers,
                MaxClinics = plan.MaxClinics,
                MaxActiveCalendars = plan.MaxActiveCalendars,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _subscriptionRepository.CreateAsync(subscription);
        }

        public async Task<SubscriptionStatusDTO> GetSubscriptionStatusAsync(int accountId)
        {
            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (subscription == null)
            {
                return new SubscriptionStatusDTO
                {
                    Status = "none",
                    HasAccess = false,
                };
            }

            var hasAccess = subscription.Status switch
            {
                "trial" => true,
                "active" => true,
                "pending_payment" => true,
                _ => false
            };

            return new SubscriptionStatusDTO
            {
                Status = subscription.Status,
                HasAccess = hasAccess,
                PlanName = subscription.SubscriptionPlan?.Name,
                IsTrialing = subscription.Status == "trial",
                TrialEndsAt = subscription.TrialEndsAt,
            };
        }

        public async Task<CheckoutSessionResponse> CreatePendingSubscriptionAsync(
            int accountId, string planName, string? stripeCustomerId = null)
        {
            var plan = await _subscriptionRepository.GetPlanByNameAsync(planName);
            if (plan == null)
                plan = await _subscriptionRepository.GetPlanByNameAsync("SOLO");
            if (plan == null || string.IsNullOrEmpty(plan.StripePriceId))
                throw new InvalidOperationException("Plan sin precio de Stripe configurado");

            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (subscription == null)
                throw new InvalidOperationException("No hay suscripción pendiente");

            if (!string.IsNullOrEmpty(stripeCustomerId))
                subscription.StripeCustomerId = stripeCustomerId;

            await _subscriptionRepository.UpdateAsync(subscription);

            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            var successUrl = config["Stripe:SuccessUrl"] ?? "http://localhost:4200/checkout/success?session_id={CHECKOUT_SESSION_ID}";
            var cancelUrl = config["Stripe:CancelUrl"] ?? "http://localhost:4200/checkout/required";

            return await _stripeService.CreateCheckoutSessionAsync(
                subscription.StripeCustomerId ?? stripeCustomerId ?? "",
                plan.StripePriceId,
                plan.TrialDays,
                successUrl,
                cancelUrl,
                accountId);
        }

        public async Task<string> GetLimitExceededMessageAsync(int accountId, string resourceType)
        {
            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            var planName = subscription?.SubscriptionPlan?.Name ?? "desconocido";
            var max = resourceType switch
            {
                "user" => subscription?.MaxTeamMembers ?? 0,
                "clinic" => subscription?.MaxClinics ?? 0,
                _ => 0
            };

            return $"Has alcanzado el límite de {resourceType}s de tu plan {planName} ({max}). " +
                   "Actualiza tu plan para agregar más.";
        }
    }
}

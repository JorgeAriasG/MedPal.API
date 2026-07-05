using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MedPal.API.Services.Implementations;
using Microsoft.Extensions.Logging;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using Stripe;
using Stripe.Checkout;

namespace MedPal.API.Services.Implementations
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IServiceScopeFactory _scopeFactory;

        public StripeService(
            IConfiguration configuration,
            ILogger<StripeService> logger,
            ISubscriptionRepository subscriptionRepository,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _subscriptionRepository = subscriptionRepository;
            _scopeFactory = scopeFactory;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreateCustomerAsync(string email, string name)
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
            };
            var service = new CustomerService();
            var customer = await service.CreateAsync(options);
            _logger.LogInformation("Stripe customer created: {CustomerId} for {Email}", customer.Id, email);
            return customer.Id;
        }

        public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
            string stripeCustomerId, string priceId, int trialDays,
            string successUrl, string cancelUrl, int? accountId = null)
        {
            var options = new SessionCreateOptions
            {
                Customer = stripeCustomerId,
                Mode = "subscription",
                LineItems = new()
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    TrialPeriodDays = trialDays > 0 ? trialDays : null,
                    Metadata = new()
                    {
                        { "account_id", accountId?.ToString() ?? "" },
                    },
                },
                Metadata = new()
                {
                    { "account_id", accountId?.ToString() ?? "" },
                },
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation(
                "Checkout session created: {SessionId} for customer {CustomerId}, trial: {TrialDays}",
                session.Id, stripeCustomerId, trialDays);

            return new CheckoutSessionResponse
            {
                CheckoutUrl = session.Url,
                SessionId = session.Id,
            };
        }

        public async Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl)
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl,
            };
            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);
            return session.Url;
        }

        public async Task<bool> HandleWebhookAsync(string json, string signatureHeader)
        {
            try
            {
                var webhookSecret = _configuration["Stripe:WebhookSecret"];
                var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

                _logger.LogInformation("Webhook received: {Type}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        await HandleCheckoutCompleted(stripeEvent);
                        break;
                    case "invoice.paid":
                        await HandleInvoicePaid(stripeEvent);
                        break;
                    case "invoice.payment_failed":
                        await HandleInvoicePaymentFailed(stripeEvent);
                        break;
                    case "customer.subscription.updated":
                        await HandleSubscriptionUpdated(stripeEvent);
                        break;
                    case "customer.subscription.deleted":
                        await HandleSubscriptionDeleted(stripeEvent);
                        break;
                }

                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing failed");
                return false;
            }
        }

        private async Task HandleCheckoutCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            var pendingRegId = session.Metadata?.GetValueOrDefault("pending_registration_id");
            if (!string.IsNullOrEmpty(pendingRegId))
            {
                _logger.LogInformation("Webhook: completing registration for session {SessionId}", session.Id);
                using (var scope = _scopeFactory.CreateScope())
               {
                   var regService = scope.ServiceProvider.GetRequiredService<IRegistrationService>();
                   await regService.CompleteFromWebhookAsync(session.Id);
               }
                return;
            }

            var accountIdStr = session.Metadata?.GetValueOrDefault("account_id");
            if (string.IsNullOrEmpty(accountIdStr) || !int.TryParse(accountIdStr, out int accountId))
            {
                _logger.LogWarning("Checkout completed without account_id metadata");
                return;
            }

            var subscription = await _subscriptionRepository.GetActiveByAccountIdAsync(accountId);
            if (subscription == null)
            {
                _logger.LogWarning("No pending subscription found for account {AccountId}", accountId);
                return;
            }

            subscription.StripeCustomerId = session.CustomerId;
            subscription.StripeSubscriptionId = session.SubscriptionId;

            if (subscription.TrialEndsAt != null && subscription.TrialEndsAt > DateTime.UtcNow)
            {
                subscription.Status = "trial";
            }
            else
            {
                subscription.Status = "active";
                subscription.CurrentPeriodStart = DateTime.UtcNow;
                subscription.CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1);
            }

            await _subscriptionRepository.UpdateAsync(subscription);
            _logger.LogInformation("Subscription activated for account {AccountId}: {Status}", accountId, subscription.Status);
        }

        private async Task HandleInvoicePaid(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice?.SubscriptionId == null) return;

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(invoice.SubscriptionId);
            if (subscription == null)
            {
                _logger.LogWarning("No local subscription for Stripe sub {SubId}", invoice.SubscriptionId);
                return;
            }

            subscription.Status = "active";
            subscription.CurrentPeriodStart = invoice.PeriodStart;
            subscription.CurrentPeriodEnd = invoice.PeriodEnd;
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        private async Task HandleInvoicePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Stripe.Invoice;
            if (invoice?.SubscriptionId == null) return;

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(invoice.SubscriptionId);
            if (subscription == null) return;

            subscription.Status = "past_due";
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var stripeSub = stripeEvent.Data.Object as Stripe.Subscription;
            if (stripeSub == null) return;

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSub.Id);
            if (subscription == null) return;

            subscription.Status = stripeSub.Status switch
            {
                "active" => "active",
                "trialing" => "trial",
                "past_due" => "past_due",
                "canceled" => "cancelled",
                "incomplete" => "pending_payment",
                _ => subscription.Status
            };

            subscription.CurrentPeriodStart = stripeSub.CurrentPeriodStart;
            subscription.CurrentPeriodEnd = stripeSub.CurrentPeriodEnd;

            if (stripeSub.Items?.Data != null && stripeSub.Items.Data.Count > 0)
            {
                var priceId = stripeSub.Items.Data[0].Price?.Id;
                if (!string.IsNullOrEmpty(priceId))
                {
                    var plan = await _subscriptionRepository.GetPlanByStripePriceIdAsync(priceId);
                    if (plan != null && plan.Id != subscription.SubscriptionPlanId)
                    {
                        subscription.SubscriptionPlanId = plan.Id;
                        subscription.MaxTeamMembers = plan.MaxTeamMembers;
                        subscription.MaxClinics = plan.MaxClinics;
                        subscription.MaxActiveCalendars = plan.MaxActiveCalendars;
                    }
                }
            }

            await _subscriptionRepository.UpdateAsync(subscription);
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var stripeSub = stripeEvent.Data.Object as Stripe.Subscription;
            if (stripeSub == null) return;

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(stripeSub.Id);
            if (subscription == null) return;

            subscription.Status = "cancelled";
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.IsActive = false;
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        public async Task<Session> CreateRegistrationCheckoutSessionAsync(
            string email, string name, string priceId, int trialDays,
            string pendingSessionId, string? planName)
        {
            var customerOptions = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new()
                {
                    { "pending_registration_id", pendingSessionId },
                },
            };
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(customerOptions);

            var returnUrl = _configuration["Stripe:ReturnUrl"] ?? "http://localhost:4200";
            var options = new SessionCreateOptions
            {
                Customer = customer.Id,
                Mode = "subscription",
                UiMode = "embedded",
                ReturnUrl = returnUrl,
                LineItems = new()
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    },
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    TrialPeriodDays = trialDays > 0 ? trialDays : null,
                    Metadata = new()
                    {
                        { "pending_registration_id", pendingSessionId },
                        { "plan_name", planName ?? "SOLO" },
                    },
                },
                Metadata = new()
                {
                    { "pending_registration_id", pendingSessionId },
                    { "plan_name", planName ?? "SOLO" },
                },
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation(
                "Registration checkout session created: {SessionId} for {Email}, customer: {CustomerId}",
                session.Id, email, customer.Id);

            return session;
        }

        public async Task<Session> VerifySessionAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            return session;
        }

        public async Task<CheckoutSessionInfoDTO> GetSessionInfoAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);

            var lineItemOptions = new SessionLineItemListOptions();
            var lineItems = await service.ListLineItemsAsync(sessionId, lineItemOptions);

            string planName = "Desconocido";
            decimal amount = 0;

            if (lineItems?.Data != null && lineItems.Data.Count > 0)
            {
                long? rawAmount = lineItems.Data[0].AmountTotal;
                amount = (rawAmount ?? 0) / 100m;
                var priceId = lineItems.Data[0].Price?.Id;
                if (!string.IsNullOrEmpty(priceId))
                {
                    var plan = await _subscriptionRepository.GetPlanByStripePriceIdAsync(priceId);
                    planName = plan?.Name ?? "Desconocido";
                }
            }

            // Get subscription details to check trial end
            DateTime? trialEnd = null;
            if (!string.IsNullOrEmpty(session.SubscriptionId))
            {
                try
                {
                    var subService = new Stripe.SubscriptionService();
                    var stripeSub = await subService.GetAsync(session.SubscriptionId);
                    trialEnd = stripeSub.TrialEnd;
                }
                catch { }
            }

            return new CheckoutSessionInfoDTO
            {
                PlanName = planName,
                Amount = amount,
                Currency = session.Currency?.ToUpperInvariant() ?? "MXN",
                Status = session.Status ?? "complete",
                CustomerEmail = session.CustomerDetails?.Email,
                TrialEnd = trialEnd,
            };
        }
    }
}

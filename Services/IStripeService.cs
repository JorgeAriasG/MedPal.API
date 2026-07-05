using System.Threading.Tasks;
using MedPal.API.DTOs;
using Stripe.Checkout;

namespace MedPal.API.Services
{
    public interface IStripeService
    {
        Task<string> CreateCustomerAsync(string email, string name);
        Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(
            string stripeCustomerId, string priceId, int trialDays,
            string successUrl, string cancelUrl, int? accountId = null);
        Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl);
        Task<bool> HandleWebhookAsync(string json, string signatureHeader);
        Task<CheckoutSessionInfoDTO> GetSessionInfoAsync(string sessionId);

        // Registration flow
        Task<Session> CreateRegistrationCheckoutSessionAsync(
            string email, string name, string priceId, int trialDays,
            string sessionId, string? planName);
        Task<Session> VerifySessionAsync(string sessionId);
    }
}

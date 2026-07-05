using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IStripeService _stripeService;
        private readonly ITenantContextService _tenantContext;

        public SubscriptionController(
            ISubscriptionService subscriptionService,
            IStripeService stripeService,
            ITenantContextService tenantContext)
        {
            _subscriptionService = subscriptionService;
            _stripeService = stripeService;
            _tenantContext = tenantContext;
        }

        [AllowAnonymous]
        [HttpGet("plans")]
        public async Task<ActionResult<List<SubscriptionPlanReadDTO>>> GetPlans()
        {
            var plans = await _subscriptionService.GetPlansAsync();
            return Ok(plans);
        }

        [HttpGet("current")]
        public async Task<ActionResult<SubscriptionReadDTO>> GetCurrentSubscription()
        {
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId == null)
                return Unauthorized(new { message = "Usuario sin cuenta asignada" });

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(accountId.Value);
            if (subscription == null)
                return NotFound(new { message = "No se encontró suscripción activa" });

            return Ok(subscription);
        }

        [HttpGet("status")]
        public async Task<ActionResult<SubscriptionStatusDTO>> GetSubscriptionStatus()
        {
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId == null)
                return Unauthorized(new { message = "Usuario sin cuenta asignada" });

            var status = await _subscriptionService.GetSubscriptionStatusAsync(accountId.Value);
            return Ok(status);
        }

        [HttpPost("create-checkout")]
        public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckout()
        {
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId == null)
                return Unauthorized(new { message = "Usuario sin cuenta asignada" });

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(accountId.Value);
            if (subscription == null)
                return BadRequest(new { message = "No hay suscripción pendiente" });

            var result = await _subscriptionService.CreatePendingSubscriptionAsync(
                accountId.Value, subscription.Plan?.Name ?? "SOLO");
            return Ok(result);
        }

        [HttpPost("create-portal")]
        public async Task<ActionResult<object>> CreatePortalSession()
        {
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId == null)
                return Unauthorized(new { message = "Usuario sin cuenta asignada" });

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(accountId.Value);
            if (subscription == null || string.IsNullOrEmpty(subscription.Plan?.StripePriceId))
                return BadRequest(new { message = "No hay suscripción activa para gestionar" });

            var subEntity = await _subscriptionService.GetCurrentSubscriptionAsync(accountId.Value);
            if (subEntity == null)
                return NotFound();

            var stripeCustomerId = ""; // We need to get this from the entity
            var returnUrl = $"{Request.Scheme}://{Request.Host}/settings";

            // We don't expose StripeCustomerId in DTO, fetch from repo
            var repo = HttpContext.RequestServices.GetService(typeof(ISubscriptionRepository))
                as ISubscriptionRepository;
            var activeSub = await repo.GetActiveByAccountIdAsync(accountId.Value);
            if (activeSub == null || string.IsNullOrEmpty(activeSub.StripeCustomerId))
                return BadRequest(new { message = "No hay cliente de Stripe asociado" });

            var portalUrl = await _stripeService.CreatePortalSessionAsync(
                activeSub.StripeCustomerId, returnUrl);
            return Ok(new { url = portalUrl });
        }

        [HttpGet("can-add-user")]
        public async Task<ActionResult<object>> CanAddUser()
        {
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId == null)
                return Unauthorized(new { message = "Usuario sin cuenta asignada" });

            var canAdd = await _subscriptionService.CanAddUserAsync(accountId.Value);
            if (canAdd)
                return Ok(new { canAdd = true });

            var message = await _subscriptionService.GetLimitExceededMessageAsync(accountId.Value, "user");
            return Ok(new { canAdd = false, message });
        }

        [HttpGet("can-add-clinic")]
        public async Task<ActionResult<object>> CanAddClinic()
        {
            var accountId = _tenantContext.CurrentAccountId;
            if (accountId == null)
                return Unauthorized(new { message = "Usuario sin cuenta asignada" });

            var canAdd = await _subscriptionService.CanAddClinicAsync(accountId.Value);
            if (canAdd)
                return Ok(new { canAdd = true });

            var message = await _subscriptionService.GetLimitExceededMessageAsync(accountId.Value, "clinic");
            return Ok(new { canAdd = false, message });
        }
    }
}

using System.IO;
using System.Threading.Tasks;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    [AllowAnonymous] // Webhook público: la autenticación se valida por firma Stripe dentro del servicio
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        public StripeWebhookController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];

            var result = await _stripeService.HandleWebhookAsync(json, signatureHeader);

            if (result)
                return Ok();

            return BadRequest(new { message = "Webhook processing failed" });
        }
    }
}

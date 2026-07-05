using System.IO;
using System.Threading.Tasks;
using MedPal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
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

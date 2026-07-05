using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/checkout")]
    public class CheckoutController : ControllerBase
    {
        private readonly IStripeService _stripeService;

        public CheckoutController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [AllowAnonymous]
        [HttpGet("session/{sessionId}")]
        public async Task<ActionResult<CheckoutSessionInfoDTO>> GetSessionInfo(string sessionId)
        {
            try
            {
                var info = await _stripeService.GetSessionInfoAsync(sessionId);
                return Ok(info);
            }
            catch
            {
                return NotFound(new { message = "Sesión no encontrada" });
            }
        }
    }
}

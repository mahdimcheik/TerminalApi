using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.V2;

namespace TerminalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WebhookController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult HandleWebhook()
        {
            var json = new StreamReader(HttpContext.Request.Body).ReadToEnd();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]
                );

                if (stripeEvent.Type == Stripe.EventTypes.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    // Mark order as paid in the database
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest($"Stripe webhook error: {e.Message}");
            }
        }
    }
}

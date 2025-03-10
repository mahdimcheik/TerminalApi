using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.V2;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                Console.WriteLine(" ****************header : " + Request.Headers);
                Console.WriteLine("fini *********************");
                var stripeSignature = Request.Headers["Stripe-Signature"];
                var webhookSecret = EnvironmentVariables.STRIPE_SECRETKEY;

                _logger.LogInformation("Stripe-Signature: {StripeSignature}", stripeSignature);
                _logger.LogInformation("Webhook Secret: {WebhookSecret}", webhookSecret);

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret
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
                _logger.LogError(e, "Stripe webhook error");
                return BadRequest($"Stripe webhook error: {e.Message}");
            }
        }
    }
}

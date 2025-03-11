using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
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
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            string endpointSecret = "whsec_acade8aec2b4a104fe0df69c7a9ed7641ec6cca650e8505a7fe726fd515f7a49";
            try
            {
                var stripeEvent = EventUtility.ParseEvent(json);
                var signatureHeader = Request.Headers["Stripe-Signature"];
                //Console.WriteLine("********************* Start");
                //Console.WriteLine(json);
                //Console.WriteLine("********************* end");

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);

                // If on SDK version < 46, use class Events instead of EventTypes
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

                    if (session.PaymentStatus == "paid") // Ensure payment is completed
                    {
                        if (session.Metadata.TryGetValue("order_id", out string orderId))
                        {
                            Console.WriteLine($"✅ Payment Order ID: {orderId}");
                            // Update the order in your database as PAID
                        }                        
                        if (session.Metadata.TryGetValue("order_number", out string orderNumber))
                        {
                            Console.WriteLine($"✅ Payment  number: {orderNumber}");
                            // Update the order in your database as PAID
                        }
                        if (session.Metadata.TryGetValue("booker_id", out string bookerId))
                        {
                            Console.WriteLine($"✅ Payment successful for bookerId: {bookerId}");
                            // Update the order in your database as PAID
                        }
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Payment for Order ID {session.Metadata["order_id"]} is still pending!");
                    }
                }
                if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine(
                        "A successful payment for {0} was made.",
                        paymentIntent.Amount
                    );
                    if (paymentIntent.Metadata.TryGetValue("order_id", out string orderId))
                    {
                        Console.WriteLine($"Payment successful for Order ID: {orderId}");

                        // Now you can fetch the order from your DB using this orderId
                        // Example: var order = _orderService.GetOrderById(orderId);
                    }
                    // Then define and call a method to handle the successful payment intent.
                    // handlePaymentIntentSucceeded(paymentIntent);
                }
                else if (stripeEvent.Type == EventTypes.PaymentMethodAttached)
                {
                    var paymentMethod = stripeEvent.Data.Object as PaymentMethod;
                    // Then define and call a method to handle the successful attachment of a PaymentMethod.
                    // handlePaymentMethodAttached(paymentMethod);
                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}


// étapes à suivre
/***
 * stripe login
 * page de connection
 * stripe forward : stripe listen --forward-to https://localhost:7113/api/webhook
 * use secret end point whsec.......
 * ****/
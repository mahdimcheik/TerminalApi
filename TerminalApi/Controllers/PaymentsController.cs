using System.Linq.Expressions;
using System.Security.Cryptography.Xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Stripe;
using Stripe.Checkout;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Notification;
using TerminalApi.Models.Payments;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly PaymentsService paymentsService;

        public PaymentsController(ApiDefaultContext context, PaymentsService paymentsService)
        {
            StripeConfiguration.ApiKey = EnvironmentVariables.STRIPE_SECRETKEY;
            this.context = context;
            this.paymentsService = paymentsService;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CheckoutRequest request)
        {
            try
            {
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(
                        new ResponseDTO { Status = 400, Message = "Demande refusée" }
                    );
                }
                var result = await paymentsService.Checkorder(request.OrderId, user.Id);
                if (!result.isValid)
                {
                    return BadRequest(
                        new { Message = "Commande non valide, vérifiez la!", Status = 401 }
                    );
                }
                var domain = EnvironmentVariables.API_FRONT_URL;

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new()
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "EUR",
                                UnitAmount = (long)(result.order.TotalDiscountedPrice * 100),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = result.order.OrderNumber,
                                    Description = result.order.OrderNumber,
                                    Metadata = new Dictionary<string, string>
                                    {
                                        { "order_id", "12345" },
                                    },
                                },
                            },

                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", result.order.Id.ToString() },
                        { "order_number", result.order.OrderNumber },
                        { "booker_id", result.order.Booker.Id },
                    },
                    SuccessUrl = $"{domain}/dashboard/success",
                    CancelUrl = $"{domain}/dashboard/cancel",
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(
                    new
                    {
                        Message = "Le paiement est prêt",
                        status = 201,
                        Data = new { sessionId = session.Id, url = session.Url },
                    }
                );
            }
            catch
            {
                return BadRequest(
                    new { Message = "Impossible de préparer le paiement", status = 400 }
                );
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];
            Console.WriteLine("json : " + json +  "        signature " + signatureHeader);

            bool result = await paymentsService.CheckPaymentAndUpdateOrder(json, signatureHeader);

            if(result)
            {                
                return Ok();
            }
            return BadRequest();            
        }
    }

    public class CheckoutRequest
    {
        public string? ItemName { get; set; }
        public decimal? Amount { get; set; }
        public Guid OrderId { get; set; }
    }
}

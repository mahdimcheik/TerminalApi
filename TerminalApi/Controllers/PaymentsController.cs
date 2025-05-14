using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des paiements via Stripe.
    /// Ce contrôleur permet de créer des sessions de paiement et de gérer les webhooks Stripe.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly PaymentsService paymentsService;
        private readonly JobChron jobChron;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur PaymentsController.
        /// </summary>
        /// <param name="context">Contexte de base de données utilisé pour accéder aux entités.</param>
        /// <param name="paymentsService">Service de gestion des paiements injecté.</param>
        public PaymentsController(ApiDefaultContext context, PaymentsService paymentsService, JobChron jobChron)
        {
            StripeConfiguration.ApiKey = EnvironmentVariables.STRIPE_SECRETKEY;
            this.context = context;
            this.paymentsService = paymentsService;
            this.jobChron = jobChron;
        }

        /// <summary>
        /// Crée une session de paiement Stripe pour un utilisateur étudiant.
        /// </summary>
        /// <param name="request">Objet contenant les informations nécessaires pour créer une session de paiement.</param>
        /// <returns>
        /// Un objet contenant l'ID de la session Stripe et l'URL de redirection en cas de succès.
        /// Retourne un code HTTP 201 si la session est créée avec succès.
        /// Retourne un code HTTP 400 si la demande est invalide ou si une erreur survient.
        /// </returns>
        /// <remarks>
        /// Cette méthode utilise le service PaymentsService pour valider la commande et Stripe pour créer la session.
        /// </remarks>
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

                var result = await paymentsService.CheckOrder(request.OrderId, user.Id);
                //if (!result.isValid)
                //{
                //    return BadRequest(
                //        new { Message = "Commande non valide, vérifiez la!", Status = 400 }
                //    );
                //}

                if(result.order is not null && !result.order.CheckoutID.IsNullOrEmpty())
                {
                    await jobChron.ExpireCheckout(result.order?.CheckoutID ?? "");
                    result.order.CheckoutID = null;
                    result.order.PaymentIntent = null;
                    result.order.Status = EnumBookingStatus.Pending;
                    await context.SaveChangesAsync();
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

                var tvaRate = context.TVARates.LastOrDefault()?.Rate ?? 0.2m;

                // Met à jour le statut de la commande dans la base de données
                result.order.Status = EnumBookingStatus.WaitingForPayment;
                result.order.CheckoutID = session.Id;
                result.order.CheckoutExpiredAt = session.ExpiresAt;
                result.order.UpdatedAt = DateTimeOffset.Now;
                result.order.TVARate = tvaRate;

                await context.SaveChangesAsync();

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

        /// <summary>
        /// Gère les webhooks Stripe pour mettre à jour l'état des paiements.
        /// </summary>
        /// <returns>
        /// Retourne un code HTTP 200 si le webhook est traité avec succès.
        /// Retourne un code HTTP 400 si une erreur survient.
        /// </returns>
        /// <remarks>
        /// Cette méthode utilise le service PaymentsService pour valider et mettre à jour les commandes en fonction des événements Stripe.
        /// </remarks>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];
            Console.WriteLine("json : " + json + "        signature " + signatureHeader);

            bool result = await paymentsService.CheckPaymentAndUpdateOrder(json, signatureHeader);

            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
    }

    /// <summary>
    /// Requête pour créer une session de paiement.
    /// </summary>
    public class CheckoutRequest
    {
        /// <summary>
        /// Nom de l'article à payer.
        /// </summary>
        public string? ItemName { get; set; }

        /// <summary>
        /// Montant à payer.
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Identifiant unique de la commande.
        /// </summary>
        public Guid OrderId { get; set; }
    }
}

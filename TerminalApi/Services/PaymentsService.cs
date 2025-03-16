using Bogus.Bson;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using Stripe.Checkout;
using TerminalApi.Contexts;
using TerminalApi.Models.Notification;
using TerminalApi.Models.Payments;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class PaymentsService
    {
        private readonly ApiDefaultContext context;
        private readonly OrderService orderService;
        private readonly NotificationService notificationService;

        public PaymentsService(
            ApiDefaultContext context,
            OrderService orderService,
            NotificationService notificationService
        )
        {
            this.context = context;
            this.orderService = orderService;
            this.notificationService = notificationService;
        }

        public async Task<(bool isValid, Order? order)> Checkorder(Guid orderId, string userId)
        {
            var order = await context
                .Orders.Include(o => o.Booker)
                .Include(o => o.Bookings)
                .ThenInclude(b => b.Slot)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return (false, null);
            }
            if (order.BookerId != userId || order.Status != EnumBookingStatus.Pending)
            {
                return (false, null);
            }
            if (order.Bookings is null || order.TotalOriginalPrice == 0)
            {
                return (false, null);
            }
            return (true, order);
        }

        public async Task<bool> CheckPaymentAndUpdateOrder(
            string json,
            StringValues signatureHeader
        )
        {
            try
            {
                string endpointSecret = EnvironmentVariables.STRIPE_SECRET_ENDPOINT_TEST ?? "";
                var stripeEvent = EventUtility.ParseEvent(json);

                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, endpointSecret);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    Console.WriteLine("********** {0} ********", json);

                    if (session.PaymentStatus == "paid") // Ensure payment is completed
                    {
                        (string? bookerId, string? orderId, string? orderNumber) orderIds;
                        if (session.Metadata.TryGetValue("order_id", out string orderId))
                        {
                            Console.WriteLine($"Payment Order ID: {orderId}");
                            orderIds.orderId = orderId;
                            // Update the order in your database as PAID
                        }
                        if (session.Metadata.TryGetValue("order_number", out string orderNumber))
                        {
                            Console.WriteLine($"Payment  number: {orderNumber}");
                            orderIds.orderNumber = orderNumber;
                            // Update the order in your database as PAID
                        }
                        if (session.Metadata.TryGetValue("booker_id", out string bookerId))
                        {
                            Console.WriteLine($"Payment successful for bookerId: {bookerId}");
                            orderIds.bookerId = bookerId;
                            // Update the order in your database as PAID
                        }

                        if (orderId is not null && session.PaymentIntentId is not null)
                        {
                            Guid orderGuid = Guid.Parse(orderId);
                            Order? newOrder = await context
                                .Orders.Where(x => x.Id == orderGuid)
                                .Include(x => x.Booker)
                                .FirstOrDefaultAsync();


                            if (
                                newOrder is not null
                                && newOrder.Status != EnumBookingStatus.Paid
                            )
                            {
                                // paiement accepté
                                await notificationService.AddNotification(
                                    new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        RecipientId = newOrder.Booker.Id,
                                        Type = EnumNotificationType.PaymentAccepted
                                    }
                                );
                                // réservation enregistrée
                                await notificationService.AddNotification(
                                    new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        RecipientId = newOrder.Booker.Id,
                                        Type = EnumNotificationType.ReservationAccepted
                                    }
                                );
                                // avertir le prof
                                await notificationService.AddNotification(
                                    new Notification
                                    {
                                        Id = Guid.NewGuid(),                                        
                                        SenderId = newOrder.Booker.Id,
                                        RecipientId = EnvironmentVariables.TEACHER_ID,
                                        Type = EnumNotificationType.NewReservation
                                    }
                                );

                                return await orderService.UpdateOrderStatus(
                                    orderGuid,
                                    EnumBookingStatus.Paid,
                                    session.PaymentIntentId
                                );
                            }
                        }
                    }
                    return false;
                }
                return false;
            }
            catch (StripeException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}

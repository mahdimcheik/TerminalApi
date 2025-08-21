using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Stripe;
using Stripe.Checkout;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly ApiDefaultContext context;
        private readonly IOrderService orderService;
        private readonly INotificationService notificationService;
        private readonly IJobChron jobChron;
        private readonly ISignalRNotificationService signalRService;

        public PaymentsService(
            ApiDefaultContext context,
            IOrderService orderService,
            INotificationService notificationService,
            IJobChron jobChron,
            ISignalRNotificationService signalRService
        )
        {
            this.context = context;
            this.orderService = orderService;
            this.notificationService = notificationService;
            this.jobChron = jobChron;
            this.signalRService = signalRService;
        }

        public async Task<(bool isValid, Order? order)> CheckOrder(Guid orderId, string userId)
        {
            var order = await context
                .Orders.Where(o => o.Id == orderId)
                .Include(o => o.Booker)
                .Include(o => o.Bookings)
                .ThenInclude(b => b.Slot)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return (false, null);
            }
            if (order.BookerId != userId || order.Status != EnumBookingStatus.Pending)
            {
                return (false, order);
            }
            if (order.Bookings is null || order.TotalOriginalPrice == 0)
            {
                return (false, order);
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

                    if (session.PaymentStatus == "paid")
                    {
                        (string? bookerId, string? orderId, string? orderNumber) orderIds;
                        if (session.Metadata.TryGetValue("order_id", out string orderId))
                        {
                            Console.WriteLine($"Payment Order ID: {orderId}");
                            orderIds.orderId = orderId;
                        }
                        if (session.Metadata.TryGetValue("order_number", out string orderNumber))
                        {
                            Console.WriteLine($"Payment  number: {orderNumber}");
                            orderIds.orderNumber = orderNumber;
                        }
                        if (session.Metadata.TryGetValue("booker_id", out string bookerId))
                        {
                            Console.WriteLine($"Payment successful for bookerId: {bookerId}");
                            orderIds.bookerId = bookerId;
                        }

                        if (orderId is not null && session.PaymentIntentId is not null)
                        {
                            Guid orderGuid = Guid.Parse(orderId);
                            Order? newOrder = await context
                                .Orders.Where(x => x.Id == orderGuid)
                                .Include(o => o.Bookings)
                                .Include(x => x.Booker)
                                .FirstOrDefaultAsync();

                            if (newOrder is not null && newOrder.Status != EnumBookingStatus.Paid)
                            {
                                // paiement accepté
                                await notificationService.AddNotification(
                                    new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        RecipientId = newOrder.Booker.Id,
                                        Type = EnumNotificationType.PaymentAccepted,
                                    }
                                );

                                // réservation enregistrée
                                foreach (var booking in newOrder.Bookings)
                                {
                                    
                                await notificationService.AddNotification(
                                    new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        RecipientId = newOrder.Booker.Id,
                                        Type = EnumNotificationType.ReservationAccepted,
                                        BookingId = booking.Id
                                    }
                                );

                                // avertir le prof
                                await notificationService.AddNotification(
                                    new Notification
                                    {
                                        Id = Guid.NewGuid(),
                                        SenderId = newOrder.Booker.Id,
                                        RecipientId = EnvironmentVariables.TEACHER_ID,
                                        Type = EnumNotificationType.NewReservation,
                                        BookingId = booking.Id
                                    }
                                );
                                }

                                // FIXED: Use SignalR notification service instead of ChatHub directly
                                var notificationMessage = new MessageDTO
                                {
                                    Content = "Payment successful! Your reservation is confirmed.",
                                    Type = MessageTypeEnum.Notification,
                                    Timestamp = DateTime.UtcNow,
                                };

                                await signalRService.SendMessageByUserEmail(
                                    newOrder.Booker.Email,
                                    MessageTypeEnum.Notification,
                                    notificationMessage
                                );

                                await signalRService.SendMessageByUserEmail(
                                    EnvironmentVariables.TEACHER_EMAIL,
                                    MessageTypeEnum.Notification,
                                    notificationMessage
                                );

                                jobChron.CancelScheuledJob(newOrder.Id.ToString());

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

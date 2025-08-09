using System.Collections;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Utilities;
using TerminalApi.Interfaces;

namespace TerminalApi.Services
{
    public class JobChron : IJobChron
    {
        private readonly ApiDefaultContext _context;
        private readonly INotificationService notificationService;

        public static Hashtable ScheduleJobOrderTable { get; set; } = new();

        public JobChron(ApiDefaultContext context, INotificationService notificationService)
        {
            _context = context;
            this.notificationService = notificationService;
            //RecurringJob.AddOrUpdate("clean-database-orders", () => CleanOrders(), "*/10 * * * *");
            RecurringJob.AddOrUpdate("delete-passed-jobs", () => RemoveFinishedJobs(), "*/10 * * * *");
        }

        public async Task CleanOrders()
        {
            int delay = EnvironmentVariables.HANGFIRE_ORDER_CLEANING_DELAY;
            try
            {
                var orders = _context
                    .Orders.Include(x => x.Bookings)
                    .Where(x =>
                        x.Status == EnumBookingStatus.Pending
                        && x.Bookings.Count > 0
                        && (
                            x.UpdatedAt != null
                            && x.UpdatedAt < DateTimeOffset.UtcNow.AddMinutes(-1 * delay)
                        )
                    )
                    .ToList();
                foreach (var order in orders)
                {
                    order.Reset();

                    await notificationService.AddNotification(
                        new Notification
                        {
                            Id = Guid.NewGuid(),
                            SenderId = EnvironmentVariables.TEACHER_ID,
                            RecipientId = order.BookerId,
                            Type = EnumNotificationType.ReservationCancelledTimeOut
                        }
                    );
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void SchedulerSingleOrderCleaning(string orderId)
        {
            // j'annule l'ancien job avant de planifier un nouveau
            CancelScheuledJob(orderId);
            int delay = EnvironmentVariables.HANGFIRE_ORDER_CLEANING_DELAY;

            var jobId = BackgroundJob.Schedule(() => TrackOrder(orderId), TimeSpan.FromMinutes(delay));
            ScheduleJobOrderTable.Add(orderId, jobId);
        }

        public void CancelScheuledJob(string orderId)
        {
            if (ScheduleJobOrderTable.Contains(orderId))
            {
                BackgroundJob.Delete(ScheduleJobOrderTable[orderId] as string);
                object _lock = new object();
                lock (_lock)
                {
                    ScheduleJobOrderTable.Remove(orderId);
                }
            }
        }

        public void RemoveFinishedJobs()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var keys = ScheduleJobOrderTable.Keys;

                var copy = DeepCopyScheduleJobOrderTable();

                foreach (DictionaryEntry item in copy)
                {
                    var jobId = ScheduleJobOrderTable[item.Key];
                    var jobData = connection.GetJobData(jobId as string);

                    if (jobData != null && jobData.State != "Scheduled" && jobData.State != "Processing")
                    {
                        ScheduleJobOrderTable.Remove(item.Key);
                        BackgroundJob.Delete(jobId as string);
                    }
                }
            }

        }

        public async Task TrackOrder(string orderId)
        {
            try
            {
                Guid.TryParse(orderId, out Guid orderGuid);
                var order = _context
                    .Orders.Include(x => x.Bookings)
                    .FirstOrDefault(x => x.Id == orderGuid);

                if (order is not null)
                {
                    _context.RemoveRange(
                       order.Bookings.Where(x => x.OrderId == orderGuid
                       )
                   );

                    order.Reset();

                    await _context.SaveChangesAsync();

                    try
                    {
                        if (order.CheckoutID.IsNullOrEmpty())
                        {
                            await ExpireCheckout(order.CheckoutID);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task ExpireCheckout(string checkoutId)
        {
            try
            {

                if (checkoutId.IsNullOrEmpty())
                {
                    throw new Exception("checkout est null");
                }

                StripeConfiguration.ApiKey = EnvironmentVariables.STRIPE_SECRET_KEY;
                var service = new Stripe.Checkout.SessionService();

                Stripe.Checkout.Session session = service.Expire(checkoutId);
            }
            catch
            {
            }
        }
        public Hashtable DeepCopyScheduleJobOrderTable()
        {
            var deepCopy = new Hashtable();
            foreach (DictionaryEntry entry in ScheduleJobOrderTable)
            {
                deepCopy.Add(entry.Key, entry.Value);
            }
            return deepCopy;
        }
    }
}

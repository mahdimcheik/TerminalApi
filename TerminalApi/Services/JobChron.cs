using Hangfire;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using TerminalApi.Contexts;
using TerminalApi.Models.Notification;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class JobChron
    {
        private readonly ApiDefaultContext _context;
        private readonly NotificationService notificationService;

        public static Hashtable ScheduleJobOrderTable { get; set; } = new();

        public JobChron(ApiDefaultContext context, NotificationService notificationService)
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
                    order.Status = EnumBookingStatus.Pending;
                    if (order.UpdatedAt is null)
                    {
                        order.UpdatedAt = DateTimeOffset.UtcNow;
                    }

                    _context.RemoveRange(
                        order.Bookings.Where(x =>
                            x.CreatedAt < DateTimeOffset.UtcNow.AddMinutes(-1 * delay)
                        )
                    );

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
                ScheduleJobOrderTable.Remove(orderId);
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

        public void TrackOrder(string orderId)
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

                    order.PaymentIntent = null;
                    order.PaymentMethod = "";
                    order.CheckoutID = "";
                    order.Status = EnumBookingStatus.Pending;
                    order.UpdatedAt = DateTimeOffset.Now;

                    if (order.CheckoutID is not null)
                    {

                    }

                    _context.SaveChanges();
                }

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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

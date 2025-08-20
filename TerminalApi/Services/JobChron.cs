using System.Collections;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Utilities;
using TerminalApi.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace TerminalApi.Services
{
    public class JobChron : IJobChron
    {
        private readonly ApiDefaultContext _context;
        private readonly INotificationService notificationService;
        private readonly ConnectionManager _connectionManager;
        private readonly IServiceProvider _serviceProvider;

        public static Hashtable ScheduleJobOrderTable { get; set; } = new();

        public JobChron(
            ApiDefaultContext context, 
            INotificationService notificationService,
            ConnectionManager connectionManager,
            IServiceProvider serviceProvider)
        {
            _context = context;
            this.notificationService = notificationService;
            _connectionManager = connectionManager;
            _serviceProvider = serviceProvider;
            
            // Existing recurring jobs
            RecurringJob.AddOrUpdate("delete-passed-jobs", () => RemoveFinishedJobs(), "*/15 * * * *");
            
            // NEW: Add SignalR connection cleanup job - runs every 2 minutes
            RecurringJob.AddOrUpdate("cleanup-dead-connections", () => CleanupDeadSignalRConnections(), "*/2 * * * *");
        }

        // NEW: SignalR Connection cleanup method
        public async Task CleanupDeadSignalRConnections()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();

                var connections = _connectionManager.GetAllConnections().ToList();
                var deadConnections = new List<string>();
                var initialCount = connections.Count;

                foreach (var connection in connections)
                {
                    try
                    {
                        // Try to ping the connection - if it throws, connection is dead
                        await hubContext.Clients.Client(connection.Key).SendAsync("ping");
                    }
                    catch
                    {
                        // Connection is dead
                        deadConnections.Add(connection.Key);
                    }
                }

                // Remove all dead connections
                foreach (var deadConnectionId in deadConnections)
                {
                    _connectionManager.RemoveConnection(deadConnectionId);
                }

                if (deadConnections.Count > 0)
                {
                    Console.WriteLine($"[Hangfire] Cleaned up {deadConnections.Count} dead SignalR connections. Active connections: {initialCount - deadConnections.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Hangfire] Error during SignalR connection cleanup: {ex.Message}");
                throw;
            }
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

        // RESTORED: TrackOrder method
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
                       order.Bookings.Where(x => x.OrderId == orderGuid)
                   );

                    order.Reset();

                    await _context.SaveChangesAsync();

                    try
                    {
                        if (!order.CheckoutID.IsNullOrEmpty())
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

        // RESTORED: ExpireCheckout method
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
                // Silent catch as per original implementation
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

using System.Threading.Tasks.Dataflow;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Notification;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class JobChron
    {
        private readonly ApiDefaultContext _context;
        private readonly NotificationService notificationService;

        public JobChron(ApiDefaultContext context, NotificationService notificationService)
        {
            _context = context;
            this.notificationService = notificationService;
            RecurringJob.AddOrUpdate("clean-database-orders", () => CleanOrders(), "*/10 * * * *");
        }

        public async Task CleanOrders()
        {
            int delay = 30; // default value
            var gotDelayed = int.TryParse( EnvironmentVariables.HANGFIRE_ORDER_CLEANING_DELAY, out delay);
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
    }
}

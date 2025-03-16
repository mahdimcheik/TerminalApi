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
            RecurringJob.AddOrUpdate("clean-database-orders", () => CleanOrders(), "*/2 * * * *");
        }

        public async Task CleanOrders()
        {
            try
            {
                var orders = _context
                    .Orders.Where(x =>
                        x.Status == EnumBookingStatus.Pending
                        && (
                            x.UpdatedAt == null
                            || x.UpdatedAt < DateTimeOffset.UtcNow.AddMinutes(-2)
                        )
                    )
                    .Include(x => x.Bookings)
                    .ToList();
                foreach (var order in orders)
                {
                    order.Status = EnumBookingStatus.Cancelled;
                    if (order.UpdatedAt is null)
                    {
                        order.UpdatedAt = DateTimeOffset.UtcNow;
                    }

                    _context.RemoveRange(order.Bookings);

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

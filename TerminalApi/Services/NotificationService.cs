using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuppeteerSharp;
using TerminalApi.Contexts;
using TerminalApi.Models.Notification;
using TerminalApi.Models.Payments;

namespace TerminalApi.Services
{
    public class NotificationService
    {
        private readonly ApiDefaultContext context;

        public NotificationService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task<Notification> AddNotification(Notification notification)
        {
            if (notification is null)
            {
                throw new Exception("Erreur lors de la création de la notification");
            }
            try
            {
                // véridier si l'utilisateur existe
                if (notification.SenderId is not null && !notification.SenderId.IsNullOrEmpty())
                {
                    var sender = await context.Users.FirstOrDefaultAsync(u =>
                        u.Id == notification.SenderId
                    );
                    if (sender is null)
                    {
                        throw new Exception("Erreur lors de la création de la notification");
                    }
                }
                // verifier si le recipient existe
                if (
                    notification.RecipientId is not null
                    && !notification.RecipientId.IsNullOrEmpty()
                )
                {
                    var recipient = await context.Users.FirstOrDefaultAsync(u =>
                        u.Id == notification.RecipientId
                    );
                    if (recipient is null)
                    {
                        throw new Exception("Erreur lors de la création de la notification");
                    }
                }

                // verifier si la réservation existe
                if (notification.BookingId is not null)
                {
                    var booking = await context.Bookings.FirstOrDefaultAsync(u =>
                        u.Id == notification.BookingId
                    );
                    if (booking is null)
                    {
                        throw new Exception("Erreur lors de la création de la notification");
                    }
                }

                // verifier si la commande existe
                if (notification.BookingId is not null)
                {
                    var order = await context.Orders.FirstOrDefaultAsync(u =>
                        u.Id == notification.OrderId
                    );
                    if (order is null)
                    {
                        throw new Exception("Erreur lors de la création de la notification");
                    }
                }

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
                return notification;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<Notification> ToggleNotification(Guid notificationId)
        {
            var notification = await context.Notifications.FirstOrDefaultAsync(u =>
                u.Id == notificationId
            );
            if (notification is null)
            {
                throw new Exception("Erreur lors de la création de la notification");
            }
            try
            {
                notification.IsRead = !notification.IsRead;
                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
                return notification;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task DeleteNotification(Guid notificationId)
        {
            var notification = await context.Notifications.FirstOrDefaultAsync(u =>
                u.Id == notificationId
            );
            if (notification is null)
            {
                throw new Exception("Erreur lors de la création de la notification");
            }
            try
            {
                context.Notifications.Remove(notification);
                await context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PaginatedResult<Notification>> GetNotifications(string userId, NotificationFilter filter)
        {
            var query = context.Notifications
                .Where(n => n.RecipientId == userId);

            if (filter.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == filter.IsRead.Value);
            }

            var totalItems = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(filter.Offset)
                .Take(filter.PerPage)
                .ToListAsync();

            return new PaginatedResult<Notification>
            {
                Items = notifications,
                TotalItems = totalItems,
                offset = filter.Offset,
                PerPage = filter.PerPage
            };
        }
    }
    public class NotificationFilter
    {
        public bool? IsRead { get; set; }
        public int Offset { get; set; } = 0;
        public int PerPage { get; set; } = 10;
    }
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int offset { get; set; }
        public int PerPage { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models.Notification;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class NotificationService
    {
        private readonly ApiDefaultContext context;

        public NotificationService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task<NotificationResponseDTO> AddNotification(
            Notification notification,
            string? customDescription = null
        )
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

                // Composer la déscription de la notification

                if (customDescription is null)
                {
                    switch (notification.Type)
                    {
                        case EnumNotificationType.AccountConfirmed:
                            notification.Description = "Votre compte vient d'être confirmé";
                            break;
                        case EnumNotificationType.AccountCreated:
                            notification.Description = "Votre compte vient d'être créé";
                            break;
                        case EnumNotificationType.NewAnnouncement:
                            notification.Description = "Nouvelle annonce / Offres";
                            break;
                        case EnumNotificationType.MessageReceived:
                            notification.Description = "Vous avez reçu un message";
                            break;
                        case EnumNotificationType.GeneralReminder:
                            notification.Description =
                                "Rappel: vous avez un rendez-vous aujourd'hui";
                            break;
                        case EnumNotificationType.PasswordChanged:
                            notification.Description = "Votre mot de passe a été modifié";
                            break;
                        case EnumNotificationType.PaymentAccepted:
                            notification.Description = "Votre paiement a été accepté";
                            break;
                        case EnumNotificationType.PaymentFailed:
                            notification.Description = "Votre paiement a échoué";
                            break;
                        case EnumNotificationType.PromotionOffer:
                            notification.Description = "Offre promotionnelle";
                            break;
                        case EnumNotificationType.RefundProcessed:
                            notification.Description =
                                "Votre remboursement a été traité, le montantd emandé sera versé prochainement ";
                            break;
                        case EnumNotificationType.ReservationAccepted:
                            notification.Description = "Votre réservation a été acceptée";
                            break;
                        case EnumNotificationType.ReservationCancelled:
                            notification.Description = "Votre réservation a été annulée";
                            break;
                        case EnumNotificationType.ReservationRejected:
                            notification.Description = "Votre réservation a été rejetée";
                            break;
                        case EnumNotificationType.ReservationReminder:
                            notification.Description =
                                "Rappel: vous avez un rendez-vous aujourd'hui";
                            break;
                        case EnumNotificationType.ReviewReceived:
                            notification.Description = "Vous avez reçu un avis";
                            break;
                        case EnumNotificationType.SystemUpdate:
                            notification.Description =
                                "Mise à jour du système prérvu le : 21/04/1986";
                            break;
                        default:
                            notification.Description = "Nouvelle notification";
                            break;
                    }
                }
                else
                {
                    notification.Description = customDescription;
                }

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
                return notification.ToRespsonseDTO();
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

        public async Task<PaginatedNotificationResult<NotificationResponseDTO>> GetNotifications(
            string userId,
            NotificationFilter filter
        )
        {
            var query = context.Notifications.Where(n => n.RecipientId == userId);

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

            return new PaginatedNotificationResult<NotificationResponseDTO>
            {
                Items = notifications.Select(x => x.ToRespsonseDTO()).ToList(),
                TotalItems = totalItems,
                offset = filter.Offset,
                PerPage = filter.PerPage
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
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
                throw new Exception("Erreur lors de la cr�ation de la notification");
            }

            try
            {
                await ValidateNotificationEntities(notification);

                notification.Description =
                    customDescription ?? GetDefaultDescription(notification.Type);

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();
                return notification.ToRespsonseDTO();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task ValidateNotificationEntities(Notification notification)
        {
            if (!string.IsNullOrEmpty(notification.SenderId))
            {
                var sender = await context.Users.FirstOrDefaultAsync(u =>
                    u.Id == notification.SenderId
                );
                if (sender is null)
                {
                    throw new Exception("Erreur lors de la cr�ation de la notification");
                }
            }

            if (!string.IsNullOrEmpty(notification.RecipientId))
            {
                var recipient = await context.Users.FirstOrDefaultAsync(u =>
                    u.Id == notification.RecipientId
                );
                if (recipient is null)
                {
                    throw new Exception("Erreur lors de la cr�ation de la notification");
                }
            }

            if (notification.BookingId.HasValue)
            {
                var booking = await context.Bookings.FirstOrDefaultAsync(u =>
                    u.Id == notification.BookingId
                );
                if (booking is null)
                {
                    throw new Exception("Erreur lors de la cr�ation de la notification");
                }
            }

            if (notification.OrderId.HasValue)
            {
                var order = await context.Orders.FirstOrDefaultAsync(u =>
                    u.Id == notification.OrderId
                );
                if (order is null)
                {
                    throw new Exception("Erreur lors de la cr�ation de la notification");
                }
            }
        }

        private string GetDefaultDescription(EnumNotificationType? type)
        {
            return type switch
            {
                EnumNotificationType.AccountConfirmed => @"<span class=""text-surface-700 dark:text-surface-100"">Votre compte vient d'�tre confirm�</span> ",
                EnumNotificationType.AccountUpdated => @"<span class=""text-surface-700 dark:text-surface-100"">Votre compte vient d'�tre mis � jour</span> ",

                EnumNotificationType.PasswordResetDemandAccepted
                    => "Un email de r�initialisation de mot de passe vient d'�tre envoy�",
                EnumNotificationType.NewAnnouncement => "Nouvelle annonce / Offres",
                EnumNotificationType.MessageReceived => "Vous avez re�u un message",
                EnumNotificationType.GeneralReminder
                    => "Rappel: vous avez un rendez-vous aujourd'hui",
                EnumNotificationType.PasswordChanged => "Votre mot de passe a �t� modifi�",
                EnumNotificationType.PaymentAccepted => "Votre paiement a �t� accept�",
                EnumNotificationType.PaymentFailed => "Votre paiement a �chou�",
                EnumNotificationType.PromotionOffer => "Offre promotionnelle",
                EnumNotificationType.RefundProcessed
                    => "Votre remboursement a �t� trait�, le montant demand� sera vers� prochainement",
                EnumNotificationType.ReservationAccepted => "Votre r�servation a �t� accept�e",
                EnumNotificationType.NewReservation
                    => "Vous venez de recevoir une nouvelle commande",
                EnumNotificationType.ReservationCancelled => @"<span class=""text-surface-700 dark:text-surface-100"">Votre r�servation a �t�  <span class=""text-primary font-bold"">annul�e</span> </span> ",
                EnumNotificationType.ReservationCancelledTimeOut
                    => @"<span class=""text-surface-700 dark:text-surface-100"">Votre r�servation a �t� annul�e <span class=""text-primary font-bold"">pour abscence de paiement</span></span> ",
                EnumNotificationType.ReservationRejected => "Votre r�servation a �t� rejet�e",
                EnumNotificationType.ReservationReminder
                    => "Rappel: vous avez un rendez-vous aujourd'hui",
                EnumNotificationType.ReviewReceived => "Vous avez re�u un avis",
                EnumNotificationType.SystemUpdate
                    => "Mise � jour du syst�me pr�vue le : 21/04/1986",
                _ => "Nouvelle notification",
            };
        }

        public async Task<Notification> ToggleNotification(Guid notificationId)
        {
            var notification = await context.Notifications.FirstOrDefaultAsync(u =>
                u.Id == notificationId
            );
            if (notification is null)
            {
                throw new Exception("Erreur lors de la cr�ation de la notification");
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
                throw new Exception("Erreur lors de la cr�ation de la notification");
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

        public async Task<
            PaginatedNotificationResult<NotificationResponseDTO>
        > GetUserNotificationsAsync(string userId, NotificationFilter filter)
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

        public async Task<ResponseDTO<NotificationResponseDTO>> Update(Notification notification, bool newValue)
        {
            try
            {
                notification.IsRead = newValue;
                context.Notifications.Update(notification);

                await context.SaveChangesAsync();

                return new ResponseDTO<NotificationResponseDTO> {
                    Message = "Notification mise � jour",
                    Status = 200,
                    Data = notification.ToRespsonseDTO()
                };
            }
            catch
            {
                return new ResponseDTO<NotificationResponseDTO> {
                    Message = "Erreur lors de la mise � jour de la notification",
                    Status = 500
                };
            }
        }
    }
}

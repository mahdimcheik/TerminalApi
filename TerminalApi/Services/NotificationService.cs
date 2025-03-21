﻿using Microsoft.EntityFrameworkCore;
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
                await ValidateNotificationEntities(notification);

                notification.Description = customDescription ?? GetDefaultDescription(notification.Type);

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
                var sender = await context.Users.FirstOrDefaultAsync(u => u.Id == notification.SenderId);
                if (sender is null)
                {
                    throw new Exception("Erreur lors de la création de la notification");
                }
            }

            if (!string.IsNullOrEmpty(notification.RecipientId))
            {
                var recipient = await context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);
                if (recipient is null)
                {
                    throw new Exception("Erreur lors de la création de la notification");
                }
            }

            if (notification.BookingId.HasValue)
            {
                var booking = await context.Bookings.FirstOrDefaultAsync(u => u.Id == notification.BookingId);
                if (booking is null)
                {
                    throw new Exception("Erreur lors de la création de la notification");
                }
            }

            if (notification.OrderId.HasValue)
            {
                var order = await context.Orders.FirstOrDefaultAsync(u => u.Id == notification.OrderId);
                if (order is null)
                {
                    throw new Exception("Erreur lors de la création de la notification");
                }
            }
        }

        private string GetDefaultDescription(EnumNotificationType? type)
        {
            return type switch
            {
                EnumNotificationType.AccountConfirmed => "Votre compte vient d'être confirmé",
                EnumNotificationType.AccountUpdated => "Votre compte vient d'être mis à jour",
                EnumNotificationType.PasswordResetDemandAccepted => "Un email de réinitialisation de mot de passe vient d'être envoyé",
                EnumNotificationType.NewAnnouncement => "Nouvelle annonce / Offres",
                EnumNotificationType.MessageReceived => "Vous avez reçu un message",
                EnumNotificationType.GeneralReminder => "Rappel: vous avez un rendez-vous aujourd'hui",
                EnumNotificationType.PasswordChanged => "Votre mot de passe a été modifié",
                EnumNotificationType.PaymentAccepted => "Votre paiement a été accepté",
                EnumNotificationType.PaymentFailed => "Votre paiement a échoué",
                EnumNotificationType.PromotionOffer => "Offre promotionnelle",
                EnumNotificationType.RefundProcessed => "Votre remboursement a été traité, le montant demandé sera versé prochainement",
                EnumNotificationType.ReservationAccepted => "Votre réservation a été acceptée",
                EnumNotificationType.NewReservation => "Vous venez de recevoir une nouvelle commande",
                EnumNotificationType.ReservationCancelled => "Votre réservation a été annulée",
                EnumNotificationType.ReservationCancelledTimeOut => "Votre réservation a été annulée pour abscence de paiement",
                EnumNotificationType.ReservationRejected => "Votre réservation a été rejetée",
                EnumNotificationType.ReservationReminder => "Rappel: vous avez un rendez-vous aujourd'hui",
                EnumNotificationType.ReviewReceived => "Vous avez reçu un avis",
                EnumNotificationType.SystemUpdate => "Mise à jour du système prévue le : 21/04/1986",
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
        public async Task<PaginatedNotificationResult<NotificationResponseDTO>> GetUserNotificationsAsync(
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

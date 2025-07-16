using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDTO> AddNotification(Notification notification, string? customDescription = null);
        Task<Notification> ToggleNotification(Guid notificationId);
        Task DeleteNotification(Guid notificationId);
        Task<PaginatedNotificationResult<NotificationResponseDTO>> GetNotifications(string userId, NotificationFilter filter);
        Task<int> GetUserNotificationsCountAsync(string userId);
        Task<ResponseDTO<NotificationResponseDTO>> Update(Notification notification, bool newValue);
    }
} 
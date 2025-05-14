namespace TerminalApi.Models
{
    public static class NotificationExtension
    {
        public static NotificationResponseDTO ToRespsonseDTO(this Notification notification)
        {
            return new NotificationResponseDTO
            {
                Id = notification.Id,
                Description = notification.Description,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                SenderId = notification.SenderId,
                RecipientId = notification.RecipientId,
                BookingId = notification.BookingId,
                OrderId = notification.OrderId
            };
        }
    }
}

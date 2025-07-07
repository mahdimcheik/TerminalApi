using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class NotificationResponseDTO
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }

        public EnumNotificationType? Type { get; set; }

        public bool IsRead { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string? SenderId { get; set; }
        public string? RecipientId { get; set; }

        public Guid? BookingId { get; set; }
        public Guid? OrderId { get; set; }
    }
}

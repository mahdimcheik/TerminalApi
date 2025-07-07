using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class Notification
    {
        public Guid Id { get; set; }

        public string? Description { get; set; }

        public EnumNotificationType? Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public string? SenderId { get; set; }

        public UserApp? Sender { get; set; }

        public string? RecipientId { get; set; }

        public UserApp? Recipient { get; set; }

        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
    }
}

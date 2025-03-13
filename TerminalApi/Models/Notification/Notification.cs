using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Notification
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public EnumNotificationType? Type { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Sender))]
        public string? SenderId { get; set; }
        public UserApp? Sender { get; set; }

        [ForeignKey(nameof(Recipient))]
        public string? RecipientId { get; set; }
        public UserApp? Recipient { get; set; }

        [ForeignKey(nameof(Booking))]
        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }

        [ForeignKey(nameof(Order))]
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
    }

}

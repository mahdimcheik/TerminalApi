using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.Bookings;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid SlotId { get; set; }
        public Slot Slot { get; set; }
        public string BookedById { get; set; }
        public UserApp Booker { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public EnumTypeHelp TypeHelp { get; set; } = 0;
        public List<ChatMessage>? Communications { get; set; } = new List<ChatMessage>();
    }
}

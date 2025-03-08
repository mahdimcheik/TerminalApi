using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Bookings
{
    public class BookingMinimalDTO
    {
        public Guid Id { get; set; }
        public string? Subject { get; set; }
        public EnumTypeHelp? TypeHelp { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Precision(18, 2)]
        public decimal DiscountedPrice { get; set; }
        public int? Reduction { get; set; }

        // slot
        public Guid SlotId { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
    }
}

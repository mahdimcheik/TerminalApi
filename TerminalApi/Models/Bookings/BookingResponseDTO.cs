using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class BookingResponseDTO
    {
        //reservation
        public Guid Id { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public EnumTypeHelp? TypeHelp { get; set; }
        public Guid? OrderId { get; set; }
        public string? OrderNumber { get; set; }
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
        public EnumSlotType? Type { get; set; }

        //student
        public string? StudentId { get; set; }
        public string? StudentFirstName { get; set; }
        public string? StudentLastName { get; set; }
        public string? StudentImgUrl { get; set; }



    }
}

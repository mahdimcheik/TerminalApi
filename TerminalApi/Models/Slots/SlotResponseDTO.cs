using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Slots
{
    public class SlotResponseDTO
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }

        public DateTimeOffset EndAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string CreatedById { get; set; }
        public decimal Price { get; set; }
        public int? Reduction { get; set; }
        public EnumSlotType Type { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Slots
{
    public class SlotUpdateDTO
    {
        public string Id { get; set; }
        [Required]
        public DateTimeOffset StartAt { get; set; }

        [Required]
        public DateTimeOffset EndAt { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Price { get; set; }
        public int? Reduction { get; set; }
        public EnumSlotType Type { get; set; }
    }
}
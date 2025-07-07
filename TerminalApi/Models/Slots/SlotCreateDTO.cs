using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class SlotCreateDTO
    {
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset StartAt { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset EndAt { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public int? Reduction { get; set; }
        public EnumSlotType Type { get; set; }
    }
}

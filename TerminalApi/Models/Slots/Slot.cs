using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Slots
{
    public class Slot
    {
        [Key]
        public Guid Id { get; set; }

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
        [ForeignKey(nameof(Creator))]
        public string CreatedById { get; set; }
        public UserApp? Creator { get; set; }
        public Booking? Booking { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public int? Reduction { get; set; }
        public EnumSlotType Type { get; set; }

        // Calculated fields
        [Precision(18, 2)]
        [JsonIgnore]
        [NotMapped]
        public decimal DiscountedPrice => Price * (decimal)(1.0 -  0.01 * (Reduction ?? 0));
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class Slot
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string CreatedById { get; set; }
        public UserApp? Creator { get; set; }
        public Booking? Booking { get; set; }
        public decimal Price { get; set; }
        public int? Reduction { get; set; }
        public EnumSlotType Type { get; set; }
        // Calculated fields
        [Precision(18, 2)]
        [JsonIgnore]
        [NotMapped]
        public decimal DiscountedPrice => Price * (decimal)(1.0 - 0.01 * (Reduction ?? 0));
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Payments
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset? PaymentDate { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset? CreatedAt { get; set; }
        public EnumBookingStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public UserApp Booker { get; set; }
        [ForeignKey(nameof(Booker))]
        public string BookerId { get; set; }
        [Precision(18, 2)]
        public decimal TVARate { get; set; }

        public string? PaymentIntent { get; set; }

        // Calculated fields
        [JsonIgnore]
        [NotMapped]
        public decimal TotalOriginalPrice
        {
            get
            {
                if (Bookings == null || !Bookings.Any())
                    return 0;
                return Bookings.Sum(booking => booking.Slot.Price);
            }
        }

        [JsonIgnore]
        [NotMapped]
        public decimal TotalDiscountedPrice
        {
            get
            {
                if (Bookings == null || !Bookings.Any())
                    return 0;
                return Bookings.Sum(booking => booking.Slot.DiscountedPrice);
            }
        }

        [JsonIgnore]
        [NotMapped]
        public decimal TotalReduction
        {
            get
            {
                if (Bookings == null || !Bookings.Any())
                    return 0;
                return TotalOriginalPrice - TotalDiscountedPrice;
            }
        }
    }
}

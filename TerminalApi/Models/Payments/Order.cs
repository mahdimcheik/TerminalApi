using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class Order
    {    
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
       
        public DateTimeOffset? PaymentDate { get; set; }
        public string? CheckoutID { get; set; }

        public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public EnumBookingStatus Status { get; set; }
        public string PaymentMethod { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public UserApp Booker { get; set; }
  
        public string BookerId { get; set; }

        public decimal TVARate { get; set; }

        public string? PaymentIntent { get; set; }

        public DateTimeOffset? CheckoutExpiredAt { get; set; }

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

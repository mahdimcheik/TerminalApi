using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.Bookings;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Payments
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public DateTimeOffset? PaymentDate { get; set; }
        public DateTimeOffset? PCreatedAt { get; set; }
        public EnumBookingStatus Status { get; set; }  
        public string PaymentMethod { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Payments
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public long OrderNumber { get; set; }
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
    }
}

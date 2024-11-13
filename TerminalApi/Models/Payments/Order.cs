using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.Bookings;

namespace TerminalApi.Models.Payments
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? PCreatedAt { get; set; }
        public string Status { get; set; }  
        public string PaymentMethod { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}

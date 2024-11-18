using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TerminalApi.Models.User;
using TerminalApi.Models.Slots;
using TerminalApi.Models.Payments;

namespace TerminalApi.Models.Bookings
{
    public class Booking
    {
        [Key]
        public Guid Id { get; set; }
        public string Status { get; set; }

        [Required]
        public DateTimeOffset BookedAt { get; set; }
        [Required]
        [ForeignKey(nameof(Slot))]
        [InverseProperty("Slots")]
        public Guid SlotId { get; set; }
        public Slot Slot { get; set; }

        [Required]
        [ForeignKey(nameof(Booker))]
        [InverseProperty("Bookings")]
        public string BookedById { get; set; }
        public UserApp Booker { get; set; }
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
        [Required]
        [ForeignKey(nameof (Order))]
        public Guid? OrderId { get; set; }
        public Order?  Order { get; set; }
    }
}

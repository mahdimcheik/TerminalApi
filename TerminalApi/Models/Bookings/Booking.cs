using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TerminalApi.Models.User;
using TerminalApi.Models.Slots;
using TerminalApi.Models.Payments;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Bookings
{
    public class Booking
    {
        [Key]
        public Guid Id { get; set; }
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
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset CreatedAt { get; set; }
        
        [ForeignKey(nameof (Order))]
        public Guid? OrderId { get; set; }
        public Order?  Order { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
        public EnumTypeHelp TypeHelp { get; set; } = 0;
    }
}

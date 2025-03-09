using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TerminalApi.Models.Payments;
using TerminalApi.Models.Slots;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Models.Bookings
{
    public class BookingCreateDTO
    {
        public Guid Id { get; set; }

        public string SlotId { get; set; }
        [MaxLength(64)]
        public string? Subject { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }
        public EnumTypeHelp? TypeHelp { get; set; } = 0;

    }
}

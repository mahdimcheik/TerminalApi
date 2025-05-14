using System.ComponentModel.DataAnnotations;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class BookingCreateDTO
    {
        public Guid Id { get; set; }

        public string SlotId { get; set; }
        [MaxLength(64)]
        [Required]
        public string? Subject { get; set; }
        [MaxLength(255)]
        public string? Description { get; set; }
        public EnumTypeHelp? TypeHelp { get; set; } = 0;

    }
}

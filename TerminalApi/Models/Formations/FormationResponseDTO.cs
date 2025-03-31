using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models.Formations
{
    public class FormationResponseDTO
    {
        public Guid Id { get; set; }
        public string Company { get; set; } = string.Empty;


        public string Title { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Country { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset StartAt { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset EndAt { get; set; }
        public string? UserId { get; set; }
    }
}

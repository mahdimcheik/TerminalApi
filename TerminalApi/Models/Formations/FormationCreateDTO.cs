using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Utilities.Validators;

namespace TerminalApi.Models
{
    public class FormationCreateDTO
    {
        [Required]
        public string Company { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(255)]
        public string? City { get; set; }
        [MaxLength(255)]
        public string? Country { get; set; }
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset StartAt { get; set; }
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        [DateRange("StartAt")]
        public DateTimeOffset EndAt { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models
{
    public class Formation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Company { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset StartAt { get; set; }
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset EndAt { get; set; }
        public string UserId { get; set; }
        [MaxLength(255)]
        public string? City { get; set; }
        [MaxLength(255)]
        public string? Country { get; set; }

        [ForeignKey("UserId")]
        public UserApp? User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.User;

namespace TerminalApi.Models.Formations
{
    public class Formation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Company { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(125)")]
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string UserId { get; set; }
        public string?  City { get; set; }
        public string? Country { get; set; }

        [ForeignKey("UserId")]
        public UserApp? User { get; set; }
    }
}

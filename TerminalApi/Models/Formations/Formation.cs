using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models
{
    public class Formation
    {
        public Guid Id { get; set; }
        public string Company { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public DateTimeOffset StartAt { get; set; }

        public DateTimeOffset EndAt { get; set; }
        public string UserId { get; set; }

        public string? City { get; set; }

        public string? Country { get; set; }

        public UserApp? User { get; set; }
    }
}

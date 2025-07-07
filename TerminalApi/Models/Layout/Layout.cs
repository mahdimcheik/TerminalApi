using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models
{
    public class Layout
    {
        public Guid Id { get; set; }

        public string? Preset { get; set; }

        public string? Surface { get; set; }

        public string? MenuMode { get; set; }
        public bool? DarkTheme { get; set; } = false;

        public string UserId { get; set; }
        public UserApp User { get; set; }
    }
}

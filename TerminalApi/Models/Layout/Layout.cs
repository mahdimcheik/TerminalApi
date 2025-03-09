using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TerminalApi.Models.User;

namespace TerminalApi.Models.Layout
{
    public class Layout
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(50)]
        public string? Preset { get; set; }
        [StringLength(50)]
        public string? Surface{ get; set; }
        [StringLength(50)]
        public string? MenuMode{ get; set; }
        public bool? DarkTheme  { get; set; } = false;
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }
        public UserApp User { get; set; }
    }
}

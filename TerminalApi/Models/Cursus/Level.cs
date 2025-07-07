using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Models
{
    public class Level
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(50)]
        public string? Icon { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        // Navigation properties
        public ICollection<Cursus>? Cursus { get; set; }
    }
} 
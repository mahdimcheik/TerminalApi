using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerminalApi.Models
{
    public class Cursus
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Level))]
        public Guid LevelId { get; set; }
        public Level Level { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Category))]
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        [Required]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? UpdatedAt { get; set; }
    }
} 
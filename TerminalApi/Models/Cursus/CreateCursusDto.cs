using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Models
{
    public class CreateCursusDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        public Guid LevelId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }
    }
} 
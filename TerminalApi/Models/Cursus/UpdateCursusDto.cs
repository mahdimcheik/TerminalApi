using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Models
{
    public class UpdateCursusDto

    {
        [Required]
        public Guid Id { get; set; }
        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public Guid? LevelId { get; set; }

        public Guid? CategoryId { get; set; }
    }
} 
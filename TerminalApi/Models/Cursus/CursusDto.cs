namespace TerminalApi.Models
{
    public class CursusDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid LevelId { get; set; }
        public Level Level { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
} 
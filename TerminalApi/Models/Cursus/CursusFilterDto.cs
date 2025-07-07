namespace TerminalApi.Models
{
    public class CursusFilterDto
    {
        public string? SearchTerm { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? CategoryId { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 10;
    }
} 
namespace TerminalApi.Models
{
    public class QueryPagination
    {
        public int Start { get; set; }
        public int PerPage { get; set; }
        public string? StudentId { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public short? OrderByName { get; set; } = 0;
        public short? OrderByDate { get; set; } = 0;
    }
}

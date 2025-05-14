namespace TerminalApi.Models
{
    public class PaginatedNotificationResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int offset { get; set; }
        public int PerPage { get; set; }
    }
}

namespace TerminalApi.Models
{
    public class ResponsePagination<T>
    {
        public int Count { get; set; }
        public List<T> DataList { get; set; } = [];
    }
}

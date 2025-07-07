namespace TerminalApi.Models
{
    public class ResponseDTO<T>
    {
        public string? Message { get; set; }
        public int? Status { get; set; }
        public T? Data { get; set; }
        public long? Count { get; set; }
    }
}

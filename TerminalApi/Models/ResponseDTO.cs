namespace TerminalApi.Models
{
    public class ResponseDTO
    {
        public string? Message { get; set; }
        public int? Status { get; set; }
        public object? Data { get; set; }
        public long? Count { get; set; }
    }
}

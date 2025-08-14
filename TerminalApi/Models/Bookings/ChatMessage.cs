namespace TerminalApi.Models.Bookings
{
   
        public class ChatMessage
        {
        public DateTimeOffset? Date { get; set; } = DateTime.UtcNow;
            public string? Author { get; set; }
            public string? userId { get; set; }
            public string Message { get; set; }        
    }
}

namespace TerminalApi.Models.Notification
{
    public class NotificationFilter
    {
        public bool? IsRead { get; set; }
        public int Offset { get; set; } = 0;
        public int PerPage { get; set; } = 10;
    }
}

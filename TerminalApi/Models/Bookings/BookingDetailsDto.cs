namespace TerminalApi.Models.Bookings
{
    public class BookingDetailsDto
    {
        public DateTimeOffset BookingCreatedAt { get; set; }
        public DateTimeOffset SlotStartAt { get; set; }
        public DateTimeOffset SlotEndAt { get; set; }
    }
}

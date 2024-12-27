using TerminalApi.Models.Bookings;

namespace TerminalApi.Models.Payments
{
    public class OrderDetailsDto
    {
        public string BookerImgUrl { get; set; }
        public List<BookingDetailsDto> Bookings { get; set; }
    }
}

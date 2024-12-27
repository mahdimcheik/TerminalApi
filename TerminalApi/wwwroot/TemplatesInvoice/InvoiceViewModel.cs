using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Models.User;

namespace TerminalApi.Services.Templates
{
    public class InvoiceViewModel
    {
        public string BookerImgUrl { get; set; }
        public decimal Total { get; set; }
        public List<BookingDetailsDto> Bookings { get; set; }

        public InvoiceViewModel(OrderDetailsDto orderDetails)
        {
            Total = 15;
            BookerImgUrl = orderDetails.BookerImgUrl;
            Bookings = orderDetails.Bookings;
        }
    }
}

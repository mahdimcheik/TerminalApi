using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;

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

    public class ValidationMailTemplateViewModel
    {
        public string ValidationLink { get; set; }
        public string WebsiteLink { get; set; }
        public ValidationMailTemplateViewModel(string validation, string site)
        {
            ValidationLink = validation;
            WebsiteLink = site;
        }
    }
}

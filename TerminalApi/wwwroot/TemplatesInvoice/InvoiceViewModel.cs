using TerminalApi.Models;

namespace TerminalApi.Services.Templates
{
    public class InvoiceViewModel
    {
        public string BookerImgUrl { get; set; }
        public decimal Total { get; set; }
        public List<BookingDetailsDto> Bookings { get; set; }

        public InvoiceViewModel(OrderResponseForTeacherDTO orderDetails)
        {
            Total = 15;
            BookerImgUrl = "" + orderDetails.OrderNumber;
            Bookings = new();
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

    public class InvoiceModel
    {
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem
    {
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}

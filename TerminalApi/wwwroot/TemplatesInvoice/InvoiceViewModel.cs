using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Models.User;

namespace TerminalApi.Services.Templates
{
    public class InvoiceViewModel
    {
        public decimal Total { get; set; }

        public InvoiceViewModel()
        {
            Total = 105;
        }
    }
}

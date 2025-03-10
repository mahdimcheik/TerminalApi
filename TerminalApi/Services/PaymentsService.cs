using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Payments;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class PaymentsService
    {
        private readonly ApiDefaultContext context;

        public PaymentsService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task<(bool isValid, Order? order)> Checkorder(Guid orderId, string userId)
        {

            var order = await context.Orders
                .Include(o => o.Booker)
                .Include(o => o.Bookings)
                .ThenInclude(b => b.Slot)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return (false, null);
            }
            if (order.BookerId != userId || order.Status != EnumBookingStatus.Pending)
            {
                return (false, null);
            }
            if (order.Bookings is null || order.TotalOriginalPrice == 0)
            {
                return (false, null);
            }
            return (true, order);
        }


    }
}

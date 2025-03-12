using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;
using TerminalApi.Contexts;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class JobChron
    {
        private readonly ApiDefaultContext _context;

        public JobChron(ApiDefaultContext context)
        {
            _context = context;
        }

        public async Task CleanOrders()
        {
            try
            {
                var orders = _context
                    .Orders.Where(x =>
                        x.Status != EnumBookingStatus.Paid
                        && (x.UpdatedAt == null || x.UpdatedAt < DateTimeOffset.UtcNow.AddMinutes(-5))
                    ).Include(x => x.Bookings)
                    .ToList();
                foreach (var order in orders)
                {
                    order.Status = EnumBookingStatus.Cancelled;
                    if (order.UpdatedAt is null)
                    {
                        order.UpdatedAt = DateTimeOffset.UtcNow;
                    }

                    _context.RemoveRange(order.Bookings);
                    
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}

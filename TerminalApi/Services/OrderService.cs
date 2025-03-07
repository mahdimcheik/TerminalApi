using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models.Payments;

namespace TerminalApi.Services
{
    public class OrderService
    {
        private readonly ApiDefaultContext context;

        public OrderService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            return await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order> GetOrCreateCurrentOrderByUserAsync(string userId)
        {
            var order = await context.Orders.FirstOrDefaultAsync(o => o.BookerId == userId && o.Status == Utilities.EnumBookingStatus.Pending);
            if (order == null)
            {
                Order newOrder = new Order
                {
                    BookerId = userId,
                    Status = Utilities.EnumBookingStatus.Pending,
                    CreatedAt = DateTimeOffset.Now
                };
                context.Orders.Add(newOrder);
                context.SaveChanges();
                return newOrder;
            }
            else
            {
                return order;
            }
        }

    }
}

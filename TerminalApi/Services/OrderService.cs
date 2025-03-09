using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TerminalApi.Contexts;
using TerminalApi.Models.Payments;
using TerminalApi.Models.User;

namespace TerminalApi.Services
{
    public class OrderService
    {
        private readonly ApiDefaultContext context;

        public OrderService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public async Task<OrderResponseForStudentDTO?> GetOrderByStudentAsync(Guid orderId)
        {
            var result = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (result is not null)
            {
                return result.ToOrderResponseForStudentDTO();
            }
            return null;
        }

        public async Task<OrderResponseForStudentDTO?> GetOrderByTeacherAsync(Guid orderId)
        {
            var result = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (result is not null)
            {
                return result.ToOrderResponseForTeacherDTO();
            }
            return null;
        }

        public async Task<OrderResponseForStudentDTO> GetOrCreateCurrentOrderByUserAsync(
            UserApp user
        )
        {
            //Stopwatch stopwatch = Stopwatch.StartNew();
            var order = await context
                .Orders
                .AsSplitQuery()
                .AsNoTracking()
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot)
                .FirstOrDefaultAsync(o =>
                    o.BookerId == user.Id && o.Status == Utilities.EnumBookingStatus.Pending
                );
            //Console.WriteLine("Times : " + stopwatch.ElapsedMilliseconds + "ms" );
            if (order == null)
            {
                Order newOrder = new Order
                {
                    BookerId = user.Id,
                    Status = Utilities.EnumBookingStatus.Pending,
                    CreatedAt = DateTimeOffset.Now,
                    PaymentMethod = "card"
                };
                newOrder.OrderNumber = await GenerateOrderNumberAsync();
                
                context.Orders.Add(newOrder);
                context.SaveChanges();
                newOrder.Booker = user;
                return newOrder.ToOrderResponseForStudentDTO();
            }
            else
            {
                order.Booker = user;
                return order.ToOrderResponseForStudentDTO();
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            int count = await context.Orders.CountAsync(o => o.CreatedAt.Value.Date == DateTimeOffset.UtcNow);
            int nextNumber = count + 1;

            return $"INSPIRE-{datePart}-{nextNumber:D5}";
        }
    }
}

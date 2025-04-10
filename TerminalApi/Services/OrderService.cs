using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Payments;
using TerminalApi.Models.TVA;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

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
            Order? order;
            //Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {

            order = await context
                .Orders
                .AsSplitQuery()
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot)
                .FirstOrDefaultAsync(o =>
                    o.BookerId == user.Id && o.Status == Utilities.EnumBookingStatus.Pending
                );
            }
            catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            //Console.WriteLine("Times : " + stopwatch.ElapsedMilliseconds + "ms" );
            if (order == null)
            {
                Order newOrder = new Order
                {
                    BookerId = user.Id,
                    Status = Utilities.EnumBookingStatus.Pending,
                    CreatedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now,
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
                order.UpdatedAt = DateTimeOffset.Now;
                return order.ToOrderResponseForStudentDTO();
            }
        }

        public async Task<bool> UpdateOrderStatus(Guid orderId, EnumBookingStatus newStatus, string paymentIntent)
        {
            var order = await context
               .Orders
               //.AsSplitQuery()
               //.AsNoTracking()
               .FirstOrDefaultAsync(o =>
                   o.Id == orderId && o.Status == Utilities.EnumBookingStatus.Pending
               );
            if(order is null)
            {
                return false;
            }
            order.Status = newStatus;
            order.PaymentIntent = paymentIntent;
            order.UpdatedAt = DateTimeOffset.Now;
            context.SaveChanges();
            return true;
        }
        public async Task<ResponseDTO> GetOrdersForStudentPaginatedAsync(OrderPagination query, UserApp user)
        {
            var sqlQuery = context
                .Orders
                .AsSplitQuery()
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot)
                .Where(x => x.BookerId == user.Id);
            if (query.FromDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate >= query.FromDate.Value);
            }
            if (query.ToDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate <= query.ToDate.Value);
            }
            var count = await sqlQuery.CountAsync();
            List<OrderResponseForStudentDTO>? result = await sqlQuery
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToOrderResponseForStudentDTO())
                .ToListAsync();
            return new ResponseDTO
            {
                Message = "Demande acceptée",
                Status = 200,
                Count = count,
                Data =  result 
            };
        }

        public async Task<ResponseDTO> GetOrdersForTeacherPaginatedAsync(OrderPagination query, UserApp user)
        {
            var sqlQuery = context
                .Orders
                .AsSplitQuery()
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot) as IQueryable<Order>;

            if(query.BookerId is not null )
            {
                sqlQuery = sqlQuery.Where(x => x.BookerId == query.BookerId);
            }else 
            {
                if (!string.IsNullOrWhiteSpace(query.SearchField))
                {
                    string search = query.SearchField.ToLower();
                    sqlQuery = sqlQuery
                        .Include(x => x.Booker)
                        .Where(x =>
                            EF.Functions.ILike(x.Booker.FirstName, $"%{search}%") ||
                            EF.Functions.ILike(x.Booker.LastName, $"%{search}%") ||
                            EF.Functions.ILike(x.Booker.Email, $"%{search}%"));
                }
            }
            if (query.FromDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate >= query.FromDate.Value);
            }
            if (query.ToDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate <= query.ToDate.Value);
            }

            var count = await sqlQuery.CountAsync();
            List<OrderResponseForStudentDTO>? result = await sqlQuery
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToOrderResponseForTeacherDTO())
                .ToListAsync();
            return new ResponseDTO
            {
                Message = "Demande acceptée",
                Status = 200,
                Count = count,
                Data = result
            };
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            int count = await context.Orders.CountAsync(o => o.CreatedAt.Value.Date == DateTimeOffset.UtcNow);
            int nextNumber = count + 1;

            return $"SKILLHIVE-{datePart}-{nextNumber:D5}";
        }

        private TVARate GetTVARate()
        {
            return context.TVARates.OrderByDescending(x => x.StartAt).FirstOrDefault();
        }
    }    

}

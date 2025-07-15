using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Utilities;
using TerminalApi.Interfaces;

namespace TerminalApi.Services
{
    public class OrderService : IOrderService
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

        public async Task<OrderResponseForTeacherDTO?> GetOrderByTeacherAsync(Guid orderId)
        {
            var result = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (result is not null)
            {
                return result.ToOrderResponseForTeacherDTO();
            }
            return null;
        }

        public async Task<OrderResponseForStudentDTO> UpdateOrderAsync(UserApp user, Guid orderId)
        {
            Order? order;
            try
            {
                order = await context.Orders.FirstOrDefaultAsync(o =>
                    o.BookerId == user.Id && o.Id == orderId
                );

                if (order == null)
                {
                    throw new Exception("La commande n'existe pas");
                }

                order.UpdatedAt = DateTimeOffset.UtcNow;
                order.CheckoutID = null;
                order.Status = Utilities.EnumBookingStatus.Pending;
                await context.SaveChangesAsync();
                return order.ToOrderResponseForStudentDTO();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<Order> GetOrCreateCurrentOrderByUserAsync(
            UserApp user
        )
        {
            Order? order;
            try
            {
                order = await context
                    .Orders.Where(o =>
                        o.BookerId == user.Id && (o.Status == Utilities.EnumBookingStatus.Pending || o.Status == Utilities.EnumBookingStatus.WaitingForPayment)
                    )
                    .Include(x => x.Bookings)
                    .ThenInclude(x => x.Slot)
                    .FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            if (order == null)
            {
                Order newOrder = new Order
                {
                    BookerId = user.Id,
                    Status = Utilities.EnumBookingStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    PaymentMethod = "card",
                };
                newOrder.OrderNumber = await GenerateOrderNumberAsync();

                context.Orders.Add(newOrder);
                newOrder.Booker = user;
                await context.SaveChangesAsync();
                return newOrder;
            }
            else
            {
                order.Booker = user;
                await context.SaveChangesAsync();
                return order;
            }
        }

        public async Task<bool> UpdateOrderStatus(
            Guid orderId,
            EnumBookingStatus newStatus,
            string paymentIntent
        )
        {
            var order = await context.Orders.FirstOrDefaultAsync(o =>
                o.Id == orderId && o.Status == Utilities.EnumBookingStatus.WaitingForPayment
            );
            if (order is null)
            {
                return false;
            }
            order.Status = newStatus;
            order.PaymentIntent = paymentIntent;
            order.UpdatedAt = DateTimeOffset.UtcNow;
            if (newStatus == EnumBookingStatus.Paid)
            {
                order.PaymentDate = DateTimeOffset.UtcNow;
            }
            context.SaveChanges();
            return true;
        }

        public async Task<ResponseDTO<List<OrderResponseForStudentDTO>>?> GetOrdersForStudentPaginatedAsync(
            OrderPagination query,
            UserApp user
        )
        {
            var sqlQuery = context
                .Orders.Where(x => x.PaymentDate != null)
                .Where(x => x.BookerId == user.Id)
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot)
                .AsSplitQuery();

            if (query.FromDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate >= query.FromDate.Value);
            }
            if (query.ToDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate <= query.ToDate.Value);
            }
            if (query.SearchField is not null && !query.SearchField.Trim().IsNullOrEmpty())
            {
                sqlQuery = sqlQuery.Where(x =>
                    EF.Functions.ILike(x.OrderNumber, $"%{query.SearchField}%")
                );
            }

            var count = await sqlQuery.CountAsync();

            List<OrderResponseForStudentDTO>? result = await sqlQuery
                .OrderByDescending(x => x.PaymentDate)
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToOrderResponseForStudentDTO())
                .ToListAsync();
            return new ResponseDTO<List<OrderResponseForStudentDTO>> {
                Count = count,
                Message = "Demande accept�e",
                Data = result,
                Status = 200,
            };
        }

        public async Task<List<OrderResponseForTeacherDTO>> GetOrdersForTeacherPaginatedAsync(
            OrderPagination query
        )
        {
            var sqlQuery =
                context.Orders.AsSplitQuery().Include(x => x.Bookings).ThenInclude(x => x.Slot)
                as IQueryable<Order>;

            if (query.BookerId is not null)
            {
                sqlQuery = sqlQuery.Where(x => x.BookerId == query.BookerId);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(query.SearchField))
                {
                    string search = query.SearchField.ToLower();
                    sqlQuery = sqlQuery
                        .Include(x => x.Booker)
                        .Where(x =>
                            EF.Functions.ILike(x.Booker.FirstName, $"%{search}%")
                            || EF.Functions.ILike(x.Booker.LastName, $"%{search}%")
                            || EF.Functions.ILike(x.Booker.Email, $"%{search}%")
                        );
                }
            }

            if (query.Status is not null)
            {
                sqlQuery = sqlQuery.Where(re => re.Status == query.Status);
            }

            if (query.FromDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate >= query.FromDate.Value);
            }
            if (query.ToDate.HasValue)
            {
                sqlQuery = sqlQuery.Where(re => re.PaymentDate <= query.ToDate.Value);
            }

            if (query.OrderByDate is not null && query.OrderByDate != 0)
            {
                if (query.OrderByDate == 1)
                {
                    sqlQuery = sqlQuery.OrderBy(x => x.PaymentDate);
                }
                else
                {
                    sqlQuery = sqlQuery.OrderByDescending(x => x.PaymentDate);
                }
            }

            if (query.PerPage == 0)
            {
                query.PerPage = 10;
            }

            var count = await sqlQuery.CountAsync();
            List<OrderResponseForTeacherDTO>? result = await sqlQuery
                .Skip(query.Start)
                .Take(query.PerPage)
                .Select(re => re.ToOrderResponseForTeacherDTO())
                .ToListAsync();
            return result;
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

            //int count = await context.Orders.CountAsync(o =>
            //    o.CreatedAt.Value.Date == DateTimeOffset.UtcNow.Date
            //);
            //int nextNumber = count + 1;
            var id = Guid.NewGuid().ToString()[..8];

            return $"SKILLHIVE-{datePart}-{id:D8}";
        }

        private TVARate GetTVARate()
        {
            return context.TVARates.OrderByDescending(x => x.StartAt).FirstOrDefault();
        }
    }
}

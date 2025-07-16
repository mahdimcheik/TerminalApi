using TerminalApi.Models;
using TerminalApi.Utilities;

namespace TerminalApi.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponseForStudentDTO?> GetOrderByStudentAsync(Guid orderId);
        Task<OrderResponseForTeacherDTO?> GetOrderByTeacherAsync(Guid orderId);
        Task<OrderResponseForStudentDTO> UpdateOrderAsync(UserApp user, Guid orderId);
        Task<Order> GetOrCreateCurrentOrderByUserAsync(UserApp user);
        Task<bool> UpdateOrderStatus(Guid orderId, EnumBookingStatus newStatus, string paymentIntent);
        Task<ResponseDTO<List<OrderResponseForStudentDTO>>?> GetOrdersForStudentPaginatedAsync(OrderPagination query, UserApp user);
        Task<List<OrderResponseForTeacherDTO>> GetOrdersForTeacherPaginatedAsync(OrderPagination query);
        Task<string> GenerateOrderNumberAsync();
    }
} 
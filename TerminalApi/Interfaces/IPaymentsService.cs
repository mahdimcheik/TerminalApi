using Microsoft.Extensions.Primitives;
using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IPaymentsService
    {
        Task<(bool isValid, Order? order)> CheckOrder(Guid orderId, string userId);
        Task<bool> CheckPaymentAndUpdateOrder(string json, StringValues signatureHeader);
    }
} 
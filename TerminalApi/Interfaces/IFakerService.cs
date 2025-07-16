using Bogus;
using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IFakerService
    {
        Faker<UserCreateDTO> GenerateUserCreateDTO();
        Task GenerateBookingCreateDTO();
        Task CreateOrdersFixtureAsync();
        Task<string> GenerateOrderNumberAsync();
    }
} 
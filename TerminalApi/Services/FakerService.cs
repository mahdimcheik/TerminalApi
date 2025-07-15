using Bogus;
using Bogus.Extensions;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Utilities;
using TerminalApi.Interfaces;

namespace TerminalApi.Services
{
    public class FakerService : IFakerService
    {
        private readonly ApiDefaultContext context;

        public FakerService(ApiDefaultContext context)
        {
            this.context = context;
        }

        public Faker<UserCreateDTO> GenerateUserCreateDTO()
        {
            return new Faker<UserCreateDTO>()
                .RuleFor(u => u.Email, f => f.Internet.Email()) // Generate a random email
                .RuleFor(u => u.Password, f => f.Internet.Password(8)) // Minimum 8-character password
                .RuleFor(u => u.FirstName, f => f.Name.FirstName()) // Random first name
                .RuleFor(u => u.LastName, f => f.Name.LastName()) // Random last name
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("+###-###-###-####")) // Optional phone number
                .RuleFor(u => u.Description, f => f.Lorem.Paragraph()) // Optional random description
                .RuleFor(u => u.Title, f => f.Name.JobTitle()) // Optional job title
                .RuleFor(u => u.Gender, f => f.PickRandom<EnumGender>()) // Pick a random EnumGender
                .RuleFor(u => u.DateOfBirth, f => f.Date.Past(30, DateTime.Now.AddYears(-18)).ToUniversalTime()); // Between 18 and 30 years ago
        }

        public Faker<FormationCreateDTO> GenerateFormationsCreateDTO()
        {
            return new Faker<FormationCreateDTO>()
                .RuleFor(u => u.City, f => f.Address.City())
                .RuleFor(u => u.Company, f => f.Company.CompanyName())
                .RuleFor(u => u.Country, f => f.Address.Country())
                .RuleFor(u => u.EndAt, f => f.Date.Recent(0).ToUniversalTime())
                .RuleFor(u => u.StartAt, f => f.Date.Recent(0).ToUniversalTime())
                .RuleFor(u => u.Title, f => f.Name.JobTitle());
        }

        public Faker<AddressCreateDTO> GenerateAddresseCreateDTO()
        {
            return new Faker<AddressCreateDTO>()
                .RuleFor(u => u.City, f => f.Address.City().ClampLength(max: 49))
                .RuleFor(u => u.StreetLine2, f => f.Address.CardinalDirection())
                .RuleFor(u => u.Country, f => f.Address.Country().ClampLength(max: 49))
                .RuleFor(u => u.Street, f => f.Address.StreetName())
                .RuleFor(u => u.PostalCode, f => f.Address.ZipCode().ClampLength(max: 19))
                .RuleFor(u => u.AddressType, f => f.PickRandom<AddressTypeEnum>());
        }

        public Faker<SlotCreateDTO> GenerateSlotCreateDTO()
        {
            return new Faker<SlotCreateDTO>()
                .RuleFor(u => u.Price, f => f.PickRandom<decimal>(40, 100))
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1, DateTime.Now).ToUniversalTime())
                .RuleFor(u => u.StartAt, f => f.Date.Past(1, DateTime.Now.AddDays(-10)).ToUniversalTime())
                .RuleFor(u => u.EndAt, (f, t) => t.StartAt.AddHours(1))
                .RuleFor(u => u.Reduction, f => f.PickRandom(0, 50));
        }

        public async Task GenerateBookingCreateDTO()
        {
            var slots = await context.Slots.ToListAsync();
            var users = await context.Users.Where(x => x.Id != "1577fcf3-35a3-42fb-add1-daffcc56f6401577fcf3-35a3-42fb-add1-daffcc56f640").ToListAsync();

            int i = 0;
            foreach (var user in users.Take(100))
            {
                var order = context.Orders.FirstOrDefault(x => x.BookerId == user.Id);

                var booking1 = new BookingCreateDTO()
                {
                    Id = Guid.NewGuid(),
                    SlotId = slots[i].Id.ToString(),
                    Subject = "test" + i,
                    Description = "description " + i,
                    TypeHelp = 0
                };
                i++;
                var booking2 = new BookingCreateDTO()
                {
                    Id = Guid.NewGuid(),
                    SlotId = slots[i].Id.ToString(),
                    Subject = "test" + i,
                    Description = "description " + i,
                    TypeHelp = 0
                }; i++;
                var booking3 = new BookingCreateDTO()
                {
                    Id = Guid.NewGuid(),
                    SlotId = slots[i].Id.ToString(),
                    Subject = "test" + i,
                    Description = "description " + i,
                    TypeHelp = 0
                }; i++;

                context.Bookings.Add(booking1.ToBooking(user.Id, order.Id));
                context.Bookings.Add(booking2.ToBooking(user.Id, order.Id));
                context.Bookings.Add(booking3.ToBooking(user.Id, order.Id));
            }

            await context.SaveChangesAsync();
        }

        public async Task CreateOrdersFixtureAsync()
        {
            var users = await context.Users.Where(x => x.Id != "1577fcf3-35a3-42fb-add1-daffcc56f6401577fcf3-35a3-42fb-add1-daffcc56f640").ToListAsync();

            foreach (var user in users)
            {
                var days = new Random().Next(1, 100);
                Order newOrder = new Order
                {
                    BookerId = user.Id,
                    Status = Utilities.EnumBookingStatus.Pending,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-days),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-days),
                    PaymentMethod = "card"
                };
                newOrder.OrderNumber = await GenerateOrderNumberAsync();

                context.Orders.Add(newOrder);

            }

            foreach (var user in users.Take(100))
            {
                var days = new Random().Next(1, 100);
                Order newOrder = new Order
                {
                    BookerId = user.Id,
                    Status = Utilities.EnumBookingStatus.Paid,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-days),
                    UpdatedAt = DateTimeOffset.UtcNow.AddDays(-days),
                    PaymentMethod = "card"
                };
                newOrder.OrderNumber = await GenerateOrderNumberAsync();

                context.Orders.Add(newOrder);

            }

            context.SaveChanges();
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

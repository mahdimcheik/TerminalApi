using Bogus;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class FakerService
    {
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
    }
}

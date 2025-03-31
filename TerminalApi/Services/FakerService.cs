using Bogus;
using Bogus.Extensions;
using System.ComponentModel.DataAnnotations;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Formations;
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
                .RuleFor(u => u.City, f => f.Address.City().ClampLength(max:49))                
                .RuleFor(u => u.StreetLine2, f => f.Address.CardinalDirection())
                .RuleFor(u => u.Country, f => f.Address.Country().ClampLength(max: 49))
                .RuleFor(u => u.Street, f => f.Address.StreetName())
                .RuleFor(u => u.PostalCode, f => f.Address.ZipCode().ClampLength(max: 19))
                .RuleFor(u => u.AddressType, f => f.PickRandom<AddressTypeEnum>());
        }
    }
}

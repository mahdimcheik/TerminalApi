using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;

namespace TerminalTest
{
    public class AddressServiceTests : IDisposable
    {
        private readonly ApiDefaultContext _context;
        private readonly AddressService _addressService;
        private readonly EncryptionService _encryptionService;

        public AddressServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDefaultContext(options, true);
            _encryptionService = new EncryptionService(); 

            _addressService = new AddressService(_context, _encryptionService);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task GetAddresses_WithValidUserId_ReturnsAddresses()
        {
            // Arrange
            var userId = "user123";
            var address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StreetNumber = 123,
                Street = "Main St",
                StreetLine2 = "Apt 1",
                City = "Paris",
                State = "Ile-de-France",
                PostalCode = "75001",
                Country = "France",
                AddressType = AddressTypeEnum.Home
            };

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            // Act
            var result = await _addressService.GetAddresses(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(address.Street, result[0].Street);
        }

        [Fact]
        public async Task GetAddresses_WithNonExistentUserId_ReturnsEmptyList()
        {
            // Act
            var result = await _addressService.GetAddresses("nonexistent");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAddress_WithValidData_AddsAddress()
        {
            // Arrange
            var userId = "user123";
            var addressCreateDTO = new AddressCreateDTO
            {
                StreetNumber = 123,
                Street = "Main St",
                StreetLine2 = "Apt 1",
                City = "Paris",
                State = "Ile-de-France",
                PostalCode = "75001",
                Country = "France",
                AddressType = AddressTypeEnum.Home
            };

            // Act
            var result = await _addressService.AddAddress(addressCreateDTO, userId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(addressCreateDTO.Street.Length, result.Street.Length);
        }

        [Fact]
        public async Task AddAddress_WithMaxAddresses_ThrowsException()
        {
            // Arrange
            var userId = "user123";
            for (int i = 0; i < 5; i++)
            {
                await _context.Addresses.AddAsync(new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    StreetNumber = i,
                    Street = $"Street {i}",
                    StreetLine2 = $"Apt {i}",
                    City = "Paris",
                    State = "Ile-de-France",
                    PostalCode = "75001",
                    Country = "France",
                    AddressType = AddressTypeEnum.Home
                });
            }
            await _context.SaveChangesAsync();

            var addressCreateDTO = new AddressCreateDTO
            {
                StreetNumber = 123,
                Street = "Main St",
                StreetLine2 = "Apt 1",
                City = "Paris",
                State = "Ile-de-France",
                PostalCode = "75001",
                Country = "France",
                AddressType = AddressTypeEnum.Home
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _addressService.AddAddress(addressCreateDTO, userId));
        }

        [Fact]
        public async Task DeleteAddress_WithValidAddress_DeletesSuccessfully()
        {
            // Arrange
            var userId = "user123";
            var address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StreetNumber = 123,
                Street = "Main St",
                StreetLine2 = "Apt 1",
                City = "Paris",
                State = "Ile-de-France",
                PostalCode = "75001",
                Country = "France",
                AddressType = AddressTypeEnum.Home
            };

            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();

            // Act
            var result = await _addressService.DeleteAddress(userId, address.Id.ToString());

            // Assert
            Assert.True(result);
            var deletedAddress = await _context.Addresses.FindAsync(address.Id);
            Assert.Null(deletedAddress);
        }

        [Fact]
        public async Task DeleteAddress_WithNonExistentAddress_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _addressService.DeleteAddress("user123", Guid.NewGuid().ToString()));
        }
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;
using Xunit;

namespace TerminalTest
{
    public class OrderServiceTests : IDisposable
    {
        private readonly ApiDefaultContext _context;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDefaultContext(options);

            _orderService = new OrderService(_context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task GetOrderByStudentAsync_WithValidOrderId_ReturnsOrder()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "TEST-001",
                BookerId = "student-1",
                Status = EnumBookingStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrderByStudentAsync(order.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.OrderNumber, result.OrderNumber);
        }

        [Fact]
        public async Task GetOrderByStudentAsync_WithInvalidOrderId_ReturnsNull()
        {
            // Act
            var result = await _orderService.GetOrderByStudentAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrderByTeacherAsync_WithValidOrderId_ReturnsOrder()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "TEST-002",
                BookerId = "student-1",
                Status = EnumBookingStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrderByTeacherAsync(order.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.OrderNumber, result.OrderNumber);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithValidOrder_UpdatesOrder()
        {
            // Arrange
            var userId = "student-1";
            var user = new UserApp
            {
                Id = userId,
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "Test"
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "TEST-003",
                BookerId = userId,
                Status = EnumBookingStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Users.AddAsync(user);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.UpdateOrderAsync(user, order.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.OrderNumber, result.OrderNumber);
        }

        [Fact]
        public async Task UpdateOrderAsync_WithInvalidOrder_ThrowsException()
        {
            // Arrange
            var user = new UserApp
            {
                Id = "student-1",
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "Test"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _orderService.UpdateOrderAsync(user, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetOrCreateCurrentOrderByUserAsync_WithNewUser_CreatesNewOrder()
        {
            // Arrange
            var user = new UserApp
            {
                Id = "student-1",
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "Test"
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrCreateCurrentOrderByUserAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.BookerId);
            Assert.Equal(EnumBookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task GetOrCreateCurrentOrderByUserAsync_WithExistingOrder_ReturnsExistingOrder()
        {
            // Arrange
            var user = new UserApp
            {
                Id = "student-1",
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "Test"
            };

            var existingOrder = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "EXISTING-001",
                BookerId = user.Id,
                Status = EnumBookingStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Users.AddAsync(user);
            await _context.Orders.AddAsync(existingOrder);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrCreateCurrentOrderByUserAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingOrder.Id, result.Id);
            Assert.Equal(existingOrder.OrderNumber, result.OrderNumber);
        }

        [Fact]
        public async Task UpdateOrderStatus_WithValidOrder_UpdatesStatus()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                OrderNumber = "TEST-004",
                BookerId = "student-1",
                Status = EnumBookingStatus.WaitingForPayment,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.UpdateOrderStatus(orderId, EnumBookingStatus.Paid, "payment-intent-123");

            // Assert
            Assert.True(result);
            
            var updatedOrder = await _context.Orders.FindAsync(orderId);
            Assert.Equal(EnumBookingStatus.Paid, updatedOrder.Status);
            Assert.Equal("payment-intent-123", updatedOrder.PaymentIntent);
        }

        [Fact]
        public async Task UpdateOrderStatus_WithInvalidOrder_ReturnsFalse()
        {
            // Act
            var result = await _orderService.UpdateOrderStatus(Guid.NewGuid(), EnumBookingStatus.Paid, "payment-intent-123");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldGenerateValidOrderNumber()
        {
            // Act
            var orderNumber = await _orderService.GenerateOrderNumberAsync();

            // Assert
            Assert.NotNull(orderNumber);
            Assert.StartsWith("SKILLHIVE-", orderNumber);
            Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), orderNumber);
        }

        [Fact]
        public async Task GenerateOrderNumberAsync_ShouldGenerateUniqueNumbers()
        {
            // Act
            var orderNumber1 = await _orderService.GenerateOrderNumberAsync();
            var orderNumber2 = await _orderService.GenerateOrderNumberAsync();

            // Assert
            Assert.NotEqual(orderNumber1, orderNumber2);
        }

        [Fact]
        public async Task GetOrdersForStudentPaginatedAsync_WithValidUser_ReturnsOrders()
        {
            // Arrange
            var user = new UserApp
            {
                Id = "student-1",
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "Test"
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "TEST-005",
                BookerId = user.Id,
                Status = EnumBookingStatus.Paid,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentDate = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Users.AddAsync(user);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var query = new OrderPagination
            {
                Start = 0,
                PerPage = 10
            };

            // Act
            var result = await _orderService.GetOrdersForStudentPaginatedAsync(query, user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal(1, result.Count);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetOrdersForTeacherPaginatedAsync_WithValidQuery_ReturnsOrders()
        {
            // Arrange
            var user = new UserApp
            {
                Id = "student-1",
                Email = "student@example.com",
                FirstName = "Student",
                LastName = "Test"
            };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "TEST-006",
                BookerId = user.Id,
                Status = EnumBookingStatus.Paid,
                CreatedAt = DateTimeOffset.UtcNow,
                PaymentDate = DateTimeOffset.UtcNow,
                PaymentMethod = "card"
            };

            await _context.Users.AddAsync(user);
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var query = new OrderPagination
            {
                Start = 0,
                PerPage = 10
            };

            // Act
            var result = await _orderService.GetOrdersForTeacherPaginatedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
} 
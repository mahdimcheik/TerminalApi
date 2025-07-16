using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;
using Xunit;

namespace TerminalTest
{
    public class NotificationServiceTests : IDisposable
    {
        private readonly ApiDefaultContext _context;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDefaultContext(options);

            _notificationService = new NotificationService(_context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        private async Task SeedTestDataAsync()
        {
            var users = new[]
            {
                new UserApp { Id = "user-1", Email = "user1@example.com", FirstName = "User", LastName = "One" },
                new UserApp { Id = "user-2", Email = "user2@example.com", FirstName = "User", LastName = "Two" }
            };

            var notifications = new[]
            {
                new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientId = "user-1",
                    SenderId = "user-2",
                    Type = EnumNotificationType.AccountUpdated,
                    Description = "Account updated",
                    IsRead = false,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-1)
                },
                new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientId = "user-1",
                    SenderId = "user-2",
                    Type = EnumNotificationType.PaymentAccepted,
                    Description = "Payment accepted",
                    IsRead = true,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-2)
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddNotification_WithValidNotification_AddsSuccessfully()
        {
            // Arrange
            await SeedTestDataAsync();
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = "user-1",
                SenderId = "user-2",
                Type = EnumNotificationType.AccountUpdated,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Act
            var result = await _notificationService.AddNotification(notification);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(notification.Id, result.Id);
            Assert.Equal(notification.RecipientId, result.RecipientId);
            Assert.Equal(notification.SenderId, result.SenderId);
            Assert.Equal(notification.Type, result.Type);
            Assert.NotNull(result.Description);
            Assert.False(result.IsRead);
        }

        [Fact]
        public async Task AddNotification_WithCustomDescription_UsesCustomDescription()
        {
            // Arrange
            await SeedTestDataAsync();
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = "user-1",
                SenderId = "user-2",
                Type = EnumNotificationType.AccountUpdated,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var customDescription = "Custom notification description";

            // Act
            var result = await _notificationService.AddNotification(notification, customDescription);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customDescription, result.Description);
        }

        [Fact]
        public async Task AddNotification_WithNullNotification_ThrowsException()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _notificationService.AddNotification(null));
        }

        [Fact]
        public async Task AddNotification_WithNonExistentSender_ThrowsException()
        {
            // Arrange
            await SeedTestDataAsync();
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientId = "user-1",
                SenderId = "nonexistent-user",
                Type = EnumNotificationType.AccountUpdated,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _notificationService.AddNotification(notification));
        }

        [Fact]
        public async Task ToggleNotification_WithValidNotificationId_TogglesSuccessfully()
        {
            // Arrange
            await SeedTestDataAsync();
            var notification = await _context.Notifications.FirstAsync(n => !n.IsRead);

            // Act
            var result = await _notificationService.ToggleNotification(notification.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsRead);
        }

        [Fact]
        public async Task ToggleNotification_WithNonExistentNotification_ThrowsException()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _notificationService.ToggleNotification(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteNotification_WithValidNotificationId_DeletesSuccessfully()
        {
            // Arrange
            await SeedTestDataAsync();
            var notification = await _context.Notifications.FirstAsync();

            // Act
            await _notificationService.DeleteNotification(notification.Id);

            // Assert
            var deletedNotification = await _context.Notifications.FindAsync(notification.Id);
            Assert.Null(deletedNotification);
        }

        [Fact]
        public async Task DeleteNotification_WithNonExistentNotification_ThrowsException()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _notificationService.DeleteNotification(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetNotifications_WithValidUserId_ReturnsUserNotifications()
        {
            // Arrange
            await SeedTestDataAsync();
            var userId = "user-1";
            var filter = new NotificationFilter
            {
                Offset = 0,
                PerPage = 10
            };

            // Act
            var result = await _notificationService.GetNotifications(userId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, n => Assert.Equal(userId, n.RecipientId));
        }

        [Fact]
        public async Task GetNotifications_WithReadFilter_ReturnsOnlyReadNotifications()
        {
            // Arrange
            await SeedTestDataAsync();
            var userId = "user-1";
            var filter = new NotificationFilter
            {
                Offset = 0,
                PerPage = 10,
                IsRead = true
            };

            // Act
            var result = await _notificationService.GetNotifications(userId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Items);
            Assert.True(result.Items.First().IsRead);
        }

        [Fact]
        public async Task GetNotifications_WithUnreadFilter_ReturnsOnlyUnreadNotifications()
        {
            // Arrange
            await SeedTestDataAsync();
            var userId = "user-1";
            var filter = new NotificationFilter
            {
                Offset = 0,
                PerPage = 10,
                IsRead = false
            };

            // Act
            var result = await _notificationService.GetNotifications(userId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalItems);
            Assert.Single(result.Items);
            Assert.False(result.Items.First().IsRead);
        }

        [Fact]
        public async Task GetUserNotificationsCountAsync_WithValidUserId_ReturnsUnreadCount()
        {
            // Arrange
            await SeedTestDataAsync();
            var userId = "user-1";

            // Act
            var result = await _notificationService.GetUserNotificationsCountAsync(userId);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task GetUserNotificationsCountAsync_WithUserHavingNoUnreadNotifications_ReturnsZero()
        {
            // Arrange
            await SeedTestDataAsync();
            var userId = "user-2";

            // Act
            var result = await _notificationService.GetUserNotificationsCountAsync(userId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task Update_WithValidNotification_UpdatesSuccessfully()
        {
            // Arrange
            await SeedTestDataAsync();
            var notification = await _context.Notifications.FirstAsync();
            var originalReadStatus = notification.IsRead;

            // Act
            var result = await _notificationService.Update(notification, !originalReadStatus);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("Notification mise � jour", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(!originalReadStatus, result.Data.IsRead);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_WithValidUserId_ReturnsUserNotifications()
        {
            // Arrange
            await SeedTestDataAsync();
            var userId = "user-1";
            var filter = new NotificationFilter
            {
                Offset = 0,
                PerPage = 10
            };

            // Act
            var result = await _notificationService.GetUserNotificationsAsync(userId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalItems);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, n => Assert.Equal(userId, n.RecipientId));
        }

        [Fact]
        public async Task GetNotifications_WithEmptyDatabase_ReturnsEmptyResult()
        {
            // Arrange
            var userId = "user-1";
            var filter = new NotificationFilter
            {
                Offset = 0,
                PerPage = 10
            };

            // Act
            var result = await _notificationService.GetNotifications(userId, filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalItems);
            Assert.Empty(result.Items);
        }
    }
} 
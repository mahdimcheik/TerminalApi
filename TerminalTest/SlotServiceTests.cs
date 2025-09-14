using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Slots;
using TerminalApi.Services;
using TerminalApi.Utilities;
using Xunit;

namespace TerminalTest
{
    public class SlotServiceTests : IDisposable
    {
        private readonly ApiDefaultContext _context;
        private readonly SlotService _slotService;

        public SlotServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDefaultContext(options, true);

            _slotService = new SlotService(_context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        [Fact]
        public async Task AddSlot_WithValidData_AddsSlotSuccessfully()
        {
            // Arrange
            var userId = "teacher-1";
            var slotCreateDTO = new SlotCreateDTO
            {
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
                Price = 100.50m,
                Reduction = 10,
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Act
            var result = await _slotService.AddSlot(slotCreateDTO, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(slotCreateDTO.StartAt, result.StartAt);
            Assert.Equal(slotCreateDTO.EndAt, result.EndAt);
            Assert.Equal(slotCreateDTO.Price, result.Price);
            Assert.Equal(slotCreateDTO.Reduction, result.Reduction);
            Assert.Equal(EnumSlotType.Presentiel, result.Type);
            Assert.Equal(userId, result.CreatedById);
        }

        [Fact]
        public async Task UpdateSlot_WithValidData_UpdatesSlotSuccessfully()
        {
            // Arrange
            var userId = "teacher-1";
            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                CreatedById = userId,
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                Price = 50.00m,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            await _context.Slots.AddAsync(slot);
            await _context.SaveChangesAsync();

            var slotUpdateDTO = new SlotUpdateDTO
            {
                Id = slot.Id.ToString(),
                StartAt = DateTimeOffset.UtcNow.AddDays(2),
                EndAt = DateTimeOffset.UtcNow.AddDays(2).AddHours(2),
                Price = 100.00m,
                Reduction = 10,
                Type = EnumSlotType.Presentiel
            };

            // Act
            var result = await _slotService.UpdateSlot(slotUpdateDTO, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(slotUpdateDTO.StartAt, result.StartAt);
            Assert.Equal(slotUpdateDTO.EndAt, result.EndAt);
            Assert.Equal(slotUpdateDTO.Price, result.Price);
            Assert.Equal(slotUpdateDTO.Reduction, result.Reduction);
            Assert.Equal(slotUpdateDTO.Type, result.Type);
        }

        [Fact]
        public async Task UpdateSlot_WithNonExistentSlot_ThrowsException()
        {
            // Arrange
            var userId = "teacher-1";
            var slotUpdateDTO = new SlotUpdateDTO
            {
                Id = Guid.NewGuid().ToString(),
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                Price = 50.00m,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _slotService.UpdateSlot(slotUpdateDTO, userId));
        }

        [Fact]
        public async Task GetSlotsById_WithValidId_ReturnsSlot()
        {
            // Arrange
            var userId = "teacher-1";
            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                CreatedById = userId,
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                Price = 50.00m,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            await _context.Slots.AddAsync(slot);
            await _context.SaveChangesAsync();

            // Act
            var result = await _slotService.GetSlotsById(slot.Id.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(slot.Id.ToString(), result.Id.ToString());
            Assert.Equal(slot.StartAt, result.StartAt);
            Assert.Equal(slot.Price, result.Price);
        }

        [Fact]
        public async Task GetSlotsById_WithNonExistentId_ReturnsNull()
        {
            // Act
            var result = await _slotService.GetSlotsById(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSlotsByCreator_WithValidTeacher_ReturnsTeacherSlots()
        {
            // Arrange
            var teacherId = "teacher-1";
            var fromDate = DateTimeOffset.UtcNow.AddDays(1);
            var toDate = DateTimeOffset.UtcNow.AddDays(7);

            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                CreatedById = teacherId,
                StartAt = fromDate,
                EndAt = fromDate.AddHours(1),
                Price = 50,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            await _context.Slots.AddAsync(slot);
            await _context.SaveChangesAsync();

            // Act
            var result = await _slotService.GetSlotsByCreator(teacherId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(teacherId, result.First().CreatedById);
        }

        [Fact]
        public async Task GetSlotsByStudent_WithAvailableSlots_ReturnsSlots()
        {
            // Arrange
            var studentId = "student-1";
            var fromDate = DateTimeOffset.UtcNow.AddDays(1);
            var toDate = DateTimeOffset.UtcNow.AddDays(7);

            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                CreatedById = "teacher-1",
                StartAt = fromDate,
                EndAt = fromDate.AddHours(1),
                Price = 50,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            await _context.Slots.AddAsync(slot);
            await _context.SaveChangesAsync();

            // Act
            var result = await _slotService.GetSlotsByStudent(studentId, fromDate, toDate);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteSlot_WithValidSlot_DeletesSuccessfully()
        {
            // Arrange
            var userId = "teacher-1";
            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                CreatedById = userId,
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                Price = 50.00m,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            await _context.Slots.AddAsync(slot);
            await _context.SaveChangesAsync();

            // Act
            var result = await _slotService.DeleteSlot(userId, slot.Id.ToString());

            // Assert
            Assert.True(result);
            var deletedSlot = await _context.Slots.FindAsync(slot.Id);
            Assert.Null(deletedSlot);
        }

        [Fact]
        public async Task DeleteSlot_WithNonExistentSlot_ThrowsException()
        {
            // Arrange
            var userId = "teacher-1";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _slotService.DeleteSlot(userId, Guid.NewGuid().ToString()));
        }

        [Fact]
        public async Task DeleteSlot_WithPastSlot_ThrowsException()
        {
            // Arrange
            var userId = "teacher-1";
            var slot = new Slot
            {
                Id = Guid.NewGuid(),
                CreatedById = userId,
                StartAt = DateTimeOffset.UtcNow.AddDays(-1), // Past slot
                EndAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1),
                Price = 50.00m,
                Reduction = 5,
                Type = EnumSlotType.Presentiel
            };

            await _context.Slots.AddAsync(slot);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _slotService.DeleteSlot(userId, slot.Id.ToString()));
        }
    }
} 
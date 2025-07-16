using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using Xunit;

namespace TerminalTest
{
    public class CursusServiceTests : IDisposable
    {
        private readonly ApiDefaultContext _context;
        private readonly CursusService _cursusService;

        public CursusServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApiDefaultContext(options);

            _cursusService = new CursusService(_context);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        private async Task SeedTestDataAsync()
        {
            var level = new Level { Id = Guid.NewGuid(), Name = "Beginner" };
            var category = new Category { Id = Guid.NewGuid(), Name = "Programming" };

            await _context.Levels.AddAsync(level);
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyResult()
        {
            // Arrange
            await SeedTestDataAsync();
            var queryPagination = new QueryPagination { Start = 0, PerPage = 10 };

            // Act
            var (cursus, count) = await _cursusService.GetAllAsync(queryPagination);

            // Assert
            Assert.NotNull(cursus);
            Assert.Empty(cursus);
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetAllAsync_WithValidData_ReturnsCorrectResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var level = await _context.Levels.FirstAsync();
            var category = await _context.Categories.FirstAsync();

            var testCursus = new Cursus
            {
                Id = Guid.NewGuid(),
                Name = "Test Course",
                Description = "Test Description",
                LevelId = level.Id,
                CategoryId = category.Id
            };

            await _context.Cursus.AddAsync(testCursus);
            await _context.SaveChangesAsync();

            var queryPagination = new QueryPagination { Start = 0, PerPage = 10 };

            // Act
            var (cursus, count) = await _cursusService.GetAllAsync(queryPagination);

            // Assert
            Assert.NotNull(cursus);
            Assert.Single(cursus);
            Assert.Equal(1, count);
            Assert.Equal("Test Course", cursus.First().Name);
        }

        [Fact]
        public async Task CreateAsync_WithValidData_CreatesSuccessfully()
        {
            // Arrange
            await SeedTestDataAsync();
            var level = await _context.Levels.FirstAsync();
            var category = await _context.Categories.FirstAsync();

            var createDto = new CreateCursusDto
            {
                Name = "New Course",
                Description = "New Description",
                LevelId = level.Id,
                CategoryId = category.Id
            };

            // Act
            var result = await _cursusService.CreateAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Name, result.Name);
            Assert.Equal(createDto.Description, result.Description);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistentLevel_ThrowsException()
        {
            // Arrange
            await SeedTestDataAsync();
            var category = await _context.Categories.FirstAsync();

            var createDto = new CreateCursusDto
            {
                Name = "New Course",
                Description = "New Description",
                LevelId = Guid.NewGuid(),
                CategoryId = category.Id
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _cursusService.CreateAsync(createDto));
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            await SeedTestDataAsync();
            var level = await _context.Levels.FirstAsync();
            var category = await _context.Categories.FirstAsync();

            var cursus = new Cursus
            {
                Id = Guid.NewGuid(),
                Name = "Test Course",
                Description = "Test Description",
                LevelId = level.Id,
                CategoryId = category.Id
            };

            await _context.Cursus.AddAsync(cursus);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cursusService.DeleteAsync(cursus.Id);

            // Assert
            Assert.True(result);
            var deletedCursus = await _context.Cursus.FindAsync(cursus.Id);
            Assert.Null(deletedCursus);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var result = await _cursusService.DeleteAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            await SeedTestDataAsync();
            var level = await _context.Levels.FirstAsync();
            var category = await _context.Categories.FirstAsync();

            var cursus = new Cursus
            {
                Id = Guid.NewGuid(),
                Name = "Test Course",
                Description = "Test Description",
                LevelId = level.Id,
                CategoryId = category.Id
            };

            await _context.Cursus.AddAsync(cursus);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cursusService.ExistsAsync(cursus.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var result = await _cursusService.ExistsAsync(Guid.NewGuid());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetLevelsAsync_ReturnsAllLevels()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var levels = await _cursusService.GetLevelsAsync();

            // Assert
            Assert.NotNull(levels);
            Assert.Single(levels);
            Assert.Equal("Beginner", levels.First().Name);
        }

        [Fact]
        public async Task GetCategoriesAsync_ReturnsAllCategories()
        {
            // Arrange
            await SeedTestDataAsync();

            // Act
            var categories = await _cursusService.GetCategoriesAsync();

            // Assert
            Assert.NotNull(categories);
            Assert.Single(categories);
            Assert.Equal("Programming", categories.First().Name);
        }
    }
} 
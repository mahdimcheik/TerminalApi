using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Interfaces;

namespace TerminalTest
{
    public class FormationServiceTests : IDisposable
    {
        private readonly Mock<UserManager<UserApp>> _mockUserManager;
        private readonly ApiDefaultContext _context;
        private readonly FormationService _formationService;
        private readonly string _testUserId = "test-user-id";
        private readonly string _testFormationId = Guid.NewGuid().ToString();

        public FormationServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApiDefaultContext(options);
            
            // Setup mock UserManager
            var mockUserStore = new Mock<IUserStore<UserApp>>();
            _mockUserManager = new Mock<UserManager<UserApp>>(mockUserStore.Object, null, null, null, null, null, null, null, null);
            
            _formationService = new FormationService(_mockUserManager.Object, _context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region GetFormations Tests

        [Fact]
        public async Task GetFormations_WithValidUserId_ShouldReturnFormations()
        {
            // Arrange
            var formations = new List<Formation>
            {
                new Formation
                {
                    Id = Guid.NewGuid(),
                    Title = "Formation 1",
                    Company = "Company A",
                    StartAt = DateTimeOffset.UtcNow.AddDays(1),
                    EndAt = DateTimeOffset.UtcNow.AddDays(2),
                    UserId = _testUserId,
                    City = "Paris",
                    Country = "France"
                },
                new Formation
                {
                    Id = Guid.NewGuid(),
                    Title = "Formation 2",
                    Company = "Company B",
                    StartAt = DateTimeOffset.UtcNow.AddDays(3),
                    EndAt = DateTimeOffset.UtcNow.AddDays(4),
                    UserId = _testUserId,
                    City = "Lyon",
                    Country = "France"
                }
            };

            _context.Formations.AddRange(formations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _formationService.GetFormations(_testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, f => Assert.Equal(_testUserId, f.UserId));
        }

        [Fact]
        public async Task GetFormations_WithNonExistentUserId_ShouldReturnEmptyList()
        {
            // Arrange
            var nonExistentUserId = "non-existent-user";

            // Act
            var result = await _formationService.GetFormations(nonExistentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFormations_WithNullUserId_ShouldReturnEmptyList()
        {
            // Act
            var result = await _formationService.GetFormations(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFormations_WithEmptyUserId_ShouldReturnEmptyList()
        {
            // Act
            var result = await _formationService.GetFormations(string.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFormations_WithFormationsFromDifferentUsers_ShouldReturnOnlyUserFormations()
        {
            // Arrange
            var otherUserId = "other-user-id";
            var formations = new List<Formation>
            {
                new Formation
                {
                    Id = Guid.NewGuid(),
                    Title = "User Formation",
                    Company = "Company A",
                    StartAt = DateTimeOffset.UtcNow.AddDays(1),
                    EndAt = DateTimeOffset.UtcNow.AddDays(2),
                    UserId = _testUserId,
                    City = "Paris",
                    Country = "France"
                },
                new Formation
                {
                    Id = Guid.NewGuid(),
                    Title = "Other User Formation",
                    Company = "Company B",
                    StartAt = DateTimeOffset.UtcNow.AddDays(3),
                    EndAt = DateTimeOffset.UtcNow.AddDays(4),
                    UserId = otherUserId,
                    City = "Lyon",
                    Country = "France"
                }
            };

            _context.Formations.AddRange(formations);
            await _context.SaveChangesAsync();

            // Act
            var result = await _formationService.GetFormations(_testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(_testUserId, result[0].UserId);
            Assert.Equal("User Formation", result[0].Title);
        }

        #endregion

        #region AddFormation Tests

        [Fact]
        public async Task AddFormation_WithValidData_ShouldAddFormationAndReturnDto()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "New Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                City = "Test City",
                Country = "Test Country"
            };

            // Act
            var result = await _formationService.AddFormation(formationCreateDTO, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formationCreateDTO.Title, result.Title);
            Assert.Equal(formationCreateDTO.Company, result.Company);
            Assert.Equal(formationCreateDTO.StartAt, result.StartAt);
            Assert.Equal(formationCreateDTO.EndAt, result.EndAt);
            Assert.Equal(formationCreateDTO.City, result.City);
            Assert.Equal(formationCreateDTO.Country, result.Country);
            Assert.Equal(_testUserId, result.UserId);

            // Verify it was saved to database
            var savedFormation = await _context.Formations.FirstOrDefaultAsync(f => f.UserId == _testUserId);
            Assert.NotNull(savedFormation);
            Assert.Equal(formationCreateDTO.Title, savedFormation.Title);
        }

        [Fact]
        public async Task AddFormation_WithNullFormationCreateDTO_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _formationService.AddFormation(null, _testUserId));
        }

        [Fact]
        public async Task AddFormation_WithNullUserId_ShouldThrowException()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "New Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2)
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _formationService.AddFormation(formationCreateDTO, null));
        }

        [Fact]
        public async Task AddFormation_WithEmptyUserId_ShouldCreateFormationWithEmptyUserId()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "New Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2)
            };

            // Act
            var result = await _formationService.AddFormation(formationCreateDTO, string.Empty);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formationCreateDTO.Title, result.Title);
            Assert.Equal(string.Empty, result.UserId);
        }

        [Fact]
        public async Task AddFormation_WithOptionalFieldsNull_ShouldAddFormationSuccessfully()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "New Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                City = null,
                Country = null
            };

            // Act
            var result = await _formationService.AddFormation(formationCreateDTO, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formationCreateDTO.Title, result.Title);
            Assert.Equal(formationCreateDTO.Company, result.Company);
            Assert.Null(result.City);
            Assert.Null(result.Country);
        }

        [Fact]
        public async Task AddFormation_WithEndDateBeforeStartDate_ShouldStillAdd()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "Invalid Date Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(2),
                EndAt = DateTimeOffset.UtcNow.AddDays(1), // End before start
                City = "Test City",
                Country = "Test Country"
            };

            // Act
            var result = await _formationService.AddFormation(formationCreateDTO, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formationCreateDTO.StartAt, result.StartAt);
            Assert.Equal(formationCreateDTO.EndAt, result.EndAt);
        }

        #endregion

        #region UpdateFormation Tests

        [Fact]
        public async Task UpdateFormation_WithValidData_ShouldUpdateFormationAndReturnDto()
        {
            // Arrange
            var originalFormation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Original Title",
                Company = "Original Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = _testUserId,
                City = "Original City",
                Country = "Original Country"
            };

            _context.Formations.Add(originalFormation);
            await _context.SaveChangesAsync();

            var updateDTO = new FormationUpdateDTO
            {
                Id = originalFormation.Id.ToString(),
                Title = "Updated Title",
                Company = "Updated Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(3),
                EndAt = DateTimeOffset.UtcNow.AddDays(4),
                City = "Updated City",
                Country = "Updated Country"
            };

            // Act
            var result = await _formationService.UpdateFormation(updateDTO, originalFormation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDTO.Title, result.Title);
            // Note: Company is not updated by the ToFormation extension method
            Assert.Equal(originalFormation.Company, result.Company);
            Assert.Equal(updateDTO.StartAt, result.StartAt);
            Assert.Equal(updateDTO.EndAt, result.EndAt);
            Assert.Equal(updateDTO.City, result.City);
            Assert.Equal(updateDTO.Country, result.Country);

            // Verify it was updated in database
            var updatedFormation = await _context.Formations.FindAsync(originalFormation.Id);
            Assert.NotNull(updatedFormation);
            Assert.Equal(updateDTO.Title, updatedFormation.Title);
            // Note: Company is not updated by the ToFormation extension method
            Assert.Equal(originalFormation.Company, updatedFormation.Company);
        }

        [Fact]
        public async Task UpdateFormation_WithNullUpdateDTO_ShouldThrowException()
        {
            // Arrange
            var formation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Test Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = _testUserId
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _formationService.UpdateFormation(null, formation));
        }

        [Fact]
        public async Task UpdateFormation_WithNullFormation_ShouldThrowException()
        {
            // Arrange
            var updateDTO = new FormationUpdateDTO
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Updated Title",
                Company = "Updated Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2)
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _formationService.UpdateFormation(updateDTO, null));
        }

        [Fact]
        public async Task UpdateFormation_WithOptionalFieldsNull_ShouldUpdateSuccessfully()
        {
            // Arrange
            var originalFormation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Original Title",
                Company = "Original Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = _testUserId,
                City = "Original City",
                Country = "Original Country"
            };

            _context.Formations.Add(originalFormation);
            await _context.SaveChangesAsync();

            var updateDTO = new FormationUpdateDTO
            {
                Id = originalFormation.Id.ToString(),
                Title = "Updated Title",
                Company = "Updated Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(3),
                EndAt = DateTimeOffset.UtcNow.AddDays(4),
                City = null,
                Country = null
            };

            // Act
            var result = await _formationService.UpdateFormation(updateDTO, originalFormation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDTO.Title, result.Title);
            // Note: Company is not updated by the ToFormation extension method
            Assert.Equal(originalFormation.Company, result.Company);
            Assert.Null(result.City);
            Assert.Null(result.Country);
        }

        #endregion

        #region DeleteFormation Tests

        [Fact]
        public async Task DeleteFormation_WithValidUserIdAndFormationId_ShouldDeleteFormationAndReturnTrue()
        {
            // Arrange
            var formation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Formation to Delete",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = _testUserId,
                City = "Test City",
                Country = "Test Country"
            };

            _context.Formations.Add(formation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _formationService.DeleteFormation(_testUserId, formation.Id.ToString());

            // Assert
            Assert.True(result);

            // Verify it was deleted from database
            var deletedFormation = await _context.Formations.FindAsync(formation.Id);
            Assert.Null(deletedFormation);
        }

        [Fact]
        public async Task DeleteFormation_WithNonExistentFormationId_ShouldThrowException()
        {
            // Arrange
            var nonExistentFormationId = Guid.NewGuid().ToString();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(_testUserId, nonExistentFormationId));
            
            Assert.Equal("La formation n'existe pas", exception.Message);
        }

        [Fact]
        public async Task DeleteFormation_WithWrongUserId_ShouldThrowException()
        {
            // Arrange
            var formation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Formation to Delete",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = "different-user-id",
                City = "Test City",
                Country = "Test Country"
            };

            _context.Formations.Add(formation);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(_testUserId, formation.Id.ToString()));
            
            Assert.Equal("La formation n'existe pas", exception.Message);
        }

        [Fact]
        public async Task DeleteFormation_WithInvalidFormationId_ShouldThrowException()
        {
            // Arrange
            var invalidFormationId = "invalid-guid";

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(_testUserId, invalidFormationId));
        }

        [Fact]
        public async Task DeleteFormation_WithNullFormationId_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(_testUserId, null));
        }

        [Fact]
        public async Task DeleteFormation_WithEmptyFormationId_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(_testUserId, string.Empty));
        }

        [Fact]
        public async Task DeleteFormation_WithNullUserId_ShouldThrowException()
        {
            // Arrange
            var formation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Formation to Delete",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = _testUserId,
                City = "Test City",
                Country = "Test Country"
            };

            _context.Formations.Add(formation);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(null, formation.Id.ToString()));
            
            Assert.Equal("La formation n'existe pas", exception.Message);
        }

        [Fact]
        public async Task DeleteFormation_WithEmptyUserId_ShouldThrowException()
        {
            // Arrange
            var formation = new Formation
            {
                Id = Guid.NewGuid(),
                Title = "Formation to Delete",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                UserId = _testUserId,
                City = "Test City",
                Country = "Test Country"
            };

            _context.Formations.Add(formation);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => 
                _formationService.DeleteFormation(string.Empty, formation.Id.ToString()));
            
            Assert.Equal("La formation n'existe pas", exception.Message);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task FormationService_FullCrudOperations_ShouldWorkCorrectly()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "Integration Test Formation",
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                City = "Test City",
                Country = "Test Country"
            };

            // Act & Assert - Add
            var addedFormation = await _formationService.AddFormation(formationCreateDTO, _testUserId);
            Assert.NotNull(addedFormation);
            Assert.Equal(formationCreateDTO.Title, addedFormation.Title);

            // Act & Assert - Get
            var formations = await _formationService.GetFormations(_testUserId);
            Assert.Single(formations);
            Assert.Equal(addedFormation.Id, formations[0].Id);

            // Act & Assert - Update
            var formationEntity = await _context.Formations.FindAsync(addedFormation.Id);
            var updateDTO = new FormationUpdateDTO
            {
                Id = addedFormation.Id.ToString(),
                Title = "Updated Integration Test Formation",
                Company = "Updated Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(3),
                EndAt = DateTimeOffset.UtcNow.AddDays(4),
                City = "Updated Test City",
                Country = "Updated Test Country"
            };

            var updatedFormation = await _formationService.UpdateFormation(updateDTO, formationEntity);
            Assert.NotNull(updatedFormation);
            Assert.Equal(updateDTO.Title, updatedFormation.Title);

            // Act & Assert - Delete
            var deleteResult = await _formationService.DeleteFormation(_testUserId, addedFormation.Id.ToString());
            Assert.True(deleteResult);

            // Verify deletion
            var formationsAfterDelete = await _formationService.GetFormations(_testUserId);
            Assert.Empty(formationsAfterDelete);
        }

        [Fact]
        public async Task FormationService_ConcurrentOperations_ShouldHandleCorrectly()
        {
            // Arrange
            var tasks = new List<Task<FormationResponseDTO>>();
            
            for (int i = 0; i < 5; i++)
            {
                var formationCreateDTO = new FormationCreateDTO
                {
                    Title = $"Concurrent Formation {i}",
                    Company = $"Company {i}",
                    StartAt = DateTimeOffset.UtcNow.AddDays(i + 1),
                    EndAt = DateTimeOffset.UtcNow.AddDays(i + 2),
                    City = $"City {i}",
                    Country = $"Country {i}"
                };

                tasks.Add(_formationService.AddFormation(formationCreateDTO, _testUserId));
            }

            // Act
            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(5, results.Length);
            Assert.All(results, r => Assert.NotNull(r));
            
            var allFormations = await _formationService.GetFormations(_testUserId);
            Assert.Equal(5, allFormations.Count);
        }

        #endregion

        #region Edge Cases and Performance Tests

        [Fact]
        public async Task GetFormations_WithLargeDataSet_ShouldPerformWell()
        {
            // Arrange
            var formations = new List<Formation>();
            for (int i = 0; i < 1000; i++)
            {
                formations.Add(new Formation
                {
                    Id = Guid.NewGuid(),
                    Title = $"Formation {i}",
                    Company = $"Company {i}",
                    StartAt = DateTimeOffset.UtcNow.AddDays(i),
                    EndAt = DateTimeOffset.UtcNow.AddDays(i + 1),
                    UserId = _testUserId,
                    City = $"City {i}",
                    Country = $"Country {i}"
                });
            }

            _context.Formations.AddRange(formations);
            await _context.SaveChangesAsync();

            // Act
            var startTime = DateTime.UtcNow;
            var result = await _formationService.GetFormations(_testUserId);
            var endTime = DateTime.UtcNow;

            // Assert
            Assert.Equal(1000, result.Count);
            Assert.True((endTime - startTime).TotalSeconds < 5); // Should complete within 5 seconds
        }

        [Fact]
        public async Task AddFormation_WithSpecialCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = "Formation with Special Characters: àáâãäåæçèéêë",
                Company = "Company with émojis 🏢🎓",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                City = "Paris (75)",
                Country = "France & Co"
            };

            // Act
            var result = await _formationService.AddFormation(formationCreateDTO, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formationCreateDTO.Title, result.Title);
            Assert.Equal(formationCreateDTO.Company, result.Company);
            Assert.Equal(formationCreateDTO.City, result.City);
            Assert.Equal(formationCreateDTO.Country, result.Country);
        }

        [Fact]
        public async Task AddFormation_WithMaxLengthStrings_ShouldHandleCorrectly()
        {
            // Arrange
            var longTitle = new string('A', 255); // Max length based on MaxLength attribute
            var formationCreateDTO = new FormationCreateDTO
            {
                Title = longTitle,
                Company = "Test Company",
                StartAt = DateTimeOffset.UtcNow.AddDays(1),
                EndAt = DateTimeOffset.UtcNow.AddDays(2),
                City = new string('B', 255),
                Country = new string('C', 255)
            };

            // Act
            var result = await _formationService.AddFormation(formationCreateDTO, _testUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(longTitle, result.Title);
            Assert.Equal(255, result.Title.Length);
        }

        #endregion
    }
} 
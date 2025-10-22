using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalTest
{
    /// <summary>
    /// Tests unitaires pour le service AuthService.
    /// Se concentre sur les méthodes Register, Update et Login.
    /// </summary>
    public class AuthServiceUnitTests : IDisposable
    {
        private readonly Mock<UserManager<UserApp>> _mockUserManager;
        private readonly Mock<ISendMailService> _mockMailService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly ApiDefaultContext _context;
        private readonly AuthService _authService;

        public AuthServiceUnitTests()
        {
            // Configuration de la base de données en mémoire
            var options = new DbContextOptionsBuilder<ApiDefaultContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDefaultContext(options, true);

            // Configuration des mocks
            _mockUserManager = CreateMockUserManager();
            _mockMailService = new Mock<ISendMailService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();

            // Configuration de l'environnement web
            _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns("wwwroot");

            // Création du service
            _authService = new AuthService(
                _context,
                _mockUserManager.Object,
                _mockMailService.Object,
                _mockNotificationService.Object,
                _mockWebHostEnvironment.Object
            );
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        #region Tests Register
        /*
        [Fact]
        public async Task Register_ValidUser_ReturnsSuccessResponse()
        {
            // Arrange
            var userCreateDTO = new UserCreateDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe",
                Gender = EnumGender.Homme,
                PhoneNumber = "0600000000",
                DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Description = "Test user",
                privacyPolicyConsent = true,
                dataProcessingConsent = true
            };

            // Configuration des mocks
            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((UserApp?)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserApp>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<UserApp>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<UserApp>()))
                .ReturnsAsync("confirmation-token");

            _mockMailService.Setup(x => x.ScheduleSendConfirmationEmail(It.IsAny<Mail>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.Register(userCreateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.Status);
            Assert.Equal("Profil créé", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(userCreateDTO.Email, result.Data.Email);
            Assert.Equal(userCreateDTO.FirstName, result.Data.FirstName);
            Assert.Equal(userCreateDTO.LastName, result.Data.LastName);

            // Vérification des appels aux mocks
            _mockUserManager.Verify(x => x.FindByEmailAsync(userCreateDTO.Email), Times.Once);
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<UserApp>(), userCreateDTO.Password), Times.Once);
            _mockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<UserApp>(), It.Is<IEnumerable<string>>(roles => roles.Contains("Student"))), Times.Once);
            _mockMailService.Verify(x => x.ScheduleSendConfirmationEmail(It.IsAny<Mail>(), It.IsAny<string>()), Times.Once);
        }
        */
        /*
        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsErrorResponse()
        {
            // Arrange
            var userCreateDTO = new UserCreateDTO
            {
                Email = "existing@example.com",
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            var existingUser = new UserApp
            {
                Id = Guid.NewGuid().ToString(),
                Email = userCreateDTO.Email,
                UserName = userCreateDTO.Email
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(userCreateDTO.Email))
                .ReturnsAsync(existingUser);

            // Act
            var result = await _authService.Register(userCreateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40, result.Status);
            Assert.Equal("\"L'email est déjà utilisé\"", result.Message);
            Assert.Null(result.Data);

            // Vérification que la création n'a pas été tentée
            _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<UserApp>(), It.IsAny<string>()), Times.Never);
        }*/

        [Fact]
        public async Task Register_UserCreationFails_ReturnsErrorResponse()
        {
            // Arrange
            var userCreateDTO = new UserCreateDTO
            {
                Email = "test@example.com",
                Password = "weak",
                FirstName = "John",
                LastName = "Doe"
            };

            var identityError = new IdentityError
            {
                Code = "PasswordTooWeak",
                Description = "Le mot de passe est trop faible"
            };

            var failedResult = IdentityResult.Failed(identityError);

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((UserApp?)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserApp>(), It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            // Act
            var result = await _authService.Register(userCreateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.Status);
            Assert.Equal("Création échouée", result.Message);
            Assert.Null(result.Data);

            _mockMailService.Verify(x => x.ScheduleSendConfirmationEmail(It.IsAny<Mail>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Register_EmailSendingFails_ReturnsErrorResponse()
        {
            // Arrange
            var userCreateDTO = new UserCreateDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((UserApp?)null);

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserApp>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<UserApp>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<UserApp>()))
                .ReturnsAsync("confirmation-token");

            _mockMailService.Setup(x => x.ScheduleSendConfirmationEmail(It.IsAny<Mail>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Erreur d'envoi d'email"));

            // Act
            var result = await _authService.Register(userCreateDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40, result.Status);
            Assert.Equal("Le compte n'est pas créé!!!", result.Message);
        }

        #endregion

        #region Tests Update
        /*
        [Fact]
        public async Task Update_ValidUser_ReturnsSuccessResponse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new UserApp
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                UserName = "test@example.com"
            };

            var userUpdateDTO = new UserUpdateDTO
            {
                Id = userId,
                FirstName = "UpdatedJohn",
                LastName = "UpdatedDoe",
                Gender = EnumGender.Femme,
                DateOfBirth = new DateTimeOffset(1992, 5, 15, 0, 0, 0, TimeSpan.Zero),
                Description = "Updated description"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName!)
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Ajouter l'utilisateur au contexte
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<UserApp>()))
                .ReturnsAsync(new List<string> { "Student" });

            // Configuration du mock pour AddNotification avec paramètres explicites
            _mockNotificationService.Setup(x => x.AddNotification(It.IsAny<Notification>(), It.IsAny<string>()))
                .ReturnsAsync(new NotificationResponseDTO());

            // Act
            var result = await _authService.Update(userUpdateDTO, claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("Profil mis à jour", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal(userUpdateDTO.FirstName, result.Data.FirstName);
            Assert.Equal(userUpdateDTO.LastName, result.Data.LastName);

            // Vérification des appels aux mocks
            _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<UserApp>()), Times.Once);
            _mockNotificationService.Verify(x => x.AddNotification(It.IsAny<Notification>(), It.IsAny<string>()), Times.Once);
        }*/

        [Fact]
        public async Task Update_UserNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var userUpdateDTO = new UserUpdateDTO
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "UpdatedJohn",
                LastName = "UpdatedDoe"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "nonexistent@example.com"),
                new Claim(ClaimTypes.Name, "nonexistent@example.com")
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Act
            var result = await _authService.Update(userUpdateDTO, claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40, result.Status);
            Assert.Equal("Le compte n'existe pas ou ne correspond pas", result.Message);

            // Vérification qu'aucune notification n'a été envoyée
            _mockNotificationService.Verify(x => x.AddNotification(It.IsAny<Notification>(), It.IsAny<string>()), Times.Never);
        }
        /*
        [Fact]
        public async Task Update_DatabaseUpdateFails_ReturnsErrorResponse()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var user = new UserApp
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                UserName = "test@example.com"
            };

            var userUpdateDTO = new UserUpdateDTO
            {
                Id = userId,
                FirstName = "UpdatedJohn",
                LastName = "UpdatedDoe"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName!)
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            // Ajouter l'utilisateur au contexte
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Simuler une erreur lors de la notification
            _mockNotificationService.Setup(x => x.AddNotification(It.IsAny<Notification>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Erreur de base de données"));

            // Act
            var result = await _authService.Update(userUpdateDTO, claimsPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(40, result.Status);
            Assert.Equal("Erreur de base de données", result.Message);
        }
        */
        #endregion

        #region Tests Login
        /*
        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccessResponse()
        {
            Environment.SetEnvironmentVariable("JWT_KEY", "verylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkey");
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new UserApp
            {
                Id = Guid.NewGuid().ToString(),
                Email = userLoginDTO.Email,
                UserName = userLoginDTO.Email,
                FirstName = "John",
                LastName = "Doe"
            };

            var mockResponse = new Mock<HttpResponse>();
            var mockHeaders = new Mock<IHeaderDictionary>();
            var mockCookies = new Mock<IResponseCookies>();

            mockResponse.Setup(r => r.Headers).Returns(mockHeaders.Object);
            mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

            _mockUserManager.Setup(x => x.FindByEmailAsync(userLoginDTO.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, userLoginDTO.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Student" });

            // Act
            var result = await _authService.Login(userLoginDTO, mockResponse.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.Equal("Connexion réussite", result.Message);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.Token);
            Assert.NotNull(result.Data.User);
            Assert.Equal(userLoginDTO.Email, result.Data.User.Email);

            // Vérification des appels aux mocks
            _mockUserManager.Verify(x => x.FindByEmailAsync(userLoginDTO.Email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, userLoginDTO.Password), Times.AtLeastOnce);
            _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
            mockHeaders.Verify(h => h.Append("Access-Control-Allow-Credentials", "true"), Times.Once);
            mockCookies.Verify(c => c.Append("refreshToken", It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Once);
        }
        */
        [Fact]
        public async Task Login_UserNotFound_ReturnsErrorResponse()
        {
            Environment.SetEnvironmentVariable("JWT_KEY", "verylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkey");
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "nonexistent@example.com",
                Password = "TestPassword123!"
            };

            var mockResponse = new Mock<HttpResponse>();

            _mockUserManager.Setup(x => x.FindByEmailAsync(userLoginDTO.Email))
                .ReturnsAsync((UserApp?)null);

            // Act
            var result = await _authService.Login(userLoginDTO, mockResponse.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.Status);
            Assert.Equal("L'utilisateur n'existe pas ", result.Message);
            Assert.Null(result.Data);

            // Vérification que la vérification du mot de passe n'a pas été tentée
            _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<UserApp>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsErrorResponse()
        {
            Environment.SetEnvironmentVariable("JWT_KEY", "verylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkey");

            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            var user = new UserApp
            {
                Id = Guid.NewGuid().ToString(),
                Email = userLoginDTO.Email,
                UserName = userLoginDTO.Email
            };

            var mockResponse = new Mock<HttpResponse>();

            _mockUserManager.Setup(x => x.FindByEmailAsync(userLoginDTO.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, userLoginDTO.Password))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.Login(userLoginDTO, mockResponse.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.Status);
            Assert.Equal("Connexion échouée", result.Message);
            Assert.Null(result.Data);

            // Vérification des appels aux mocks
            _mockUserManager.Verify(x => x.FindByEmailAsync(userLoginDTO.Email), Times.Once);
            _mockUserManager.Verify(x => x.CheckPasswordAsync(user, userLoginDTO.Password), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Login_ValidCredentials_UpdatesLastLoginTime()
        {
            Environment.SetEnvironmentVariable("JWT_KEY", "verylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkeyverylongjwtkey");
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var user = new UserApp
            {
                Id = Guid.NewGuid().ToString(),
                Email = userLoginDTO.Email,
                UserName = userLoginDTO.Email,
                FirstName = "John",
                LastName = "Doe",
                LastLogginAt = DateTime.Now.AddDays(-1) // Date ancienne
            };

            var mockResponse = new Mock<HttpResponse>();
            var mockHeaders = new Mock<IHeaderDictionary>();
            var mockCookies = new Mock<IResponseCookies>();

            mockResponse.Setup(r => r.Headers).Returns(mockHeaders.Object);
            mockResponse.Setup(r => r.Cookies).Returns(mockCookies.Object);

            // Ajouter l'utilisateur au contexte pour pouvoir suivre les changements
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var originalLastLogin = user.LastLogginAt;

            _mockUserManager.Setup(x => x.FindByEmailAsync(userLoginDTO.Email))
                .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.CheckPasswordAsync(user, userLoginDTO.Password))
                .ReturnsAsync(true);

            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Student" });

            // Act
            var result = await _authService.Login(userLoginDTO, mockResponse.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Status);
            Assert.True(user.LastLogginAt > originalLastLogin);
        }

        #endregion

        #region Méthodes utilitaires

        /// <summary>
        /// Crée un mock du UserManager pour les tests.
        /// </summary>
        /// <returns>Mock du UserManager configuré</returns>
        private static Mock<UserManager<UserApp>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<UserApp>>();
            var mockUserManager = new Mock<UserManager<UserApp>>(
                store.Object,
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<UserApp>>(),
                new IUserValidator<UserApp>[0],
                new IPasswordValidator<UserApp>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<UserApp>>>()
            );

            return mockUserManager;
        }

        #endregion
    }
}
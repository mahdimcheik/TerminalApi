using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TerminalApi.Models;
using TerminalApi.Utilities;

namespace TerminalTestIntegration
{
    public class AuthTestIntegration : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerOptions jsonOptions;

        public AuthTestIntegration(CustomWebApplicationFactory factory)
        {
            this.httpClient = factory.CreateClient();
            this.jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        #region Registration Tests

        [Fact]
        public async Task Register_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var uniqueEmail = $"test.{Guid.NewGuid().ToString("N")[..8]}@skillhive.fr";
            var userCreateDTO = new UserCreateDTO
            {
                Email = uniqueEmail,
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe",
                Gender = EnumGender.Homme,
                PhoneNumber = "0600000000",
                DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Description = "Test user for registration",
                privacyPolicyConsent = true,
                dataProcessingConsent = true
            };

            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response was not successful. Status: {response.StatusCode}, Content: {responseContent}");
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<UserResponseDTO>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Data);
            Assert.Equal(userCreateDTO.Email, deserializedResponse.Data.Email);
        }

        [Fact]
        public async Task Register_MissingRequiredFields_ReturnsBadRequest()
        {
            // Arrange
            var userCreateDTO = new UserCreateDTO
            {
                Email = $"invalid.{Guid.NewGuid().ToString("N")[..8]}@skillhive.fr",
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe"
                // Missing required fields: Gender, DateOfBirth, Consent fields
            };

            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/register", content);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            // Arrange - Using existing seeded user
            var userCreateDTO = new UserCreateDTO
            {
                Email = "admin@skillhive.fr", // This email already exists in seeded data
                Password = "TestPassword123!",
                FirstName = "John",
                LastName = "Doe",
                Gender = EnumGender.Homme,
                PhoneNumber = "0600000000",
                DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Description = "Test user for registration",
                privacyPolicyConsent = true,
                dataProcessingConsent = true
            };

            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/register", content);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Login Tests

        [Fact]
        public async Task Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "admin@skillhive.fr",
                Password = "Admin123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/login", content);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDTO<LoginOutputDTO>>();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(responseContent);
            Assert.NotNull(responseContent.Data);
            Assert.NotNull(responseContent.Data.Token);
            Assert.NotNull(responseContent.Data.User);
            Assert.Equal("admin@skillhive.fr", responseContent.Data.User.Email);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "admin@skillhive.fr",
                Password = "WrongPassword123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/login", content);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_NonExistentUser_ReturnsBadRequest()
        {
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "nonexistent@skillhive.fr",
                Password = "Password123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/login", content);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_MissingFields_ReturnsBadRequest()
        {
            // Arrange
            var userLoginDTO = new UserLoginDTO
            {
                Email = "admin@skillhive.fr"
                // Missing Password
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/login", content);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Password Reset Tests

        [Fact]
        public async Task ForgotPassword_ValidEmail_ReturnsSuccess()
        {
            // Arrange
            var forgotPasswordInput = new ForgotPasswordInput
            {
                Email = "admin@skillhive.fr"
            };

            var content = new StringContent(JsonSerializer.Serialize(forgotPasswordInput, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/forgot-password", content);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDTO<PasswordResetResponseDTO>>();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(responseContent);
            Assert.NotNull(responseContent.Data);
            Assert.Equal("admin@skillhive.fr", responseContent.Data.Email);
        }

        [Fact]
        public async Task ForgotPassword_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var forgotPasswordInput = new ForgotPasswordInput
            {
                Email = "nonexistent@skillhive.fr"
            };

            var content = new StringContent(JsonSerializer.Serialize(forgotPasswordInput, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/forgot-password", content);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region User Information Tests

        [Fact]
        public async Task GetMyInformations_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await httpClient.GetAsync("/users/my-informations");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMyInformations_WithAuth_ReturnsSuccess()
        {
            // Arrange - First login to get token
            var loginResponse = await LoginAndGetToken("admin@skillhive.fr", "Admin123!");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);

            // Act
            var response = await httpClient.GetAsync("/users/my-informations");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response failed: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);
            
            // Only try to parse JSON if we have content
            if (!string.IsNullOrEmpty(responseContent))
            {
                var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<object>>(responseContent, jsonOptions);
                Assert.NotNull(deserializedResponse);
                Assert.NotNull(deserializedResponse.Data);
            }
        }

        [Fact]
        public async Task GetPublicInformations_ValidUserId_ReturnsSuccess()
        {
            // Arrange - Get admin user ID first
            var loginResponse = await LoginAndGetToken("admin@skillhive.fr", "Admin123!");
            var userId = loginResponse.Data.User.Id;

            // Act
            var response = await httpClient.GetAsync($"/users/public-informations?userId={userId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response failed: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);
            
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<UserResponseDTO>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Data);
            Assert.Equal("admin@skillhive.fr", deserializedResponse.Data.Email);
        }

        [Fact]
        public async Task GetPublicInformations_Teacher_ReturnsSuccess()
        {
            // Act
            var response = await httpClient.GetAsync("/users/public-informations?userId=teacher");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response failed: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);
            
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<UserResponseDTO>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Data);
        }

        [Fact]
        public async Task Update_WithAuth_ReturnsSuccess()
        {
            // Arrange - Login first
            var loginResponse = await LoginAndGetToken("update@skillhive.fr", "Update123!");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);

            var updateDTO = new UserUpdateDTO
            {
                Id = loginResponse.Data.User.Id,
                FirstName = "Updated",
                LastName = "Name",
                Gender = EnumGender.Femme,
                DateOfBirth = new DateTimeOffset(1993, 4, 5, 0, 0, 0, TimeSpan.Zero),
                Description = "Updated description"
            };

            var content = new StringContent(JsonSerializer.Serialize(updateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PatchAsync("/users/update", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response failed: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);
            
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<UserResponseDTO>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Data);
            Assert.Equal("Updated", deserializedResponse.Data.FirstName);
        }

        [Fact]
        public async Task Update_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var updateDTO = new UserUpdateDTO
            {
                Id = "some-id",
                FirstName = "Updated",
                LastName = "Name",
                Gender = EnumGender.Femme,
                DateOfBirth = new DateTimeOffset(1993, 4, 5, 0, 0, 0, TimeSpan.Zero)
            };

            var content = new StringContent(JsonSerializer.Serialize(updateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PatchAsync("/users/update", content);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

        #region Token Tests

        [Fact]
        public async Task RefreshToken_WithoutCookie_ReturnsUnauthorized()
        {
            // Act
            var response = await httpClient.GetAsync("/users/refresh-token");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Logout_ReturnsSuccess()
        {
            // Act
            var response = await httpClient.GetAsync("/users/logout");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Console.WriteLine(responseContent);
            Assert.Contains("déconnecté", responseContent);
        }

        #endregion

        #region Admin Tests

        [Fact]
        public async Task GetAllUsers_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await httpClient.GetAsync("/users/all?first=0&rows=10");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllUsers_WithAdminAuth_ReturnsSuccess()
        {
            // Arrange - Login as admin
            var loginResponse = await LoginAndGetToken("admin@skillhive.fr", "Admin123!");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);

            // Act
            var response = await httpClient.GetAsync("/users/all?first=0&rows=10");
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response failed: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);
            
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<ResponsePagination<UserResponseDTO>>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Data);
            Assert.NotNull(deserializedResponse.Data.DataList);
            Assert.True(deserializedResponse.Data.DataList.Count > 0);
        }

        [Fact]
        public async Task GetAllUsers_WithStudentAuth_ReturnsForbidden()
        {
            // Arrange - Login as student
            var loginResponse = await LoginAndGetToken("student@skillhive.fr", "Student123!");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);

            // Act
            var response = await httpClient.GetAsync("/users/all?first=0&rows=10");

            // Assert - Should be Forbidden because student doesn't have admin role
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SeedUsers_WithoutAuth_ReturnsUnauthorized()
        {
            // Act
            var response = await httpClient.GetAsync("/users/seed");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        //[Fact]
        //public async Task SeedUsers_WithAdminAuth_ReturnsSuccess()
        //{
        //    // Arrange - Login as admin
        //    var loginResponse = await LoginAndGetToken("admin@skillhive.fr", "Admin123!");
        //    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);

        //    // Act
        //    var response = await httpClient.GetAsync("/users/seed");
        //    var responseContent = await response.Content.ReadAsStringAsync();

        //    // Assert - Accept both success and server error since seed might fail due to constraints
        //    Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.InternalServerError, 
        //               $"Response failed: {response.StatusCode}, Content: {responseContent}");
        //}

        #endregion

        #region Email Confirmation Tests

        [Fact]
        public async Task EmailConfirmation_InvalidToken_ReturnsBadRequest()
        {
            // Act
            var response = await httpClient.GetAsync("/users/email-confirmation?userId=invalid&confirmationToken=invalid");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task ResendConfirmationLink_WithoutAuth_ReturnsBadRequest()
        {
            // Act
            var response = await httpClient.GetAsync("/users/resend-confirmation-link");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Helper Methods

        private async Task<ResponseDTO<LoginOutputDTO>> LoginAndGetToken(string email, string password)
        {
            var userLoginDTO = new UserLoginDTO
            {
                Email = email,
                Password = password
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("/users/login", content);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDTO<LoginOutputDTO>>();

            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(responseContent);
            return responseContent;
        }

        #endregion
    }
}

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

        [Fact]
        public async Task RegisterUserAsync()
        {
            // Arrange - Use unique email to avoid conflicts
            var uniqueEmail = $"mahdi.test.{Guid.NewGuid().ToString("N")[..8]}@hotmail.fr";

            UserCreateDTO userCreateDTO = new UserCreateDTO
            {
                Email = uniqueEmail,
                Password = "Olitec1>",
                FirstName = "mahdi",
                LastName = "mcheik",
                Gender = EnumGender.Homme, // Use the correct enum value
                PhoneNumber = "123456789", // Use correct property name
                DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), // Use DateTimeOffset
                Description = "Test user for integration test",
                privacyPolicyConsent = true, // Use correct property name
                dataProcessingConsent = true, // Use correct property name
            };

            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response was not successful. Status: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);

            // Deserialize and verify the response structure
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<UserResponseDTO>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Data);
            Assert.Equal(userCreateDTO.Email, deserializedResponse.Data.Email);
            Assert.Equal(userCreateDTO.FirstName, deserializedResponse.Data.FirstName);
            Assert.Equal(userCreateDTO.LastName, deserializedResponse.Data.LastName);
            Assert.Equal(userCreateDTO.Gender, deserializedResponse.Data.Gender);
        }

        [Fact]
        public async Task Register_returnBadRequest_Validation()
        {
            // Arrange - Use unique email to avoid conflicts
            var uniqueEmail = $"mahdi.test.{Guid.NewGuid().ToString("N")[..8]}@hotmail.fr";

            UserCreateDTO userCreateDTO = new UserCreateDTO
            {
                Email = uniqueEmail,
                Password = "Olitec1>",
                FirstName = "mahdi",
                LastName = "mcheik",
            };

            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode, $"Response was successful. Status: {response.StatusCode}, Content: {responseContent}");
        }

        [Fact]
        public async Task Register_returnBadRequest_UserExists()
        {
            // Arrange - Use unique email to avoid conflicts
            var uniqueEmail = "teacher@skillhive.fr";

            UserCreateDTO userCreateDTO = new UserCreateDTO
            {
                Email = uniqueEmail,
                Password = "Olitec1>",
                FirstName = "mahdi",
                LastName = "mcheik",
                Gender = EnumGender.Homme, // Use the correct enum value
                PhoneNumber = "123456789", // Use correct property name
                DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), // Use DateTimeOffset
                Description = "Test user for integration test",
                privacyPolicyConsent = true, // Use correct property name
                dataProcessingConsent = true, // Use correct property name
            };

            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.False(response.IsSuccessStatusCode, $"Response was successful. Status: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.NotEmpty(responseContent);

            // Deserialize and verify the response structure
            var deserializedResponse = JsonSerializer.Deserialize<ResponseDTO<UserResponseDTO>>(responseContent, jsonOptions);
            Assert.NotNull(deserializedResponse);
            Assert.NotNull(deserializedResponse.Message);
        }

        [Fact]
        public async Task LoginUserAsync()
        {

            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = "teacher@skillhive.fr",
                Password = "Admin123!",
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/login", content);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDTO<LoginOutputDTO>>();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Response was not successful. Status: {response.StatusCode}, Content: {responseContent}");
            Assert.NotNull(responseContent);
            Assert.IsType<ResponseDTO<LoginOutputDTO>>(responseContent);
            Assert.NotNull(responseContent?.Data?.Token);
            Assert.NotNull(responseContent?.Data?.User);
        }

        [Fact]
        public async Task LoginUserAsync_returnBadCredentials()
        {

            UserLoginDTO userLoginDTO = new UserLoginDTO
            {
                Email = "teache@skillhive.fr",
                Password = "Admin123",
            };

            var content = new StringContent(JsonSerializer.Serialize(userLoginDTO, jsonOptions), Encoding.UTF8, "application/json");

            // Act
            var response = await httpClient.PostAsync("/users/login", content);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDTO<LoginOutputDTO?>?>();

            // Assert
            Assert.False(response.IsSuccessStatusCode, $"Response was not successful. Status: {response.StatusCode}, Content: {responseContent}");

        }
    }
}

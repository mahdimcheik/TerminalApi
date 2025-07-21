using System.Net;
using TerminalTestIntegration;

namespace TerminalTestIntegration.Tests
{
    public class BasicIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _httpClient;

        public BasicIntegrationTest(CustomWebApplicationFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task Swagger_ReturnsOk()
        {
            // Act
            var response = await _httpClient.GetAsync("/swagger");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SwaggerJson_ReturnsOk()
        {
            // Act
            var response = await _httpClient.GetAsync("/swagger/v1/swagger.json");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Application_StartsSuccessfully()
        {
            // Act - Try to access any endpoint
            var response = await _httpClient.GetAsync("/swagger");

            // Assert - If we get any response, the application started successfully
            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.NotFound || 
                       response.StatusCode == HttpStatusCode.MethodNotAllowed,
                       $"Application failed to start. Status: {response.StatusCode}");
        }
    }
} 
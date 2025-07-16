using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TerminalApi.Models;
using TerminalApi.Utilities;

namespace TerminalTestIntegration
{
    public class AuthTestIntegration
    {
        private readonly HttpClient httpClient;

        public AuthTestIntegration(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        [Fact]
        public async Task RegisterUserAsync()
        {
            UserCreateDTO userCreateDTO = new UserCreateDTO
            {
                Email = "mahdi.mcheik@hotmail.fr",
                Password = "Olitec1>",
                FirstName = "mahdi",
                LastName = "bensalem",
                DateOfBirth = DateTimeOffset.Now.AddYears(-39),
                Gender = EnumGender.Homme,
                Description = "test user",
                privacyPolicyConsent = true,
                dataProcessingConsent = true,   
            };
            var content = new StringContent(JsonSerializer.Serialize(userCreateDTO), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("/users/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Assert.NotNull(responseContent);
            Assert.IsType<ResponseDTO<UserResponseDTO>>(responseContent);            
        }
    }
}

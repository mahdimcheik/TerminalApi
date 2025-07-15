using Moq;
using TerminalApi.Models;
using TerminalApi.Services;

namespace TerminalTest
{
    public class UnitTestFormation
    {
        // This is a test class for testing purposes.
        // It is empty and does not contain any tests.
        // You can add your test methods here.    
    
        [Fact]
        public async Task GetFormationById()
        {
            var mockFormation = new Mock<FormationService>();
            mockFormation.Setup(service => service.GetFormations(It.IsAny<string>()))
                .ReturnsAsync(new List<FormationResponseDTO>());

            var result = await mockFormation.Object.GetFormations((Guid.NewGuid().ToString()));
            Assert.NotNull(result);
            Assert.IsType<List<FormationResponseDTO>>(result);
        }
    }
}
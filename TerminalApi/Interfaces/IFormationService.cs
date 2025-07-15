using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IFormationService
    {
        Task<List<FormationResponseDTO>?> GetFormations(string userId);
        Task<FormationResponseDTO?> AddFormation(FormationCreateDTO formationCreate, string userId);
        Task<FormationResponseDTO> UpdateFormation(FormationUpdateDTO updatedFormation, Formation formation);
        Task<bool> DeleteFormation(string userId, string formationId);
    }
} 
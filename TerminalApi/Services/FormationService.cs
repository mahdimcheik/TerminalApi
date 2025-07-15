using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Interfaces;

namespace TerminalApi.Services
{
    public class FormationService : IFormationService
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;

        public FormationService(UserManager<UserApp> userManager, ApiDefaultContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        public async Task<List<FormationResponseDTO>?> GetFormations(string userId)
        {
            return await context
                .Formations.Where(ad => ad.UserId == userId)
                .Select(ad => ad.ToFormationDTO())
                .ToListAsync();
        }


        public async Task<FormationResponseDTO?> AddFormation(FormationCreateDTO formationCreate, string userId)
        {
            try
            {
                var formation = formationCreate.ToFormation(userId);
                context.Formations.Add(formation);
                await context.SaveChangesAsync();
                return formation.ToFormationDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<FormationResponseDTO> UpdateFormation(FormationUpdateDTO updatedFormation, Formation formation)
        {
            try
            {
                updatedFormation.ToFormation(formation);
                await context.SaveChangesAsync();
                return formation.ToFormationDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> DeleteFormation(string userId, string formationId)
        {
            try
            {
                var formation = await context.Formations.FirstOrDefaultAsync(x => x.Id == Guid.Parse(formationId) && x.UserId == userId);
                if (formation is null) throw new Exception("La formation n'existe pas");
                context.Formations.Remove(formation);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

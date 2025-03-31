using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Formations;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FixturesController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly FakerService fakerService;
        private readonly FormationService formationService;

        public FixturesController(ApiDefaultContext context,FakerService fakerService ,FormationService formationService)
        {
            this.context = context;
            this.fakerService = fakerService;
            this.formationService = formationService;
        }

        [HttpGet("add-formations")]
        public async Task<IActionResult> AddFormations()
        {
            var users =  context.Users.ToList();

            foreach(var user in users)
            {
                int randomInt = new Random().Next(1, 5);
                var formations = fakerService.GenerateFormationsCreateDTO().Generate(randomInt);
                foreach (var formation in formations)
                {
                    context.Formations.Add(formation.ToFormation(user.Id));
                }
            }

            await context.SaveChangesAsync();
            return Ok("Formations ajoutées");
        }

        [HttpGet("add-addresses")]
        public async Task<IActionResult> AddAddresses()
        {
            var users = context.Users.ToList();

            foreach (var user in users)
            {
                int randomInt = new Random().Next(1, 3);
                List<AddressCreateDTO>? addresses = fakerService.GenerateAddresseCreateDTO().Generate(randomInt);
                foreach (var addresse in addresses)
                {
                    context.Addresses.Add(addresse.ToAddress(user.Id));
                }
            }

            await context.SaveChangesAsync();
            return Ok("Addresses ajoutées");
        }
    }
}

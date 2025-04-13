using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Formations;
using TerminalApi.Models.Slots;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles="Admin")]
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

        [HttpGet("add-slots")]
        public async Task<IActionResult> AddSlots()
        {
            var slotsDTO = fakerService.GenerateSlotCreateDTO().Generate(3000);

            var slots = slotsDTO.Select(x =>
            {
                return x.ToSlot("1577fcf3-35a3-42fb-add1-daffcc56f640");
            }).ToList();

            context.Slots.AddRange(slots);

            await context.SaveChangesAsync();
            return Ok("Addresses ajoutées");
        }

        [HttpGet("add-orders")]
        public async Task<IActionResult> AddOrders()
        {
            await fakerService.CreateOrdersFixtureAsync();
            return Ok("orders ajoutées");
        }

        [HttpGet("add-bookings")]
        public async Task<IActionResult> AddBookings()
        {
            await fakerService.GenerateBookingCreateDTO();
            return Ok("bookingsajoutées");
        }
    }
}

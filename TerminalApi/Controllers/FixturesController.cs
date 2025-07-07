using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Slots;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des données de test (fixtures) pour l'application.
    /// Permet de générer des données fictives pour les formations, adresses, créneaux, commandes et réservations.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FixturesController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly FakerService fakerService;
        private readonly FormationService formationService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur FixturesController.
        /// </summary>
        /// <param name="context">Contexte de base de données utilisé pour accéder et manipuler les entités.</param>
        /// <param name="fakerService">Service utilisé pour générer des données fictives.</param>
        /// <param name="formationService">Service utilisé pour gérer les formations.</param>
        public FixturesController(ApiDefaultContext context, FakerService fakerService, FormationService formationService)
        {
            this.context = context;
            this.fakerService = fakerService;
            this.formationService = formationService;
        }

        /// <summary>
        /// Génère et ajoute des formations fictives pour chaque utilisateur existant.
        /// </summary>
        /// <returns>Un message confirmant l'ajout des formations.</returns>
        /// <response code="200">Formations ajoutées avec succès.</response>
        /// <response code="500">Erreur lors de l'ajout des formations.</response>
        [HttpGet("add-formations")]
        public async Task<IActionResult> AddFormations()
        {
            var users = context.Users.ToList();

            foreach (var user in users)
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

        /// <summary>
        /// Génère et ajoute des adresses fictives pour chaque utilisateur existant.
        /// </summary>
        /// <returns>Un message confirmant l'ajout des adresses.</returns>
        /// <response code="200">Adresses ajoutées avec succès.</response>
        /// <response code="500">Erreur lors de l'ajout des adresses.</response>
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

        /// <summary>
        /// Génère et ajoute un grand nombre de créneaux fictifs.
        /// </summary>
        /// <returns>Un message confirmant l'ajout des créneaux.</returns>
        /// <response code="200">Créneaux ajoutés avec succès.</response>
        /// <response code="500">Erreur lors de l'ajout des créneaux.</response>
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
            return Ok("Créneaux ajoutés");
        }

        /// <summary>
        /// Génère et ajoute des commandes fictives.
        /// </summary>
        /// <returns>Un message confirmant l'ajout des commandes.</returns>
        /// <response code="200">Commandes ajoutées avec succès.</response>
        /// <response code="500">Erreur lors de l'ajout des commandes.</response>
        [HttpGet("add-orders")]
        public async Task<IActionResult> AddOrders()
        {
            await fakerService.CreateOrdersFixtureAsync();
            return Ok("Commandes ajoutées");
        }

        /// <summary>
        /// Génère et ajoute des réservations fictives.
        /// </summary>
        /// <returns>Un message confirmant l'ajout des réservations.</returns>
        /// <response code="200">Réservations ajoutées avec succès.</response>
        /// <response code="500">Erreur lors de l'ajout des réservations.</response>
        [HttpGet("add-bookings")]
        public async Task<IActionResult> AddBookings()
        {
            await fakerService.GenerateBookingCreateDTO();
            return Ok("Réservations ajoutées");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des activités pour les enseignants et les étudiants.
    /// Fournit des données statistiques sur les réservations, commandes et nouveaux étudiants.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur ActivitiesController.
        /// </summary>
        /// <param name="context">Contexte de base de données injecté.</param>
        public ActivitiesController(ApiDefaultContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Récupère les données d'activité pour un enseignant (réservations de la semaine, commandes et nouveaux étudiants).
        /// </summary>
        /// <returns>Un objet ResponseDTO contenant les données d'activité de l'enseignant.</returns>
        /// <response code="200">Données d'activité récupérées avec succès.</response>
        /// <response code="400">Erreur lors de la récupération des données.</response>
        [HttpGet("teacher")]
        public async Task<ActionResult<ResponseDTO<ActivitiesTeacher>>> GetTeacherTraffic()
        {
            // Calcul du premier jour de la semaine actuelle
            var firstDayOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);

            // Récupération des réservations de la semaine
            var bookingsOfTheWeek = await context.Bookings
                .Include(b => b.Slot)
                .Include(b => b.Booker)
                .Where(b => b.Slot.StartAt >= firstDayOfWeek && b.Slot.EndAt < firstDayOfWeek.AddDays(7))
                .OrderBy(b => b.Slot.StartAt)
                .Select(b => b.ToBookingResponseDTO())
                .ToListAsync();

            // Récupération des commandes de la semaine
            var ordersOfTheWeek = await context.Orders
                .Where(o => o.UpdatedAt >= firstDayOfWeek && o.PaymentIntent != null)
                .OrderBy(o => o.UpdatedAt)
                .Select(o => o.ToOrderResponseForTeacherDTO())
                .ToListAsync();

            // Récupération des nouveaux étudiants de la semaine
            var newStudents = await context.Users
                .Where(u => u.CreatedAt >= firstDayOfWeek)
                .Select(u => u.ToUserResponseDTO(null))
                .ToListAsync();

            var trafficData = new ActivitiesTeacher
            {
                NewStudents = newStudents,
                BookingsOftheWeek = bookingsOfTheWeek,
                OrdersOfTheWeek = ordersOfTheWeek
            };

            return Ok(new ResponseDTO<ActivitiesTeacher>
            {
                Message = "Activités envoyées avec succès",
                Data = trafficData,
                Status = 200
            });
        }

        /// <summary>
        /// Récupère les données d'activité pour un étudiant spécifique (réservations et historique des commandes).
        /// </summary>
        /// <param name="id">Identifiant de l'étudiant.</param>
        /// <returns>Un objet ResponseDTO contenant les données d'activité de l'étudiant.</returns>
        /// <response code="200">Données d'activité récupérées avec succès.</response>
        /// <response code="400">Erreur lors de la récupération des données.</response>
        [HttpGet("student/{id}")]
        public async Task<ActionResult<ResponseDTO<ActivitiesStudent>>> GetStudentTraffic([FromRoute] string id)
        {
            // Calcul du premier jour de la semaine actuelle
            var firstDayOfWeek = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);

            // Récupération des réservations de la semaine pour l'étudiant
            var bookingsOfTheWeek = await context.Bookings
                .Include(b => b.Slot)
                .Include(b => b.Booker)
                .Where(b => b.Slot.StartAt >= firstDayOfWeek && b.Slot.EndAt < firstDayOfWeek.AddDays(7) && b.BookedById == id)
                .OrderBy(b => b.Slot.StartAt)
                .Select(b => b.ToBookingResponseDTO())
                .ToListAsync();

            // Récupération de l'historique des commandes de l'étudiant
            var ordersHistory = await context.Orders
                .Where(o => o.UpdatedAt >= firstDayOfWeek && o.PaymentIntent != null && o.BookerId == id)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(o => o.ToOrderResponseForTeacherDTO())
                .ToListAsync();

            var trafficData = new ActivitiesStudent
            {
                BookingsOftheWeek = bookingsOfTheWeek,
                OrdersHistory = ordersHistory
            };

            return Ok(new ResponseDTO<ActivitiesStudent>
            {
                Message = "Activités envoyées avec succès",
                Data = trafficData,
                Status = 200
            });
        }
    }
}

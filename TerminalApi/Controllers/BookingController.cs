using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des réservations.
    /// Permet aux utilisateurs de réserver, annuler et consulter leurs réservations.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [Authorize(Policy = "NotBanned")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService bookingService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur BookingController.
        /// </summary>
        /// <param name="bookingService">Service de gestion des réservations injecté.</param>
        /// <param name="context">Contexte de base de données injecté.</param>
        public BookingController(BookingService bookingService, ApiDefaultContext context)
        {
            this.bookingService = bookingService;
            this.context = context;
        }

        /// <summary>
        /// Permet à un utilisateur de réserver un créneau horaire.
        /// </summary>
        /// <param name="bookingCreateDTO">Données de la réservation à créer.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succès ou l'échec de l'opération.</returns>
        /// <response code="204">Réservation enregistrée avec succès.</response>
        /// <response code="400">Demande invalide ou créneau déjà réservé.</response>
        [HttpPost("book")]
        public async Task<ActionResult<ResponseDTO>> BookSlot([FromBody] BookingCreateDTO bookingCreateDTO)
        {
            try
            {
                if (bookingCreateDTO.SlotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var resultBooking = await bookingService.BookSlot(bookingCreateDTO, user);
                if (resultBooking)
                {
                    return Ok(new ResponseDTO { Message = "La résérvation est enregistrée", Status = 204 });
                }
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée, créneau déjà résérvé ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }

        /// <summary>
        /// Permet à un administrateur d'annuler une réservation pour un créneau spécifique.
        /// </summary>
        /// <param name="slotId">Identifiant du créneau à annuler.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succès ou l'échec de l'opération.</returns>
        /// <response code="204">Réservation annulée avec succès.</response>
        /// <response code="400">Demande invalide ou réservation inexistante.</response>
        [HttpDelete("unbook")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> RemoveReservationByTeacher([FromQuery] string slotId)
        {
            try
            {
                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée, la réservation n'existe pas!!!" });
                }
                var resultBooking = await bookingService.RemoveReservationByTeacher(slotId);
                if (resultBooking)
                {
                    return Ok(new ResponseDTO { Message = "La résérvation est annulée", Status = 204 });
                }
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }




        /// <summary>
        /// Permet à un étudiant d'annuler sa propre réservation.
        /// </summary>
        /// <param name="slotId">Identifiant du créneau à annuler.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succès ou l'échec de l'opération.</returns>
        /// <response code="204">Réservation annulée avec succès.</response>
        /// <response code="400">Demande invalide ou réservation inexistante.</response>
        [HttpDelete("student/unbook")]
        public async Task<ActionResult<ResponseDTO>> RemoveReservationByStudent([FromQuery] string slotId)
        {
            try
            {
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }

                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }

                var resultRemove = await bookingService.RemoveReservationByStudent(slotId, user.Id);
                if (resultRemove)
                {
                    return Ok(new ResponseDTO { Message = "La résérvation est annulée", Status = 204 });
                }
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }




        /// <summary>
        /// Permet à un utilisateur de réserver plusieurs créneaux payants.
        /// </summary>
        /// <param name="slotIds">Liste des identifiants des créneaux à réserver.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succès ou l'échec de l'opération.</returns>
        /// <response code="204">Réservations enregistrées avec succès.</response>
        /// <response code="400">Demande invalide ou créneaux indisponibles.</response>
        [Authorize]
        [HttpPost("book-paid")]
        public async Task<ActionResult<ResponseDTO>> BookingPaid([FromBody] List<string> slotIds)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                if (slotIds is null || !slotIds.Any())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée ?" });
                }

                var checkAvailability = await context.Slots
                    .Where(x => slotIds.Contains(x.Id.ToString()) && x.Booking == null)
                    .ToListAsync();
                if (checkAvailability.Count != slotIds.Count)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée ?" });
                }
                else
                {
                    var bookings = new List<Booking>();
                    foreach (var slotId in slotIds)
                    {
                        // checks here
                    }
                    await context.Bookings.AddRangeAsync(bookings);
                    await context.SaveChangesAsync();
                    Order order = new Order
                    {
                        BookerId = user.Id,
                        CreatedAt = DateTimeOffset.Now,

                        //OrderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                        PaymentDate = DateTimeOffset.Now,
                        Status = EnumBookingStatus.Paid,
                        PaymentMethod = "card"
                    };
                    await context.Orders.AddAsync(order);
                    await context.SaveChangesAsync();

                    transaction.Commit();
                    return Ok(new ResponseDTO { Message = "La résérvation est enregistrée", Status = 204 });
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les réservations associées à un enseignant.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de recherche.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les réservations.</returns>
        /// <response code="200">Réservations récupérées avec succès.</response>
        /// <response code="400">Demande invalide.</response>
        [HttpPost("reservations-teacher")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> GetTeacherReservations([FromBody] QueryPagination query)
        {
            try
            {
                //var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                //if (user is null)
                //{
                //    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                //}
                if (query is null || query.PerPage <= 0 || query.Start < 0)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }

                var result = await bookingService.GetTeacherReservations(query);
                return Ok(new ResponseDTO { Message = "Demande acceptée", Status = 200, Count = result.Count, Data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }




        /// <summary>
        /// Récupère les réservations associées à un étudiant.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de recherche.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les réservations.</returns>
        /// <response code="200">Réservations récupérées avec succès.</response>
        /// <response code="400">Demande invalide.</response>
        [Authorize(Roles = "Student")]
        [HttpPost("reservations-student")]
        public async Task<ActionResult<ResponseDTO>> GetStudentReservations([FromBody] QueryPagination query)
        {
            try
            {
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                if (query is null || query.PerPage <= 0 || query.Start < 0)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }

                var result = await bookingService.GetStudentReservations(query, user);
                return Ok(new ResponseDTO { Message = "Demande acceptée", Status = 200, Count = result.Count, Data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }

        }
    }
}

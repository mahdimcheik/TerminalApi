using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Models.Bookings;
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
        private readonly IBookingService bookingService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur BookingController.
        /// </summary>
        /// <param name="bookingService">Service de gestion des réservations injecté.</param>
        /// <param name="context">Contexte de base de données injecté.</param>
        public BookingController(IBookingService bookingService, ApiDefaultContext context)
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
        public async Task<ActionResult<ResponseDTO<string?>>> BookSlot([FromBody] BookingCreateDTO bookingCreateDTO)
        {
            try
            {
                if (bookingCreateDTO.SlotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée" });
                }
                var resultBooking = await bookingService.BookSlot(bookingCreateDTO, user);
                if (resultBooking)
                {
                    return Ok(new ResponseDTO<string?> { Message = "La réservation est enregistrée", Status = 204 });
                }
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée, créneau déjà réservé ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
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
        public async Task<ActionResult<ResponseDTO<string?>>> RemoveReservationByTeacher([FromQuery] string slotId)
        {
            try
            {
                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée, la réservation n'existe pas !" });
                }
                var resultBooking = await bookingService.RemoveReservationByTeacher(slotId);
                if (resultBooking)
                {
                    return Ok(new ResponseDTO<string?> { Message = "La réservation est annulée", Status = 204 });
                }
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Récupère les communications associées à une réservation spécifique.
        /// </summary>
        /// <param name="bookingId">Identifiant de la réservation.</param>
        /// <returns>Liste des messages de communication pour cette réservation.</returns>
        /// <response code="200">Communications récupérées avec succès.</response>
        /// <response code="404">Réservation non trouvée.</response>
        [HttpGet("communications/{bookingId:Guid}")]
        public async Task<ActionResult<ResponseDTO<List<ChatMessage>>>> GetCommunicationsForBooking([FromRoute] Guid bookingId)
        {
            try
            {
                var communications = await bookingService.GetCommunicationsForBooking(bookingId);
                return Ok(new ResponseDTO<List<ChatMessage>>
                {
                    Message = "Demande valide",
                    Data = communications,
                    Status = 200
                });
            }catch(Exception ex)
            {
                return BadRequest(new ResponseDTO<List<ChatMessage>>
                {
                    Message = ex.Message,
                    Status = 404
                });
            }
        }

        /// <summary>
        /// Ajoute un nouveau message à la communication d'une réservation.
        /// </summary>
        /// <param name="bookingId">Identifiant de la réservation.</param>
        /// <param name="newMessage">Nouveau message à ajouter.</param>
        /// <returns>Confirmation de l'ajout du message.</returns>
        /// <response code="201">Message ajouté avec succès.</response>
        /// <response code="400">Demande invalide.</response>
        [HttpPost("communications/add-message/{bookingId:Guid}")]
        public async Task<ActionResult<bool>> AddMessage([FromRoute] Guid bookingId, [FromBody] ChatMessage newMessage )
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if(user is null)
            {
                BadRequest(new ResponseDTO<bool>
                {
                    Message = "Demande invalide",
                    Status = 400
                });
            }
            newMessage.userId = user.Id;
            newMessage.Author = user.FirstName +  " . " + user.LastName[..1].ToUpper();

            try
            {

            var result = await bookingService.AddMessage(bookingId, newMessage);
            if(result)
            {
                return Ok(new ResponseDTO<bool>
                {
                    Message = "Demande valide",
                    Status = 201
                });
            } 
            return BadRequest(new ResponseDTO<bool>
            {
                Message = "Demande invalide",
                Status = 400
            });
            }catch(Exception ex)
            {
                return BadRequest(new ResponseDTO<bool>
                {
                    Message = ex.Message,
                    Status = 400
                });
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
        public async Task<ActionResult<ResponseDTO<string?>>> RemoveReservationByStudent([FromQuery] string slotId)
        {
            try
            {
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée" });
                }

                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée" });
                }

                var resultRemove = await bookingService.RemoveReservationByStudent(slotId, user.Id);
                if (resultRemove)
                {
                    return Ok(new ResponseDTO<string?> { Message = "La réservation est annulée", Status = 204 });
                }
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refusée ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
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
        public async Task<ActionResult<ResponseDTO<string?>>> BookingPaid([FromBody] List<string> slotIds)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                if (slotIds is null || !slotIds.Any())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 400, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 400, Message = "Demande refusée ?" });
                }

                var checkAvailability = await context.Slots
                    .Where(x => slotIds.Contains(x.Id.ToString()) && x.Booking == null)
                    .ToListAsync();
                if (checkAvailability.Count != slotIds.Count)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 400, Message = "Demande refusée ?" });
                }
                else
                {
                    var bookings = new List<Booking>();
                    foreach (var slotId in slotIds)
                    {
                        // TODO vérifications ici
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
                    return Ok(new ResponseDTO<string?> { Message = "La réservation est enregistrée", Status = 204 });
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
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
        public async Task<ActionResult<ResponseDTO<List<BookingResponseDTO>>>> GetTeacherReservations([FromBody] QueryPagination query)
        {
            try
            {
                //var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                //if (user is null)
                //{
                //    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refusée" });
                //}
                if (query is null || query.PerPage <= 0 || query.Start < 0)
                {
                    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refusée" });
                }

                var result = await bookingService.GetTeacherReservations(query);
                return Ok(new ResponseDTO<List<BookingResponseDTO>> { Message = "Demande acceptée", Status = 200, Count = result.Count, Data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = ex.Message });
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
        public async Task<ActionResult<ResponseDTO<List<BookingResponseDTO>>>> GetStudentReservations([FromBody] QueryPagination query)
        {
            try
            {
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refusée" });
                }
                if (query is null || query.PerPage <= 0 || query.Start < 0)
                {
                    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refusée" });
                }

                var result = await bookingService.GetStudentReservations(query, user);
                return Ok(new ResponseDTO<List<BookingResponseDTO>> { Message = "Demande acceptée", Status = 200, Count = result.Count, Data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = ex.Message });
            }

        }
    }
}

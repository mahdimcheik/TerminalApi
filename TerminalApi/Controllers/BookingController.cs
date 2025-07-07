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
    /// Contr�leur responsable de la gestion des r�servations.
    /// Permet aux utilisateurs de r�server, annuler et consulter leurs r�servations.
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
        /// Initialise une nouvelle instance du contr�leur BookingController.
        /// </summary>
        /// <param name="bookingService">Service de gestion des r�servations inject�.</param>
        /// <param name="context">Contexte de base de donn�es inject�.</param>
        public BookingController(BookingService bookingService, ApiDefaultContext context)
        {
            this.bookingService = bookingService;
            this.context = context;
        }

        /// <summary>
        /// Permet � un utilisateur de r�server un cr�neau horaire.
        /// </summary>
        /// <param name="bookingCreateDTO">Donn�es de la r�servation � cr�er.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succ�s ou l'�chec de l'op�ration.</returns>
        /// <response code="204">R�servation enregistr�e avec succ�s.</response>
        /// <response code="400">Demande invalide ou cr�neau d�j� r�serv�.</response>
        [HttpPost("book")]
        public async Task<ActionResult<ResponseDTO<string?>>> BookSlot([FromBody] BookingCreateDTO bookingCreateDTO)
        {
            try
            {
                if (bookingCreateDTO.SlotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }
                var resultBooking = await bookingService.BookSlot(bookingCreateDTO, user);
                if (resultBooking)
                {
                    return Ok(new ResponseDTO<string?> { Message = "La r�s�rvation est enregistr�e", Status = 204 });
                }
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e, cr�neau d�j� r�s�rv� ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Permet � un administrateur d'annuler une r�servation pour un cr�neau sp�cifique.
        /// </summary>
        /// <param name="slotId">Identifiant du cr�neau � annuler.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succ�s ou l'�chec de l'op�ration.</returns>
        /// <response code="204">R�servation annul�e avec succ�s.</response>
        /// <response code="400">Demande invalide ou r�servation inexistante.</response>
        [HttpDelete("unbook")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<string?>>> RemoveReservationByTeacher([FromQuery] string slotId)
        {
            try
            {
                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e, la r�servation n'existe pas!!!" });
                }
                var resultBooking = await bookingService.RemoveReservationByTeacher(slotId);
                if (resultBooking)
                {
                    return Ok(new ResponseDTO<string?> { Message = "La r�s�rvation est annul�e", Status = 204 });
                }
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
            }
        }




        /// <summary>
        /// Permet � un �tudiant d'annuler sa propre r�servation.
        /// </summary>
        /// <param name="slotId">Identifiant du cr�neau � annuler.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succ�s ou l'�chec de l'op�ration.</returns>
        /// <response code="204">R�servation annul�e avec succ�s.</response>
        /// <response code="400">Demande invalide ou r�servation inexistante.</response>
        [HttpDelete("student/unbook")]
        public async Task<ActionResult<ResponseDTO<string?>>> RemoveReservationByStudent([FromQuery] string slotId)
        {
            try
            {
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }

                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }

                var resultRemove = await bookingService.RemoveReservationByStudent(slotId, user.Id);
                if (resultRemove)
                {
                    return Ok(new ResponseDTO<string?> { Message = "La r�s�rvation est annul�e", Status = 204 });
                }
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e ?" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
            }
        }




        /// <summary>
        /// Permet � un utilisateur de r�server plusieurs cr�neaux payants.
        /// </summary>
        /// <param name="slotIds">Liste des identifiants des cr�neaux � r�server.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> indiquant le succ�s ou l'�chec de l'op�ration.</returns>
        /// <response code="204">R�servations enregistr�es avec succ�s.</response>
        /// <response code="400">Demande invalide ou cr�neaux indisponibles.</response>
        [Authorize]
        [HttpPost("book-paid")]
        public async Task<ActionResult<ResponseDTO<string?>>> BookingPaid([FromBody] List<string> slotIds)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                if (slotIds is null || !slotIds.Any())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e ?" });
                }

                var checkAvailability = await context.Slots
                    .Where(x => slotIds.Contains(x.Id.ToString()) && x.Booking == null)
                    .ToListAsync();
                if (checkAvailability.Count != slotIds.Count)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e ?" });
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
                    return Ok(new ResponseDTO<string?> { Message = "La r�s�rvation est enregistr�e", Status = 204 });
                }

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// R�cup�re les r�servations associ�es � un enseignant.
        /// </summary>
        /// <param name="query">Param�tres de pagination et de recherche.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les r�servations.</returns>
        /// <response code="200">R�servations r�cup�r�es avec succ�s.</response>
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
                //    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refus�e" });
                //}
                if (query is null || query.PerPage <= 0 || query.Start < 0)
                {
                    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refus�e" });
                }

                var result = await bookingService.GetTeacherReservations(query);
                return Ok(new ResponseDTO<List<BookingResponseDTO>> { Message = "Demande accept�e", Status = 200, Count = result.Count, Data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = ex.Message });
            }
        }




        /// <summary>
        /// R�cup�re les r�servations associ�es � un �tudiant.
        /// </summary>
        /// <param name="query">Param�tres de pagination et de recherche.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les r�servations.</returns>
        /// <response code="200">R�servations r�cup�r�es avec succ�s.</response>
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
                    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refus�e" });
                }
                if (query is null || query.PerPage <= 0 || query.Start < 0)
                {
                    return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = "Demande refus�e" });
                }

                var result = await bookingService.GetStudentReservations(query, user);
                return Ok(new ResponseDTO<List<BookingResponseDTO>> { Message = "Demande accept�e", Status = 200, Count = result.Count, Data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<List<BookingResponseDTO>> { Status = 40, Message = ex.Message });
            }

        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Models.User;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class BookingController : ControllerBase
    {
        private readonly BookingService bookingService
            ;
        private readonly ApiDefaultContext context;

        public BookingController(BookingService bookingService, ApiDefaultContext context)
        {
            this.bookingService = bookingService;
            this.context = context;
        }
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

        private ActionResult<ResponseDTO> BadRequest(ResponseDTO responseDTO)
        {
            throw new NotImplementedException();
        }

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

        //[Authorize(Roles ="Admin")]
        [HttpPost("reservations-teacher")]
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

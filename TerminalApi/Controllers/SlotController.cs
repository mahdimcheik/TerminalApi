using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Payments;
using TerminalApi.Models.Slots;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SlotController : ControllerBase
    {
        private readonly SlotService slotService;
        private readonly ApiDefaultContext context;

        public SlotController(SlotService slotService, ApiDefaultContext context)
        {
            this.slotService = slotService;
            this.context = context;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> GetSlotsByCreatorId([FromQuery] string userId, [FromQuery] DateTimeOffset fromDate, [FromQuery] DateTimeOffset toDate)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Status = 404, Message = "L'utilisateur n'existe pas" }
                );
            }

            try
            {
                var result = await slotService.GetSlotsByCreator(userId, fromDate, toDate);
                return Ok(
                    new ResponseDTO
                    {
                        Status = 200,
                        Message = "Liste de créneaux envoyée",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }

        [HttpGet("student")]
        public async Task<ActionResult<ResponseDTO>> GetSlotsForStudent( [FromQuery] DateTimeOffset fromDate, [FromQuery] DateTimeOffset toDate)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if(user is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Quelque chose ne va pas !!!" });
            try
            {
                var result = await slotService.GetSlotsByStudent(user.Id, fromDate, toDate);
                return Ok(
                    new ResponseDTO
                    {
                        Status = 200,
                        Message = "Liste de créneaux envoyée",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> AddSlot([FromBody] SlotCreateDTO slotCreateDTO)
        {
            if (slotCreateDTO is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }

            try
            {
                var result = await slotService.AddSlot(slotCreateDTO, user.Id);
                return Ok(new ResponseDTO { Data = result, Message = "Créneau ajoutée", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseDTO>> UpdateSlot([FromBody] SlotUpdateDTO slotUpdateDTO)
        {
            if (slotUpdateDTO is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }

            try
            {
                var result = await slotService.UpdateSlot(slotUpdateDTO, user.Id);
                return Ok(new ResponseDTO { Data = result, Message = "Créneau ajoutée", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> DeleteSlot([FromQuery] string slotId)
        {
            try
            {
                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var resultDelete = await slotService.DeleteSlot(user.Id, slotId);
                return Ok(new ResponseDTO { Message = "l'addresse est supprimée", Status = 204 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
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
                var resultBooking = await slotService.BookSlot(bookingCreateDTO.SlotId, user.Id);
                if(resultBooking)
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

        [HttpDelete("unbook")]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult<ResponseDTO>> RemoveReservationByTeacher([FromQuery] string slotId )
        {
            try
            {
                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée, la réservation n'existe pas!!!" });
                }
                var resultBooking = await slotService.RemoveReservationByTeacher(slotId);
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

                var resultRemove = await slotService.RemoveReservationByStudent(slotId, user.Id);
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
                        bookings.Add(new Booking
                        {
                            SlotId = Guid.Parse(slotId),
                            BookedById = user.Id,
                            CreatedAt = DateTimeOffset.Now
                        });
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
                        PaymentMethod = "Stripe"
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
    }
}

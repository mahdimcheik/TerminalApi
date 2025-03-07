using Microsoft.AspNetCore.Authorization;
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
    //[Authorize]
    public class SlotController : ControllerBase
    {
        private readonly SlotService slotService;
        private readonly ApiDefaultContext context;

        public SlotController(SlotService slotService, ApiDefaultContext context)
        {
            this.slotService = slotService;
            this.context = context;
        }
        [HttpGet("slotId/{slotId}")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO>> GetSlotsForStudent([FromRoute] string slotId)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Quelque chose ne va pas !!!" });
            try
            {
                var result = await slotService.GetSlotsById(slotId);
                return Ok(
                    new ResponseDTO
                    {
                        Status = 200,
                        Message = "Créneau envoyé",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
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
        public async Task<ActionResult<ResponseDTO>> GetSlotsForStudent([FromQuery] DateTimeOffset fromDate, [FromQuery] DateTimeOffset toDate)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Quelque chose ne va pas !!!" });
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> AddSlot([FromBody] SlotCreateDTO slotCreateDTO)
        {
            if (slotCreateDTO is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            if (slotCreateDTO.StartAt < DateTimeOffset.Now) return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            if (slotCreateDTO.StartAt >= slotCreateDTO.EndAt) return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });

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
        [Authorize(Roles = "Admin")]
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


    }
}

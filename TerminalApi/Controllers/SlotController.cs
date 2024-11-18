using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Slots;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
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
        public async Task<ActionResult<ResponseDTO>> GetSlotsByCreatorId([FromQuery] string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Status = 404, Message = "L'utilisateur n'existe pas" }
                );
            }
            try
            {
                var result = await slotService.GetSlotsByCreator(userId);
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
            if(slotCreateDTO is null) return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
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
    }
}

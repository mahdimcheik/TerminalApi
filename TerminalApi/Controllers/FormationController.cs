using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Migrations;
using TerminalApi.Models;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Formations;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class FormationController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly FormationService formationService;

        public FormationController(ApiDefaultContext context, FormationService formationService)
        {
            this.context = context;
            this.formationService = formationService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<ResponseDTO>> GetFormationsByUserId([FromQuery] string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Status = 404, Message = "L'utilisateur n'existe pas" }
                );
            }
            try
            {
                var result = await formationService.GetFormations(userId);
                return Ok(
                    new ResponseDTO
                    {
                        Status = 200,
                        Message = "Liste d'addresses envoyée",
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
        public async Task<ActionResult<ResponseDTO>> AddFormation([FromBody] FormationCreateDTO formation)
        {
            if (formation is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }

            try
            {
                var result = await formationService.AddFormation(formation, user.Id);
                return Ok(new ResponseDTO { Data = result, Message = "Addresse ajoutée", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }
        [HttpPut]
        public async Task<ActionResult<ResponseDTO>> UpdateFormation([FromBody] FormationUpdateDTO formationDTO)

        {
            if (formationDTO is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);

            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }

            var formationFromDb = await context.Formations.FirstOrDefaultAsync(x => x.Id == Guid.Parse(formationDTO.Id) && x.UserId == user.Id);

            if (formationFromDb is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }

            try
            {
                var result = await formationService.UpdateFormation(formationDTO, formationFromDb);
                return Ok(new ResponseDTO { Data = result, Message = "Addresse ajoutée", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }

        }

        [HttpDelete]
        public async Task<ActionResult<ResponseDTO>> DeleteFormation([FromQuery] string formationId)
        {
            try
            {
                if (formationId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
                }
                var resultDelete = await formationService.DeleteFormation(user.Id, formationId);
                return Ok(new ResponseDTO { Message = "l'addresse est supprimée", Status = 204 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }
    }
}

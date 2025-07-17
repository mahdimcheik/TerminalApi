using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contr�leur responsable de la gestion des formations des utilisateurs.
    /// Permet de r�cup�rer, ajouter, mettre � jour et supprimer des formations.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class FormationController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly IFormationService formationService;

        /// <summary>
        /// Initialise une nouvelle instance du contr�leur avec les services n�cessaires.
        /// </summary>
        /// <param name="context">Contexte de base de donn�es inject�.</param>
        /// <param name="formationService">Service de gestion des formations inject�.</param>
        public FormationController(ApiDefaultContext context, IFormationService formationService)
        {
            this.context = context;
            this.formationService = formationService;
        }

        /// <summary>
        /// R�cup�re toutes les formations associ�es � un utilisateur donn�.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> contenant la liste des formations (200),
        /// ou un message d'erreur si l'utilisateur n'existe pas (404) ou en cas d'erreur (400).
        /// </returns>
        [HttpGet("all")]
        public async Task<ActionResult<ResponseDTO<List<FormationResponseDTO>>>> GetFormationsByUserId([FromQuery] string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO<object> { Status = 404, Message = "L'utilisateur n'existe pas" }
                );
            }
            try
            {
                var result = await formationService.GetFormations(userId);
                return Ok(
                    new ResponseDTO<List<FormationResponseDTO>>
                    {
                        Status = 200,
                        Message = "Liste d'addresses envoy�e",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 400, Message = ex.Message });
            }
        }

        /// <summary>
        /// Ajoute une nouvelle formation pour l'utilisateur connect�.
        /// </summary>
        /// <param name="formation">Donn�es de la formation � ajouter.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> contenant la formation ajout�e (200),
        /// ou un message d'erreur en cas de donn�es invalides (400).
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<FormationResponseDTO>>> AddFormation([FromBody] FormationCreateDTO formation)
        {
            if (formation is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 400, Message = "Demande refus�e" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }

            try
            {
                var result = await formationService.AddFormation(formation, user.Id);
                return Ok(new ResponseDTO<object> { Data = result, Message = "Addresse ajout�e", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Met � jour une formation existante pour l'utilisateur connect�.
        /// </summary>
        /// <param name="formationDTO">Donn�es de la formation � mettre � jour.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> contenant la formation mise � jour (200),
        /// ou un message d'erreur en cas de donn�es invalides ou si la formation n'existe pas (400).
        /// </returns>
        [HttpPut]
        public async Task<ActionResult<ResponseDTO<FormationResponseDTO>>> UpdateFormation([FromBody] FormationUpdateDTO formationDTO)
        {
            if (formationDTO is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);

            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }

            var formationFromDb = await context.Formations.FirstOrDefaultAsync(x => x.Id == Guid.Parse(formationDTO.Id) && x.UserId == user.Id);

            if (formationFromDb is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }

            try
            {
                var result = await formationService.UpdateFormation(formationDTO, formationFromDb);
                return Ok(new ResponseDTO<object> { Data = result, Message = "Addresse ajout�e", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une formation existante pour l'utilisateur connect�.
        /// </summary>
        /// <param name="formationId">Identifiant de la formation � supprimer.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> indiquant le succ�s de l'op�ration (204),
        /// ou un message d'erreur en cas de donn�es invalides ou d'erreur (400).
        /// </returns>
        [HttpDelete]
        public async Task<ActionResult<ResponseDTO<object>>> DeleteFormation([FromQuery] string formationId)
        {
            try
            {
                if (formationId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
                }
                var resultDelete = await formationService.DeleteFormation(user.Id, formationId);
                return Ok(new ResponseDTO<object> { Message = "l'addresse est supprim�e", Status = 204 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }
    }
}

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
    /// Contrôleur responsable de la gestion des formations des utilisateurs.
    /// Permet de récupérer, ajouter, mettre à jour et supprimer des formations.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class FormationController : ControllerBase
    {
        private readonly ApiDefaultContext context;
        private readonly FormationService formationService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur avec les services nécessaires.
        /// </summary>
        /// <param name="context">Contexte de base de données injecté.</param>
        /// <param name="formationService">Service de gestion des formations injecté.</param>
        public FormationController(ApiDefaultContext context, FormationService formationService)
        {
            this.context = context;
            this.formationService = formationService;
        }

        /// <summary>
        /// Récupère toutes les formations associées à un utilisateur donné.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> contenant la liste des formations (200),
        /// ou un message d'erreur si l'utilisateur n'existe pas (404) ou en cas d'erreur (400).
        /// </returns>
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

        /// <summary>
        /// Ajoute une nouvelle formation pour l'utilisateur connecté.
        /// </summary>
        /// <param name="formation">Données de la formation à ajouter.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> contenant la formation ajoutée (200),
        /// ou un message d'erreur en cas de données invalides (400).
        /// </returns>
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

        /// <summary>
        /// Met à jour une formation existante pour l'utilisateur connecté.
        /// </summary>
        /// <param name="formationDTO">Données de la formation à mettre à jour.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> contenant la formation mise à jour (200),
        /// ou un message d'erreur en cas de données invalides ou si la formation n'existe pas (400).
        /// </returns>
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

        /// <summary>
        /// Supprime une formation existante pour l'utilisateur connecté.
        /// </summary>
        /// <param name="formationId">Identifiant de la formation à supprimer.</param>
        /// <returns>
        /// Un objet <see cref="ResponseDTO"/> indiquant le succès de l'opération (204),
        /// ou un message d'erreur en cas de données invalides ou d'erreur (400).
        /// </returns>
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

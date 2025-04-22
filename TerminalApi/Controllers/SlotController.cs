using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Slots;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des créneaux horaires (Slots).
    /// Permet de récupérer, ajouter, mettre à jour et supprimer des créneaux.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SlotController : ControllerBase
    {
        private readonly SlotService slotService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur avec les services nécessaires.
        /// </summary>
        /// <param name="slotService">Service pour la gestion des créneaux.</param>
        /// <param name="context">Contexte de base de données.</param>
        public SlotController(SlotService slotService, ApiDefaultContext context)
        {
            this.slotService = slotService;
            this.context = context;
        }

        /// <summary>
        /// Récupère un créneau spécifique par son identifiant.
        /// </summary>
        /// <param name="slotId">Identifiant du créneau.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les détails du créneau.</returns>
        /// <response code="200">Créneau trouvé et retourné avec succès.</response>
        /// <response code="400">Requête invalide ou erreur interne.</response>
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

        /// <summary>
        /// Récupère les créneaux créés par un utilisateur spécifique dans une plage de dates.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur créateur.</param>
        /// <param name="fromDate">Date de début de la plage.</param>
        /// <param name="toDate">Date de fin de la plage.</param>
        /// <returns>Une liste de créneaux sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Liste des créneaux retournée avec succès.</response>
        /// <response code="404">Utilisateur non trouvé.</response>
        /// <response code="400">Requête invalide ou erreur interne.</response>
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

        /// <summary>
        /// Récupère les créneaux associés à un étudiant dans une plage de dates.
        /// </summary>
        /// <param name="fromDate">Date de début de la plage.</param>
        /// <param name="toDate">Date de fin de la plage.</param>
        /// <returns>Une liste de créneaux sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Liste des créneaux retournée avec succès.</response>
        /// <response code="400">Requête invalide ou erreur interne.</response>
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

        /// <summary>
        /// Ajoute un nouveau créneau.
        /// </summary>
        /// <param name="slotCreateDTO">Données du créneau à créer.</param>
        /// <returns>Le créneau créé sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Créneau ajouté avec succès.</response>
        /// <response code="400">Requête invalide ou erreur interne.</response>
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

        /// <summary>
        /// Met à jour un créneau existant.
        /// </summary>
        /// <param name="slotUpdateDTO">Données du créneau à mettre à jour.</param>
        /// <returns>Le créneau mis à jour sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Créneau mis à jour avec succès.</response>
        /// <response code="400">Requête invalide ou erreur interne.</response>
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

        /// <summary>
        /// Supprime un créneau existant.
        /// </summary>
        /// <param name="slotId">Identifiant du créneau à supprimer.</param>
        /// <returns>Un message de confirmation sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="204">Créneau supprimé avec succès.</response>
        /// <response code="400">Requête invalide ou erreur interne.</response>
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

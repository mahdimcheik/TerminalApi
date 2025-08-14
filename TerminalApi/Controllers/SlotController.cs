using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contr�leur responsable de la gestion des cr�neaux horaires (Slots).
    /// Permet de r�cup�rer, ajouter, mettre � jour et supprimer des cr�neaux.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SlotController : ControllerBase
    {
        private readonly ISlotService slotService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contr�leur avec les services n�cessaires.
        /// </summary>
        /// <param name="slotService">Service pour la gestion des cr�neaux.</param>
        /// <param name="context">Contexte de base de donn�es.</param>
        public SlotController(ISlotService slotService, ApiDefaultContext context)
        {
            this.slotService = slotService;
            this.context = context;
        }

        /// <summary>
        /// R�cup�re un cr�neau sp�cifique par son identifiant.
        /// </summary>
        /// <param name="slotId">Identifiant du cr�neau.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les d�tails du cr�neau.</returns>
        /// <response code="200">Cr�neau trouv� et retourn� avec succ�s.</response>
        /// <response code="400">Requ�te invalide ou erreur interne.</response>
        [HttpGet("slotId/{slotId}")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO<List<SlotResponseDTO>>>> GetSlotsForStudent([FromRoute] string slotId)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null) return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Quelque chose ne va pas !!!" });
            try
            {
                var result = await slotService.GetSlotsById(slotId);
                return Ok(
                    new ResponseDTO<object> {
                        Status = 200,
                        Message = "Cr�neau envoy�",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// R�cup�re les cr�neaux cr��s par un utilisateur sp�cifique dans une plage de dates.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur cr�ateur.</param>
        /// <param name="fromDate">Date de d�but de la plage.</param>
        /// <param name="toDate">Date de fin de la plage.</param>
        /// <returns>Une liste de cr�neaux sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Liste des cr�neaux retourn�e avec succ�s.</response>
        /// <response code="404">Utilisateur non trouv�.</response>
        /// <response code="400">Requ�te invalide ou erreur interne.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<List<SlotResponseDTO>>>> GetSlotsByCreatorId([FromQuery] string userId, [FromQuery] DateTimeOffset fromDate, [FromQuery] DateTimeOffset toDate)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO<object> { Status = 40, Message = "L'utilisateur n'existe pas" }
                );
            }

            try
            {
                var result = await slotService.GetSlotsByCreator(userId, fromDate, toDate);
                return Ok(
                    new ResponseDTO<object> {
                        Status = 200,
                        Message = "Liste de cr�neaux envoy�e",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// R�cup�re les cr�neaux associ�s � un �tudiant dans une plage de dates.
        /// </summary>
        /// <param name="fromDate">Date de d�but de la plage.</param>
        /// <param name="toDate">Date de fin de la plage.</param>
        /// <returns>Une liste de cr�neaux sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Liste des cr�neaux retourn�e avec succ�s.</response>
        /// <response code="400">Requ�te invalide ou erreur interne.</response>
        [HttpGet("student")]
        public async Task<ActionResult<ResponseDTO<List<SlotResponseDTO>>>> GetSlotsForStudent([FromQuery] DateTimeOffset fromDate, [FromQuery] DateTimeOffset toDate)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null) return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Quelque chose ne va pas !!!" });
            try
            {
                var result = await slotService.GetSlotsByStudent(user.Id, fromDate, toDate);
                return Ok(
                    new ResponseDTO<List<SlotResponseDTO>> {
                        Status = 200,
                        Message = "Liste de cr�neaux envoy�e",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Ajoute un nouveau cr�neau.
        /// </summary>
        /// <param name="slotCreateDTO">Donn�es du cr�neau � cr�er.</param>
        /// <returns>Le cr�neau cr�� sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Cr�neau ajout� avec succ�s.</response>
        /// <response code="400">Requ�te invalide ou erreur interne.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<SlotResponseDTO>>> AddSlot([FromBody] SlotCreateDTO slotCreateDTO)
        {
            if (slotCreateDTO is null) return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            if (slotCreateDTO.StartAt < DateTimeOffset.Now) return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            if (slotCreateDTO.StartAt >= slotCreateDTO.EndAt) return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });

            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }

            try
            {
                var result = await slotService.AddSlot(slotCreateDTO, user.Id);
                return Ok(new ResponseDTO<object> { Data = result, Message = "Cr�neau ajout�e", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Met � jour un cr�neau existant.
        /// </summary>
        /// <param name="slotUpdateDTO">Donn�es du cr�neau � mettre � jour.</param>
        /// <returns>Le cr�neau mis � jour sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="200">Cr�neau mis � jour avec succ�s.</response>
        /// <response code="400">Requ�te invalide ou erreur interne.</response>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<SlotResponseDTO>>> UpdateSlot([FromBody] SlotUpdateDTO slotUpdateDTO)
        {
            if (slotUpdateDTO is null) return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }

            try
            {
                var result = await slotService.UpdateSlot(slotUpdateDTO, user.Id);
                return Ok(new ResponseDTO<object> { Data = result, Message = "Cr�neau ajout�e", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime un cr�neau existant.
        /// </summary>
        /// <param name="slotId">Identifiant du cr�neau � supprimer.</param>
        /// <returns>Un message de confirmation sous forme de <see cref="ResponseDTO"/>.</returns>
        /// <response code="204">Cr�neau supprim� avec succ�s.</response>
        /// <response code="400">Requ�te invalide ou erreur interne.</response>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<object>>> DeleteSlot([FromQuery] string slotId)
        {
            try
            {
                if (slotId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
                }
                var resultDelete = await slotService.DeleteSlot(user.Id, slotId);
                return Ok(new ResponseDTO<object> { Message = "l'addresse est supprim�e", Status = 204 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }
    }
}

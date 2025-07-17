using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contr�leur responsable de la gestion des notifications des utilisateurs.
    /// Permet d'ajouter, r�cup�rer et mettre � jour les notifications.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contr�leur NotificationsController.
        /// </summary>
        /// <param name="notificationService">Service pour la gestion des notifications.</param>
        /// <param name="context">Contexte de base de donn�es pour acc�der aux entit�s.</param>
        public NotificationsController(INotificationService notificationService, ApiDefaultContext context)
        {
            _notificationService = notificationService;
            this.context = context;
        }

        /// <summary>
        /// Ajoute une nouvelle notification.
        /// </summary>
        /// <param name="notification">Objet Notification contenant les d�tails de la notification � ajouter.</param>
        /// <returns>
        /// Un objet ResponseDTO contenant les d�tails de la notification ajout�e.
        /// Codes HTTP possibles :
        /// - 201 : Notification ajout�e avec succ�s.
        /// - 400 : Erreur de validation ou probl�me lors de l'ajout.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<NotificationResponseDTO?>>> AddNotification(Notification notification)
        {
            try
            {
                var response = await _notificationService.AddNotification(notification);
                return Ok(
                    new ResponseDTO<object> {
                        Data = response,
                        Status = StatusCodes.Status201Created,
                        Message = "Notification ajout�e avec succ�s"
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO<object> {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Notification non-ajout�e {e.Message} "
                    }
                );
            }
        }

        /// <summary>
        /// R�cup�re les notifications d'un utilisateur en fonction d'un filtre.
        /// </summary>
        /// <param name="filter">Filtre pour sp�cifier les crit�res de r�cup�ration des notifications.</param>
        /// <returns>
        /// Un objet ResponseDTO contenant une liste pagin�e des notifications.
        /// Codes HTTP possibles :
        /// - 200 : Notifications r�cup�r�es avec succ�s.
        /// - 400 : Utilisateur non authentifi� ou probl�me de validation.
        /// - 404 : Notifications non trouv�es.
        /// </returns>
        [HttpPost("user")]
        public async Task<ActionResult<ResponseDTO< PaginatedNotificationResult<NotificationResponseDTO>>>> GetUserNotifications(
            [FromBody] NotificationFilter filter
        )
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }
            try
            {
                var response = await _notificationService.GetUserNotificationsAsync(user.Id, filter);
                return Ok(
                    new ResponseDTO<object> {
                        Data = response,
                        Count = response.TotalItems,
                        Status = StatusCodes.Status200OK,
                        Message = "Notifications r�cup�r�es avec succ�s"
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO<object> {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Notifications non-trouv�es {e.Message}"
                    }
                );
            }
        }


        [HttpGet("count")]
        public async Task<ActionResult<ResponseDTO<int>>> GetUserNotificationsCount()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }
            try
            {
                var count = await _notificationService.GetUserNotificationsCountAsync(user.Id);
                return Ok(
                    new ResponseDTO<object>
                    {
                        Count = count,
                        Status = StatusCodes.Status200OK,
                        Message = "Notifications r�cup�r�es avec succ�s",
                        Data = count
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO<object>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Je ne sais pas : {e.Message}"
                    }
                );
            }
        }

        /// <summary>
        /// Met � jour l'�tat d'une notification (par exemple, marquer comme lue ou non lue).
        /// </summary>
        /// <param name="notificationId">Identifiant unique de la notification � mettre � jour.</param>
        /// <param name="newValue">Nouvelle valeur de l'�tat (true pour lu, false pour non lu).</param>
        /// <returns>
        /// Un objet ResponseDTO contenant les d�tails de la notification mise � jour.
        /// Codes HTTP possibles :
        /// - 200 : Notification mise � jour avec succ�s.
        /// - 400 : Utilisateur non authentifi� ou notification non trouv�e.
        /// - 404 : Erreur lors de la mise � jour.
        /// </returns>
        [HttpPut("{notificationId}/{newValue}")]
        public async Task<ActionResult<ResponseDTO<NotificationResponseDTO>>> UpdateNotification(Guid notificationId, bool newValue)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }
            var notification = await context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientId == user.Id);
            if (notification is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refus�e" });
            }
            try
            {
                var response = await _notificationService.Update(notification, newValue);
                return Ok(
                    new ResponseDTO<object> {
                        Data = response.Data,
                        Status = response.Status,
                        Message = response.Message
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO<object> {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Notification non-mise � jour {e.Message}"
                    }
                );
            }
        }
    }
}

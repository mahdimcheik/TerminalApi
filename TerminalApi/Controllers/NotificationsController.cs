using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des notifications des utilisateurs.
    /// Permet d'ajouter, récupérer et mettre à jour les notifications.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur NotificationsController.
        /// </summary>
        /// <param name="notificationService">Service pour la gestion des notifications.</param>
        /// <param name="context">Contexte de base de données pour accéder aux entités.</param>
        public NotificationsController(NotificationService notificationService, ApiDefaultContext context)
        {
            _notificationService = notificationService;
            this.context = context;
        }

        /// <summary>
        /// Ajoute une nouvelle notification.
        /// </summary>
        /// <param name="notification">Objet Notification contenant les détails de la notification à ajouter.</param>
        /// <returns>
        /// Un objet ResponseDTO contenant les détails de la notification ajoutée.
        /// Codes HTTP possibles :
        /// - 201 : Notification ajoutée avec succès.
        /// - 400 : Erreur de validation ou problème lors de l'ajout.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddNotification(Notification notification)
        {
            try
            {
                var response = await _notificationService.AddNotification(notification);
                return Ok(
                    new ResponseDTO
                    {
                        Data = response,
                        Status = StatusCodes.Status201Created,
                        Message = "Notification ajoutée avec succès"
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Notification non-ajoutée {e.Message} "
                    }
                );
            }
        }

        /// <summary>
        /// Récupère les notifications d'un utilisateur en fonction d'un filtre.
        /// </summary>
        /// <param name="filter">Filtre pour spécifier les critères de récupération des notifications.</param>
        /// <returns>
        /// Un objet ResponseDTO contenant une liste paginée des notifications.
        /// Codes HTTP possibles :
        /// - 200 : Notifications récupérées avec succès.
        /// - 400 : Utilisateur non authentifié ou problème de validation.
        /// - 404 : Notifications non trouvées.
        /// </returns>
        [HttpPost("user")]
        public async Task<IActionResult> GetUserNotifications(
            [FromBody] NotificationFilter filter
        )
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            try
            {
                var response = await _notificationService.GetUserNotificationsAsync(user.Id, filter);
                return Ok(
                    new ResponseDTO
                    {
                        Data = response,
                        Count = response.TotalItems,
                        Status = StatusCodes.Status200OK,
                        Message = "Notifications récupérées avec succès"
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Notifications non-trouvées {e.Message}"
                    }
                );
            }
        }

        /// <summary>
        /// Met à jour l'état d'une notification (par exemple, marquer comme lue ou non lue).
        /// </summary>
        /// <param name="notificationId">Identifiant unique de la notification à mettre à jour.</param>
        /// <param name="newValue">Nouvelle valeur de l'état (true pour lu, false pour non lu).</param>
        /// <returns>
        /// Un objet ResponseDTO contenant les détails de la notification mise à jour.
        /// Codes HTTP possibles :
        /// - 200 : Notification mise à jour avec succès.
        /// - 400 : Utilisateur non authentifié ou notification non trouvée.
        /// - 404 : Erreur lors de la mise à jour.
        /// </returns>
        [HttpPut("{notificationId}/{newValue}")]
        public async Task<IActionResult> UpdateNotification(Guid notificationId, bool newValue)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var notification = await context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientId == user.Id);
            if (notification is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            try
            {
                var response = await _notificationService.Update(notification, newValue);
                return Ok(
                    new ResponseDTO
                    {
                        Data = response.Data,
                        Status = response.Status,
                        Message = response.Message
                    }
                );
            }
            catch (Exception e)
            {
                return BadRequest(
                    new ResponseDTO
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = $"Notification non-mise à jour {e.Message}"
                    }
                );
            }
        }
    }
}

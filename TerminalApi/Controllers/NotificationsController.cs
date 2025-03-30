using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Notification;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;
        private readonly ApiDefaultContext context;

        public NotificationsController(NotificationService notificationService, ApiDefaultContext context)
        {
            _notificationService = notificationService;
            this.context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddNotification(Notification notification)
        {
            try
            {
                var response = await _notificationService.AddNotification(notification);
                return  Ok(
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

        [HttpGet("user")]
        public async Task<IActionResult> GetUserNotifications(
            [FromQuery] NotificationFilter filter
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

        [HttpPut("{notificationId}/{newValue}")]
        public async Task<IActionResult> UpdateNotification(Guid notificationId, bool newValue)
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var notification =  await context.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientId == user.Id);
            if(notification is null)
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

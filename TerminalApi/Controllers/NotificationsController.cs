using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using TerminalApi.Models;
using TerminalApi.Models.Notification;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationsController(NotificationService notificationService)
        {
            _notificationService = notificationService;
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
                        Message = "Notification non-ajoutée"
                    }
                );
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(
            string userId,
            [FromQuery] NotificationFilter filter
        )
        {
            try
            {
                var response = await _notificationService.GetUserNotificationsAsync(userId, filter);
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
                        Message = "Notifications non-trouvées"
                    }
                );
            }
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SseController : ControllerBase
    {
        private readonly SseService _sseService;

        public SseController(SseService sseService)
        {
            _sseService = sseService;
        }

        [HttpGet("{email}/{token}")]
        public async Task Get(string email,string token, CancellationToken cancellationToken)
        {
            if(email is null || email.IsNullOrEmpty() || token.IsNullOrEmpty())
            {
                return;
            }

            try
            {

                var principal = UtilitiesUser.GetPrincipalFromToken(token);
                
                var userEmail = principal
                    ?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)
                    ?.Value?.ToLower();
                if(userEmail.IsNullOrEmpty() || userEmail != email )
                {
                    return;
                }

            }
            catch { return; }

            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var reader = _sseService.ConnectUser(email, token);
             
            await using var writer = new StreamWriter(Response.Body, Encoding.UTF8, leaveOpen: true);

            try
            {
                await foreach (var message in reader.ReadAllAsync(cancellationToken))
                {
                    await writer.WriteAsync($"data: {message}\n\n");
                    await writer.FlushAsync();
                }
            }
            finally
            {
                _sseService.DisconnectUser(email, token);
            }
        }

        [HttpPost("notify/{userId}")]
        public async Task<IActionResult> SendMessage(string userId, [FromBody] string message)
        {
            await _sseService.SendMessageToUserAsync(userId, message);
            return Ok($"Message sent to {userId}");
        }
    }

}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly SseConnectionManager _connectionManager;

        public SseController(SseConnectionManager connectionManager)
        {           
             _connectionManager = connectionManager;
        }

        [HttpGet("{userId}")]
        public async Task SseEndpoint([FromRoute] string userId, CancellationToken cancellationToken)
        {
            var toto = userId;
            await _connectionManager.Subscribe(Response,userId, cancellationToken,  HttpContext);
        }

        [HttpPost("notify/{clientId}/{type:int}")]
        public async Task SendMessageToClient(string clientId, EnumEventSSEType type, CancellationToken cancellationToken, [FromBody] object? message)
        {
            var res = await _connectionManager.NotifyUserById(clientId, type, message, cancellationToken);
        }

        [HttpPost("notify-all/{type:int}")]
        public async Task BroadcastMessage(EnumEventSSEType type, [FromBody] object? message, CancellationToken cancellationToken )
        {
            await _connectionManager.NotifyAllUsers( type, message, cancellationToken);
        }
    }

}

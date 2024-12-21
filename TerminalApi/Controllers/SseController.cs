using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TerminalApi.Services;

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

        [HttpGet("{clientId}")]
        public async Task SseEndpoint(string clientId, CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";

            // Register the connection
            _connectionManager.AddConnection(clientId, Response);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var resp = new
                    {
                        Name = "Event Test",
                        Message = "lorem epsum"
                    };
                    string jsonData = System.Text.Json.JsonSerializer.Serialize(resp);

                    // Send a named event with the object
                    var eventName = "event";
                    var message = $"event: {eventName}\ndata: {jsonData}\n\n";
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await Response.Body.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);

                    await Response.Body.FlushAsync(cancellationToken);
                    await Task.Delay(1000); // Simulate periodic updates
                }
            }
            finally
            {
                // Clean up when the client disconnects
                _connectionManager.RemoveConnection(clientId);
            }
        }

        [HttpGet("notify/{clientId}")]
        public async Task SendMessageToClient(string clientId, CancellationToken cancellationToken)
        {
            var response = _connectionManager.GetConnection(clientId);
            if (response != null)
            {
               
                var resp = new
                {
                    Name = "Event Test",
                    Message = "lorem epsum"
                };
                var messageBytes = Encoding.UTF8.GetBytes($"{resp}");
                await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpGet("notify-all")]
        public async Task BroadcastMessage(string message, CancellationToken cancellationToken)
        {
            foreach (var response in _connectionManager.GetAllConnections())
            {
                var messageBytes = Encoding.UTF8.GetBytes($"data: {message}\n\n");
                await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        }
    }

}

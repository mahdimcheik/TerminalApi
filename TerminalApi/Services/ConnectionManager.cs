using Hangfire.Storage;
using PuppeteerSharp;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using TerminalApi.Utilities;
namespace TerminalApi.Services
{

    public class SseConnectionManager
    {
        private readonly ConcurrentDictionary<string, HttpResponse> _connections = new();

        public void AddConnection(string clientId, HttpResponse response)
        {
            _connections.TryAdd(clientId, response);
        }

        public void RemoveConnection(string clientId)
        {
            _connections.TryRemove(clientId, out _);
        }

        public HttpResponse? GetConnection(string clientId)
        {
            _connections.TryGetValue(clientId, out var response);
            return response;
        }

        public IEnumerable<HttpResponse> GetAllConnections()
        {
            return _connections.Values;
        }
        public async Task Subscribe( HttpResponse Response,string clientId, CancellationToken cancellationToken, HttpContext context)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            // Register the connection
            AddConnection(clientId, Response);

            try
            {
                while (!context.RequestAborted.IsCancellationRequested)
                {
                    var resp = new
                    {
                        Message = "Connexion établie"
                    };

                    string jsonData = System.Text.Json.JsonSerializer.Serialize(resp);

                    var eventName = EnumEventSSEType.ConnectionSuccess.ToString();
                    var message = $"event: {eventName}\ndata: {jsonData}\n\n";
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await Response.Body.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);

                    await Response.Body.FlushAsync(cancellationToken);
                    await Task.Delay(1000);
                }
            }
            catch
            {
                RemoveConnection(clientId);
            }
        }

        public async Task<bool> NotifyUserById(string clientId, EnumEventSSEType type, object? messageBody, CancellationToken cancellationToken )
        {
            var response = GetConnection(clientId);
            if (response != null)
            {
                string jsonData = System.Text.Json.JsonSerializer.Serialize(messageBody );
                var eventName = type;

                var message = $"event: {eventName}\ndata: {jsonData}\n\n";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task NotifyAllUsers(EnumEventSSEType type,  object? messageBody,CancellationToken cancellationToken )
        {
            var eventName = type;
            foreach (var response in GetAllConnections())
            {
                var message = $"event: {eventName}\ndata: {messageBody}\n\n";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        }
    }

}

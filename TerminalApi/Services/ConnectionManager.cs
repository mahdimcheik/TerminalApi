using System.Collections.Concurrent;
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
    }

}

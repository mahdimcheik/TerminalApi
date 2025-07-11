using System.Collections.Concurrent;

namespace TerminalApi.Services
{
    public class SignalConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _connections = new();

        public void AddConnection(string connectionId, string userName)
        {
            _connections.TryAdd(connectionId, userName);
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public int GetConnectionCount() => _connections.Count;

        public IReadOnlyDictionary<string, string> GetAllConnections() => _connections;
    }
}

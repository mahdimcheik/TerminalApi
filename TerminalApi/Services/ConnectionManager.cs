using System.Collections.Concurrent;
using TerminalApi.Interfaces;

namespace TerminalApi.Services
{
    public class ConnectionManager : ISignalConnectionManager
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

        public void RemoveUserConnections(string userName)
        {
            var connectionsToRemove = _connections
                .Where(kvp => kvp.Value == userName)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var connectionId in connectionsToRemove)
            {
                _connections.TryRemove(connectionId, out _);
            }
        }

        public int GetConnectionCount() => _connections.Count;

        public IReadOnlyDictionary<string, string> GetAllConnections() => _connections;
    }
}
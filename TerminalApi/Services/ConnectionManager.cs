using System.Collections.Concurrent;
using TerminalApi.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace TerminalApi.Services
{
    public class ConnectionManager : ISignalConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _connections = new();
        private readonly IHubContext<ChatHub>? _hubContext;

        public ConnectionManager(IHubContext<ChatHub>? hubContext = null)
        {
            _hubContext = hubContext;
        }

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

        // NEW: Get only active connections
        public async Task<int> GetActiveConnectionCountAsync()
        {
            if (_hubContext == null) return _connections.Count;

            var activeConnections = new List<string>();
            
            foreach (var connection in _connections.Keys)
            {
                try
                {
                    // Try to send a ping to verify connection is alive
                    await _hubContext.Clients.Client(connection).SendAsync("ping");
                    activeConnections.Add(connection);
                }
                catch
                {
                    // Connection is dead, remove it
                    _connections.TryRemove(connection, out _);
                }
            }

            return activeConnections.Count;
        }

        public int GetConnectionCount() => _connections.Count;

        public IReadOnlyDictionary<string, string> GetAllConnections() => _connections;
    }
}
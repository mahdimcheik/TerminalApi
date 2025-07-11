using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    //[Authorize]
    public class SignalRHub : Hub
    {
        private readonly SignalConnectionManager _connectionManager;

        public SignalRHub(SignalConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public override Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var toto = Context;
            var userName = Context.User?.Identity?.Name ?? "Anonymous";
            _connectionManager.AddConnection(connectionId, userName);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _connectionManager.RemoveConnection(connectionId);

            return base.OnDisconnectedAsync(exception);
        }

        // Optionnel : exposer une méthode pour obtenir le nombre de clients connectés
        public Task<int> GetOnlineCount()
        {
            return Task.FromResult(_connectionManager.GetConnectionCount());
        }

        public async Task SendMessage(string user, string message)
        {
            var toto = _connectionManager.GetAllConnections();
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}

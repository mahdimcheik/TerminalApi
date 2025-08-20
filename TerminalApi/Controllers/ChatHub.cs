using TerminalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class ChatHub : Hub
{
    private readonly ConnectionManager _connectionManager;

    public ChatHub(ConnectionManager connectionManager)
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
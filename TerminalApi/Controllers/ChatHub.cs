using TerminalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

[Authorize]
public class ChatHub : Hub
{
    private readonly ConnectionManager _connectionManager;

    public ChatHub(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var connectionId = Context.ConnectionId;
            var userName = Context.User?.FindFirst(ClaimTypes.Email)?.Value ?? 
                          Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? 
                          Context.User?.Identity?.Name ?? 
                          "Anonymous";
            
            _connectionManager.AddConnection(connectionId, userName);
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var connectionId = Context.ConnectionId;
            _connectionManager.RemoveConnection(connectionId);
        }
        catch (Exception ex)
        {
            // Log error if needed
            Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public Task<int> GetOnlineCount()
    {
        return Task.FromResult(_connectionManager.GetConnectionCount());
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
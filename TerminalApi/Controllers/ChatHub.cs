using TerminalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TerminalApi.Models;
using TerminalApi.Contexts;
using TerminalApi.Utilities;

[Authorize]
public class ChatHub : Hub
{
    private readonly ConnectionManager _connectionManager;
    private readonly ApiDefaultContext _context;

    public ChatHub(ConnectionManager connectionManager, ApiDefaultContext context)
    {
        _connectionManager = connectionManager;
        _context = context;
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

    public async Task SendMessageByUserEmail(string email, MessageTypeEnum type, MessageDTO message)
    {
        try
        {
            var userConnections = _connectionManager.GetAllConnections()
                .Where(kvp => kvp.Value == email)
                .Select(kvp => kvp.Key)
                .ToList();

            if (userConnections.Count > 0)
            {
                // Send message to all connections for this user
                await Clients.Clients(userConnections).SendAsync(type.ToString(), message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message to user {email}: {ex.Message}");
        }
    }

    public async Task SendMessageToAll(MessageTypeEnum type, MessageDTO messageDTO)
    {
        await Clients.All.SendAsync(type.ToString(), messageDTO);
    }
}

public enum MessageTypeEnum
{
    Notification,
    Email,
    Chat
}

public class MessageDTO
{
    public UserResponseDTO User { get; set; }
    public string Content { get; set; }
    public MessageTypeEnum Type { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
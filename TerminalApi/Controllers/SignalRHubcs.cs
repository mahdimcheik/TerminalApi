using Microsoft.AspNetCore.SignalR;

namespace TerminalApi;

public class NotificationHub : Hub
{
    public async Task SendNotificationToAll(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}, Exception: {exception?.Message}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGroup(string groupName)
    {
        Console.WriteLine($"Client reconnected: {Context.ConnectionId}");
        await base.Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }
    public async Task LeaveGroup(string groupName)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, string message)
    {
        await base.Clients.Group(groupName).SendAsync(message);
    }
}

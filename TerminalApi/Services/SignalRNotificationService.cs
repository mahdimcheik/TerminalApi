using Microsoft.AspNetCore.SignalR;
using TerminalApi.Models;

namespace TerminalApi.Services
{
    public interface ISignalRNotificationService 
    {
        Task SendMessageByUserEmail(string email, MessageTypeEnum type, object message);
        Task SendMessageToAll(MessageTypeEnum type, object message);
    }

    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<ChatHub> hubContext;
        private readonly ConnectionManager connectionManager;

        public SignalRNotificationService(
            IHubContext<ChatHub> hubContext,
            ConnectionManager connectionManager)
        {
            this.hubContext = hubContext;
            this.connectionManager = connectionManager;
        }

        public async Task SendMessageByUserEmail(string email, MessageTypeEnum type, object message)
        {
            try
            {
                var userConnections = connectionManager.GetAllConnections()
                    .Where(kvp => kvp.Value == email)
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (userConnections.Count > 0)
                {
                    await hubContext.Clients.Clients(userConnections).SendAsync(type.ToString(), message);
                }
                else
                {
                    Console.WriteLine($"User with email {email} is not connected");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to user {email}: {ex.Message}");
            }
        }

        public async Task SendMessageToAll(MessageTypeEnum type, object message)
        {
            try
            {
                await hubContext.Clients.All.SendAsync(type.ToString(), message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to all users: {ex.Message}");
            }
        }
    }
}
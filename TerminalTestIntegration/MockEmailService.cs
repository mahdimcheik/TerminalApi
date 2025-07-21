using TerminalApi.Interfaces;
using TerminalApi.Models;

namespace TerminalTestIntegration
{
    public class MockEmailService : ISendMailService
    {
        public Task SendEmail(Mail mail)
        {
            // Mock implementation - always succeed
            return Task.CompletedTask;
        }

        public Task ContactAdmin(Mail mail)
        {
            // Mock implementation - always succeed
            return Task.CompletedTask;
        }

        public Task SendConfirmationEmail(Mail mail, string link)
        {
            // Mock implementation - always succeed
            return Task.CompletedTask;
        }

        public Task ScheduleSendConfirmationEmail(Mail mail, string link)
        {
            // Mock implementation - always succeed
            return Task.CompletedTask;
        }

        public Task SendResetEmail(Mail mail, string link)
        {
            // Mock implementation - always succeed
            return Task.CompletedTask;
        }

        public Task ScheduleSendResetEmail(Mail mail, string link)
        {
            // Mock implementation - always succeed
            return Task.CompletedTask;
        }
    }
} 
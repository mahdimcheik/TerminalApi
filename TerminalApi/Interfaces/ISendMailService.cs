using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface ISendMailService
    {
        Task SendEmail(Mail mail);
        Task ContactAdmin(Mail mail);
        Task SendConfirmationEmail(Mail mail, string link);
        Task ScheduleSendConfirmationEmail(Mail mail, string link);
        Task SendResetEmail(Mail mail, string link);
        Task ScheduleSendResetEmail(Mail mail, string link);
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TerminalApi.Models.Mail;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class SendMailService
    {
        public async Task SendEmail(Mail mail)
        {
            var smtpClient = new SmtpClient(EnvironmentVariables.SMTP_HostAddress)
            {
                Port = EnvironmentVariables.SMTP_Port,
                Credentials = new NetworkCredential(
                    EnvironmentVariables.SMTP_EmailFrom,
                    EnvironmentVariables.SMTP_Password
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                //From = new MailAddress(
                //    EnvironmentVariables.SMTP_EmailFrom ?? "avrilwithmahdi@gmail.com"
                //),
                From = new MailAddress("ne-pas-repondre@dls.fr"),
                Subject = mail.MailSubject,
                Body = mail.MailBody,
                IsBodyHtml = true, // Set to true if the body contains HTML
            };

            mailMessage.To.Add(mail.MailTo);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendConfirmationEmail(Mail mail)
        {
            var smtpClient = new SmtpClient(EnvironmentVariables.SMTP_HostAddress)
            {
                Port = EnvironmentVariables.SMTP_Port, // Common SMTP port; may vary based on provider
                Credentials = new NetworkCredential(
                    EnvironmentVariables.SMTP_EmailFrom,
                    EnvironmentVariables.SMTP_Password
                ),
                EnableSsl = true // Use SSL for secure connections
            };

            var mailMessage = new MailMessage
            {
                //From = new MailAddress(
                //    EnvironmentVariables.SMTP_EmailFrom ?? "avrilwithmahdi@gmail.com"
                //),
                From = new MailAddress("ne-pas-repondre@dls.fr"),
                Subject = mail.MailSubject,
                Body = mail.MailBody,
                IsBodyHtml = true, // Set to true if the body contains HTML
            };

            mailMessage.To.Add(mail.MailTo);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

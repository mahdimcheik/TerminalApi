using RazorLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TerminalApi.Models.Mail;
using TerminalApi.Services.Templates;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class SendMailService
    {
        private readonly IRazorLightEngine _razorLightEngine;
        private readonly IWebHostEnvironment _env;
        public SendMailService(IWebHostEnvironment env)
        {
            _env = env;
            _razorLightEngine = new RazorLightEngineBuilder()
    .UseFileSystemProject(Path.Combine(_env.WebRootPath, "TemplatesInvoice"))
    .UseMemoryCachingProvider()
    .Build();
        }
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
                From = new MailAddress("ne-pas-repondre@dls.fr"),
                Subject = mail.MailSubject,
                Body = mail.MailBody,
                IsBodyHtml = true, 
            };

            mailMessage.To.Add(mail.MailTo);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendConfirmationEmail(Mail mail, string link)
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
            string templatePath = "ValidationMailTemplate.cshtml"; // Name of your template file
            var model = new ValidationMailTemplateViewModel(link, link);
            string htmlContent = await _razorLightEngine.CompileRenderAsync(templatePath, model);
            //var mailBody = EmailTemplates.ConfirmeMail.Replace(@"{{link}}", link);
            var mailBody = htmlContent;
            var mailMessage = new MailMessage
            {
                From = new MailAddress("ne-pas-repondre@dls.fr"),
                Subject = mail.MailSubject,
                Body = mailBody,
                IsBodyHtml = true, 
            };

            mailMessage.To.Add(mail.MailTo);
            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendResetEmail(Mail mail, string link)
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
            var mailBody = EmailTemplates.ResetPassword.Replace(@"{{link}}", link);

            var mailMessage = new MailMessage
            {                
                From = new MailAddress("ne-pas-repondre@dls.fr"),
                Subject = mail.MailSubject,
                Body = mailBody,
                IsBodyHtml = true, 
            };

            mailMessage.To.Add(mail.MailTo);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

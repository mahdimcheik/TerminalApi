using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Hangfire;
using RazorLight;
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
        /// <summary>
        /// Cette m�thode sert � envoyer un email de confirmation de mail.
        /// C'est une version asynchrone
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        public async Task SendConfirmationEmail(Mail mail, string link)
        {
            mail.MailFrom = EnvironmentVariables.DO_NO_REPLY_MAIL;
            await SendEmail(mail, link, "ValidationEmailTemplate.cshtml");
        }
        /// <summary>
        /// Cette m�thode sert � envoyer un email de confirmation de mail.
        /// En utilisant hangfire, l'envoi du mail sera execut� dnas un backgorund Job.
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        public async Task ScheduleSendConfirmationEmail(Mail mail, string link)
        {
            await Task.Delay(10);
            BackgroundJob.Enqueue(() => SendConfirmationEmail(mail, link));
        }

        public async Task SendResetEmail(Mail mail, string link)
        {
            mail.MailFrom = EnvironmentVariables.DO_NO_REPLY_MAIL;
            await SendEmail(mail, link, "PasswordResetTemplate.cshtml");
        }

        public async Task ScheduleSendResetEmail(Mail mail, string link)
        {
            await Task.Delay(10);
            BackgroundJob.Enqueue(() => SendResetEmail(mail, link));
        }


        /// <summary>
        /// M�thode g�n�rale d'envoi de mail 
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="link"></param>
        /// <param name="templateName"></param>
        /// <returns></returns>
        private async Task SendEmail(Mail mail, string link, string templateName)
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
            string templatePath = templateName;
            var model = new ValidationMailTemplateViewModel(link, link);
            string htmlContent = await _razorLightEngine.CompileRenderAsync(templatePath, model);
            var mailBody = htmlContent;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(mail.MailFrom),
                Subject = mail.MailSubject,
                Body = mailBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(mail.MailTo);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

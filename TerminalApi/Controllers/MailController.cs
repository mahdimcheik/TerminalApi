using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des opérations liées à l'envoi d'emails.
    /// Utilise le service <see cref="SendMailService"/> pour effectuer les envois.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    public class MailController : Controller
    {
        private readonly SendMailService mailService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur <see cref="MailController"/>.
        /// </summary>
        /// <param name="mailService">Service injecté pour gérer l'envoi des emails.</param>
        public MailController(SendMailService mailService, ApiDefaultContext context)
        {
            this.mailService = mailService;
            this.context = context;
        }

        /// <summary>
        /// Envoie un email en utilisant les informations fournies dans l'objet <see cref="Mail"/>.
        /// </summary>
        /// <param name="mail">Objet contenant les informations nécessaires à l'envoi de l'email (destinataire, sujet, corps, expéditeur).</param>
        /// <returns>
        /// Retourne un objet <see cref="ResponseDTO"/> contenant :
        /// - En cas de succès (HTTP 200) : un message de confirmation, le statut et les données de l'email envoyé.
        /// - En cas d'échec (HTTP 400) : un message d'erreur et le statut.
        /// </returns>
        /// <remarks>
        /// Cette méthode utilise le service <see cref="SendMailService"/> pour envoyer l'email.
        /// </remarks>
        /// <response code="200">Email envoyé avec succès.</response>
        /// <response code="400">Échec de l'envoi de l'email.</response>
        [HttpPost("send")]
        public async Task<ActionResult<Mail>> SendMail(Mail mail)
        {
            try
            {
                Console.WriteLine("test");
                await mailService.SendEmail(mail);
                return Ok(
                    new ResponseDTO
                    {
                        Message = "Email est envoyé avec succès",
                        Status = 200,
                        Data = mail
                    }
                );
            }
            catch
            {
                return BadRequest(
                    new ResponseDTO { Message = "Email n'est pas envoyé", Status = 400 }
                );
            }
        }

        [HttpPost("contact-admin")]
        public async Task<ActionResult<Mail>> ContactAdmin([FromBody]Mail mail)
        {
            var sender = CheckUser.GetUserFromClaim(HttpContext.User, context);

            if (sender  is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            try
            {
                mail.Sender = sender;
                await mailService.ContactAdmin(mail );
                return Ok(
                    new ResponseDTO
                    {
                        Message = "Email est envoyé avec succès",
                        Status = 200,
                        Data = mail
                    }
                );
            }
            catch
            {
                return BadRequest(
                    new ResponseDTO { Message = "Email n'est pas envoyé", Status = 400 }
                );
            }
        }
    }
}

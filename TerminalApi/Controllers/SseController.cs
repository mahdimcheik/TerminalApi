/*
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des connexions SSE (Server-Sent Events) et de l'envoi de messages aux utilisateurs connectés.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class SseController : ControllerBase
    {
        private readonly SseService _sseService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur <see cref="SseController"/>.
        /// </summary>
        /// <param name="sseService">Service injecté pour gérer les connexions SSE et les messages.</param>
        public SseController(SseService sseService)
        {
            _sseService = sseService;
        }

        /// <summary>
        /// Établit une connexion SSE pour un utilisateur spécifique.
        /// </summary>
        /// <param name="email">Adresse e-mail de l'utilisateur.</param>
        /// <param name="token">Jeton d'authentification de l'utilisateur.</param>
        /// <param name="cancellationToken">Jeton d'annulation pour gérer les interruptions.</param>
        /// <returns>Ne retourne pas de valeur directement, mais maintient une connexion SSE active.</returns>
        /// <remarks>
        /// - Vérifie l'authenticité de l'utilisateur via le jeton.
        /// - Utilise le service <see cref="SseService"/> pour connecter l'utilisateur.
        /// - Retourne des messages en temps réel via le flux SSE.
        /// - Codes HTTP potentiels : 200 (connexion établie), 400 (paramètres invalides), 500 (erreur interne).
        /// </remarks>
        [HttpGet("{email}/{token}")]
        public async Task Get(string email, string token, CancellationToken cancellationToken)
        {
            if (email is null || email.IsNullOrEmpty() || token.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                var principal = UtilitiesUser.GetPrincipalFromToken(token);

                var userEmail = principal
                    ?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)
                    ?.Value?.ToLower();
                if (userEmail.IsNullOrEmpty() || userEmail != email)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var reader = _sseService.ConnectUser(email);

            await using var writer = new StreamWriter(Response.Body, Encoding.UTF8, leaveOpen: true);

            try
            {
                await foreach (var message in reader.ReadAllAsync(cancellationToken))
                {
                    await writer.WriteAsync($"data: {message}\n\n");
                    await writer.FlushAsync();
                }
            }
            finally
            {
                _sseService.DisconnectUser(email);
            }
        }

        /// <summary>
        /// Envoie un message à un utilisateur spécifique.
        /// </summary>
        /// <param name="email">Adresse e-mail de l'utilisateur cible.</param>
        /// <param name="message">Message à envoyer.</param>
        /// <returns>Un objet <see cref="IActionResult"/> indiquant le succès ou l'échec de l'opération.</returns>
        /// <remarks>
        /// - Utilise le service <see cref="SseService"/> pour envoyer le message.
        /// - Codes HTTP potentiels : 200 (succès), 500 (erreur interne).
        /// </remarks>
        [HttpPost("notify/{email}")]
        public async Task<IActionResult> SendMessage(string email, [FromBody] string message)
        {
            await _sseService.SendMessageToUserAsync(email, message);
            return Ok($"Message envoyé à {email}");
        }

        /// <summary>
        /// Envoie un message à tous les utilisateurs connectés.
        /// </summary>
        /// <param name="message">Message à envoyer.</param>
        /// <returns>Un objet <see cref="IActionResult"/> indiquant le succès ou l'échec de l'opération.</returns>
        /// <remarks>
        /// - Utilise le service <see cref="SseService"/> pour diffuser le message à tous les utilisateurs.
        /// - Codes HTTP potentiels : 200 (succès), 500 (erreur interne).
        /// </remarks>
        [HttpPost("notifyAll")]
        public async Task<IActionResult> SendMessageToall([FromBody] string message)
        {
            await _sseService.SendMessageToAllAsync(message);
            return Ok("Message envoyé");
        }
    }

}
*/
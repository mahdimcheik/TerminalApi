using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des commandes pour les étudiants et les enseignants.
    /// Fournit des points d'entrée pour récupérer, créer et gérer les commandes.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService orderService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur avec les services nécessaires.
        /// </summary>
        /// <param name="orderService">Service pour gérer les opérations liées aux commandes.</param>
        /// <param name="context">Contexte de base de données pour accéder aux données utilisateur.</param>
        public OrderController(OrderService orderService, ApiDefaultContext context)
        {
            this.orderService = orderService;
            this.context = context;
        }

        /// <summary>
        /// Récupère les commandes paginées pour un étudiant connecté.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de filtrage.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les commandes ou un message d'erreur.</returns>
        /// <response code="200">Retourne les commandes paginées.</response>
        /// <response code="400">L'utilisateur n'est pas authentifié ou la demande est invalide.</response>
        [HttpPost("student/all")]
        public async Task<ActionResult<ResponseDTO>> GetOrderForStudentPaginated(
            [FromBody] OrderPagination query
        )
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var response = await orderService.GetOrdersForStudentPaginatedAsync(query, user);

            return Ok(response);
        }

        /// <summary>
        /// Récupère les commandes paginées pour un enseignant.
        /// </summary>
        /// <param name="query">Paramètres de pagination et de filtrage.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les commandes ou un message d'erreur.</returns>
        /// <response code="200">Retourne les commandes paginées.</response>
        /// <response code="404">Aucune commande trouvée.</response>
        [HttpPost("teacher/all")]
        public async Task<ActionResult<ResponseDTO>> GetOrderForTeacherPaginated(
            [FromBody] OrderPagination query
        )
        {
            var orders = await orderService.GetOrdersForTeacherPaginatedAsync(query);
            if (orders is null || orders.Count == 0)
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = orders
                }
            );
        }

        /// <summary>
        /// Récupère une commande spécifique pour un étudiant.
        /// </summary>
        /// <param name="orderId">Identifiant unique de la commande.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant la commande ou un message d'erreur.</returns>
        /// <response code="200">Retourne la commande demandée.</response>
        /// <response code="404">Commande introuvable.</response>
        [HttpGet("student/{orderId}")]
        public async Task<ActionResult<ResponseDTO>> GetOrderByStudent(Guid orderId)
        {
            var order = await orderService.GetOrderByStudentAsync(orderId);
            if (order is null)
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = order
                }
            );
        }

        /// <summary>
        /// Récupère une commande spécifique pour un enseignant.
        /// </summary>
        /// <param name="orderId">Identifiant unique de la commande.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant la commande ou un message d'erreur.</returns>
        /// <response code="200">Retourne la commande demandée.</response>
        /// <response code="404">Commande introuvable.</response>
        [HttpGet("teacher/{orderId}")]
        public async Task<ActionResult<ResponseDTO>> GetOrderByTeacher(Guid orderId)
        {
            var order = await orderService.GetOrderByTeacherAsync(orderId);
            if (order is null)
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = order
                }
            );
        }

        /// <summary>
        /// Récupère ou crée la commande actuelle pour un étudiant connecté.
        /// </summary>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant la commande actuelle ou un message d'erreur.</returns>
        /// <response code="200">Retourne la commande actuelle.</response>
        /// <response code="400">L'utilisateur n'est pas authentifié.</response>
        /// <response code="404">Aucune commande actuelle trouvée.</response>
        [HttpGet("student/current")]
        public async Task<ActionResult<ResponseDTO>> GetCurrentOrder()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var order = await orderService.GetOrCreateCurrentOrderByUserAsync(user);
            if (order is null)
            {
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            }
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = order
                }
            );
        }
    }
}

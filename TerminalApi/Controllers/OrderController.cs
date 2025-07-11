using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contr�leur responsable de la gestion des commandes pour les �tudiants et les enseignants.
    /// Fournit des points d'entr�e pour r�cup�rer, cr�er et g�rer les commandes.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService orderService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contr�leur avec les services n�cessaires.
        /// </summary>
        /// <param name="orderService">Service pour g�rer les op�rations li�es aux commandes.</param>
        /// <param name="context">Contexte de base de donn�es pour acc�der aux donn�es utilisateur.</param>
        public OrderController(OrderService orderService, ApiDefaultContext context)
        {
            this.orderService = orderService;
            this.context = context;
        }

        /// <summary>
        /// R�cup�re les commandes pagin�es pour un �tudiant connect�.
        /// </summary>
        /// <param name="query">Param�tres de pagination et de filtrage.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les commandes ou un message d'erreur.</returns>
        /// <response code="200">Retourne les commandes pagin�es.</response>
        /// <response code="400">L'utilisateur n'est pas authentifi� ou la demande est invalide.</response>
        [HttpPost("student/all")]
        public async Task<ActionResult<ResponseDTO<List<OrderResponseForStudentDTO>>>> GetOrderForStudentPaginated(
            [FromBody] OrderPagination query
        )
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<List<OrderResponseForStudentDTO>> { Status = 40, Message = "Demande refus�e" });
            }
            var response = await orderService.GetOrdersForStudentPaginatedAsync(query, user);

            return Ok(response);
        }

        /// <summary>
        /// R�cup�re les commandes pagin�es pour un enseignant.
        /// </summary>
        /// <param name="query">Param�tres de pagination et de filtrage.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant les commandes ou un message d'erreur.</returns>
        /// <response code="200">Retourne les commandes pagin�es.</response>
        /// <response code="404">Aucune commande trouv�e.</response>
        [HttpPost("teacher/all")]
        public async Task<ActionResult<ResponseDTO<List<OrderResponseForTeacherDTO>>>> GetOrderForTeacherPaginated(
            [FromBody] OrderPagination query
        )
        {
            var orders = await orderService.GetOrdersForTeacherPaginatedAsync(query);
            if (orders is null || orders.Count == 0)
                return NotFound(
                    new ResponseDTO<List<OrderResponseForTeacherDTO>> { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO<List<OrderResponseForTeacherDTO>> {
                    Message = "Demande accept�e",
                    Status = 200,
                    Data = orders
                }
            );
        }

        /// <summary>
        /// R�cup�re une commande sp�cifique pour un �tudiant.
        /// </summary>
        /// <param name="orderId">Identifiant unique de la commande.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant la commande ou un message d'erreur.</returns>
        /// <response code="200">Retourne la commande demand�e.</response>
        /// <response code="404">Commande introuvable.</response>
        [HttpGet("student/{orderId}")]
        public async Task<ActionResult<ResponseDTO<OrderResponseForStudentDTO>>> GetOrderByStudent(Guid orderId)
        {
            var order = await orderService.GetOrderByStudentAsync(orderId);
            if (order is null)
                return NotFound(
                    new ResponseDTO<OrderResponseForStudentDTO> { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO<OrderResponseForStudentDTO> {
                    Message = "Demande accept�e",
                    Status = 200,
                    Data = order
                }
            );
        }

        /// <summary>
        /// R�cup�re une commande sp�cifique pour un enseignant.
        /// </summary>
        /// <param name="orderId">Identifiant unique de la commande.</param>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant la commande ou un message d'erreur.</returns>
        /// <response code="200">Retourne la commande demand�e.</response>
        /// <response code="404">Commande introuvable.</response>
        [HttpGet("teacher/{orderId}")]
        public async Task<ActionResult<ResponseDTO<OrderResponseForTeacherDTO>>> GetOrderByTeacher(Guid orderId)
        {
            var order = await orderService.GetOrderByTeacherAsync(orderId);
            if (order is null)
                return NotFound(
                    new ResponseDTO<OrderResponseForTeacherDTO> { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO<OrderResponseForTeacherDTO> {
                    Message = "Demande accept�e",
                    Status = 200,
                    Data = order
                }
            );
        }

        /// <summary>
        /// R�cup�re ou cr�e la commande actuelle pour un �tudiant connect�.
        /// </summary>
        /// <returns>Un objet <see cref="ResponseDTO"/> contenant la commande actuelle ou un message d'erreur.</returns>
        /// <response code="200">Retourne la commande actuelle.</response>
        /// <response code="400">L'utilisateur n'est pas authentifi�.</response>
        /// <response code="404">Aucune commande actuelle trouv�e.</response>
        [HttpGet("student/current")]
        public async Task<ActionResult<ResponseDTO<OrderResponseForStudentDTO>>> GetCurrentOrder()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<OrderResponseForStudentDTO> { Status = 40, Message = "Demande refus�e" });
            }
            var order = await orderService.GetOrCreateCurrentOrderByUserAsync(user);
            if (order is null)
            {
                return NotFound(
                    new ResponseDTO<OrderResponseForStudentDTO> { Message = "Aucune commande disponible", Status = 404 }
                );
            }
            return Ok(
                new ResponseDTO<OrderResponseForStudentDTO> {
                    Message = "Demande accept�e",
                    Status = 200,
                    Data = order.ToOrderResponseForStudentDTO()
                }
            );
        }
    }
}

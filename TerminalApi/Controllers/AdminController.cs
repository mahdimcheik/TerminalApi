using Microsoft.AspNetCore.Mvc;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{

    /// <summary>
    /// Contrôleur responsable de la gestion des utilisateurs administratifs.
    /// Permet de récupérer les étudiants et de bannir/débannir des utilisateurs.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    //[Authorize(Roles ="Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUsersService userService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur AdminController.
        /// </summary>
        /// <param name="userService">Service injecté pour la gestion des utilisateurs.</param>
        public AdminController(IUsersService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Récupère une liste paginée de tous les étudiants.
        /// </summary>
        /// <param name="query">Objet contenant les paramètres de pagination et de filtrage.</param>
        /// <returns>Une réponse HTTP contenant la liste des étudiants (code 200) ou une erreur (code 500).</returns>
        /// <remarks>
        /// Cette méthode utilise le service <see cref="UsersService"/> pour récupérer les données.
        /// </remarks>
        /// <response code="200">Retourne la liste des étudiants.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpPost("all-students")]
        public async Task<IActionResult> GetAllStudents([FromBody] QueryPagination query)
        {
            var students = await userService.GetAllStudentsDTO(query);
            return Ok(students);
        }

        /// <summary>
        /// Banni ou débanni un utilisateur.
        /// </summary>
        /// <param name="userBanDTO">Objet contenant les informations de l'utilisateur à bannir ou débannir.</param>
        /// <returns>Une réponse HTTP indiquant le succès ou l'échec de l'opération.</returns>
        /// <remarks>
        /// Cette méthode utilise le service <see cref="UsersService"/> pour effectuer l'opération.
        /// </remarks>
        /// <response code="200">L'utilisateur a été banni ou débanni avec succès.</response>
        /// <response code="400">Requête invalide (ex : données manquantes ou incorrectes).</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpPut("ban-unban")]
        public async Task<IActionResult> BanUnbanUser([FromBody] UserBanDTO userBanDTO)
        {
            var result = await userService.BanUnbanUser(userBanDTO);
            return Ok(result);
        }
    }
}

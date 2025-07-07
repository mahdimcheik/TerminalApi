using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur pour gérer les utilisateurs.
    /// </summary>
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        #region Attributes

        /// <summary>
        /// Contexte de la base de données.
        /// </summary>
        private readonly ApiDefaultContext _context;

        /// <summary>
        /// Gestionnaire des utilisateurs.
        /// </summary>
        private readonly UserManager<UserApp> _userManager;

        /// <summary>
        /// Service pour générer des données fictives.
        /// </summary>
        private readonly FakerService fakerService;

        /// <summary>
        /// Service d'authentification.
        /// </summary>
        private readonly AuthService authService;

        /// <summary>
        /// Constructeur du contrôleur des utilisateurs.
        /// </summary>
        /// <param name="context">Contexte de la base de données.</param>
        /// <param name="userManager">Gestionnaire des utilisateurs.</param>
        /// <param name="fakerService">Service pour générer des données fictives.</param>
        /// <param name="authService">Service d'authentification.</param>
        public UsersController(
            ApiDefaultContext context,
            UserManager<UserApp> userManager,
            FakerService fakerService,
            AuthService authService
        )
        {
            this._context = context;
            this._userManager = userManager;
            this.fakerService = fakerService;
            this.authService = authService;
        }

        #endregion

        #region Register update upload

        /// <summary>
        /// Enregistre un nouvel utilisateur.
        /// </summary>
        /// <param name="model">Données de création de l'utilisateur.</param>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [EnableCors]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserCreateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new ResponseDTO<object> { Status = 404, Message = "problème de validation" }
                );
            }

            var response = await authService.Register(model);

            if (response.Status == 200 || response.Status == 201)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        /// <summary>
        /// Met à jour les informations d'un utilisateur.
        /// </summary>
        /// <param name="model">Données de mise à jour de l'utilisateur.</param>
        /// <returns>Résultat de l'opération.</returns>
        [EnableCors]
        [Route("update")]
        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UserUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await authService.Update(model, HttpContext.User);

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Télécharge un avatar (image) pour l'utilisateur.
        /// </summary>
        /// <param name="file">Fichier de l'avatar.</param>
        /// <returns>Résultat de l'opération.</returns>
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            var result = await authService.UploadAvatar(
                file,
                HttpContext.User,
                HttpContext.Request
            );

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        #endregion

        #region POST Login

        /// <summary>
        /// Connecte un utilisateur.
        /// </summary>
        /// <param name="model">Données de connexion de l'utilisateur.</param>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<LoginOutputDTO>>> Login([FromBody] UserLoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    new ResponseDTO<object> {
                        Message = "Connexion échouée",
                        Status = 401,
                        Data = ModelState,
                    }
                );
            var result = await authService.Login(model, HttpContext.Response);

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        #endregion

        #region Confirm account

        /// <summary>
        /// Valide une adresse e-mail.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="confirmationToken">Token de confirmation.</param>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [Route("email-confirmation")]
        [HttpGet]
        public async Task<ActionResult<ResponseDTO<object>>> EmailConfirmation(
            [FromQuery] string userId,
            [FromQuery] string confirmationToken
        )
        {
            var result = await authService.EmailConfirmation(userId, confirmationToken);

            if (result.Status == 200 || result.Status == 201)
            {
                return Redirect(result.Message);
            }
            return BadRequest(result);
        }

        /// <summary>
        /// Récupère un nouveau lien de confirmation.
        /// </summary>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [HttpGet("resend-confirmation-link")]
        public async Task<IActionResult> ResendConfirmationLink()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, _context);

            if (user == null)
                return BadRequest(
                    new ResponseDTO<object> { Message = "Vous n'êtes pas connecté", Status = 401 }
                );

            var res = await authService.ResendConfirmationMail(user);

            return res.Status >= 400 ? BadRequest(res) : Ok(res);
        }

        #endregion

        #region CurrentUser informations

        /// <summary>
        /// Récupère les informations de l'utilisateur connecté.
        /// </summary>
        /// <returns>Informations de l'utilisateur.</returns>
        [HttpGet("my-informations")]
        public async Task<ActionResult<ResponseDTO<object>>> GetMyInformations()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, _context);

            if (user == null)
                return BadRequest(
                    new ResponseDTO<object> { Message = "Vous n'êtes pas connecté", Status = 401 }
                );

            var result = user.ToUserResponseDTO();
            var userRoles = await _userManager.GetRolesAsync(user);

            return Ok(
                new ResponseDTO<object> {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = new
                    {
                        Token = await authService.GenerateAccessTokenAsync(user),
                        User = user.ToUserResponseDTO(userRoles),
                    },
                }
            );
        }

        /// <summary>
        /// Récupère les informations publiques d'un utilisateur.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <returns>Informations publiques de l'utilisateur.</returns>
        [AllowAnonymous]
        [HttpGet("public-informations")]
        public async Task<ActionResult<ResponseDTO<object>>> GetPublicInformations(
            [FromQuery] string userId
        )
        {
            UserApp? user;
            if (userId.ToLower().Trim().IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO<object> { Message = "Aucun profil trouvé", Status = 404 }
                );
            }
            if (userId.ToLower().Trim() == "teacher")
            {
                List<UserApp>? users =
                    (List<UserApp>)await _userManager.GetUsersInRoleAsync("Admin");
                user = users.FirstOrDefault();
            }
            else
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            }

            if (user == null)
                return BadRequest(
                    new ResponseDTO<object> { Message = "Vous n'êtes pas connecté", Status = 401 }
                );

            return Ok(
                new ResponseDTO<object> {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = user.ToUserResponseDTO()
                }
            );
        }

        #endregion

        #region POST AskForPasswordRecoveryMail

        /// <summary>
        /// Demande un e-mail de récupération de mot de passe.
        /// </summary>
        /// <param name="model">Données pour la récupération du mot de passe.</param>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [Route("forgot-password")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<object>>> ForgotPassword(
            [FromBody] ForgotPasswordInput model
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<object> { Message = "Demande refusée", Status = 400 });
            }

            var result = await authService.ForgotPassword(model);

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        #endregion

        #region PasswordChange after recovery

        /// <summary>
        /// Change le mot de passe après une récupération.
        /// </summary>
        /// <param name="model">Données pour changer le mot de passe.</param>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [Route("reset-password")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<object>>> ChangePassword(
            [FromBody] PasswordRecoveryInput model
        )
        {
            if (!ModelState.IsValid || model.Password != model.PasswordConfirmation)
            {
                return BadRequest(new ResponseDTO<object> { Message = "Demande refusée", Status = 400 });
            }

            var result = await authService.ChangePassword(model);

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        #endregion

        #region refresh token

        /// <summary>
        /// Met à jour le token de rafraîchissement.
        /// </summary>
        /// <returns>Résultat de l'opération.</returns>
        [Route("refresh-token")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> UpdateRefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized(
                    new
                    {
                        Message = "Refresh token non-existant",
                        Status = 401
                    });
            }

            var result = await authService.UpdateRefreshToken(refreshToken, HttpContext);

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        #endregion

        /// <summary>
        /// Récupère tous les utilisateurs avec pagination.
        /// </summary>
        /// <param name="first">Index de départ.</param>
        /// <param name="rows">Nombre d'utilisateurs à récupérer.</param>
        /// <returns>Liste des utilisateurs.</returns>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO<IEnumerable<object>>>> GetAll(
            [FromQuery] int first,
            [FromQuery] int rows
        )
        {
            var users = await _context.Users.Skip(first).Take(rows).ToListAsync();
            var totalCount = await _context.Users.CountAsync();
            return Ok(
                new ResponseDTO<object> {
                    Message = "Les utilisateurs",
                    Data = new { users, totalCount },
                    Status = 200
                }
            );
        }

        /// <summary>
        /// Déconnecte l'utilisateur.
        /// </summary>
        /// <returns>Résultat de l'opération.</returns>
        [AllowAnonymous]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            Response.Cookies.Delete("refreshToken");
            return Ok(new
            {
                Message = "Vous êtes déconnecté",
                Status = 200
            });
        }

        #region fixture

        /// <summary>
        /// Génère des utilisateurs fictifs pour les tests.
        /// </summary>
        /// <returns>Liste des utilisateurs générés.</returns>
        [HttpGet("seed")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SeedUsers()
        {
            var usersDTO = fakerService
                .GenerateUserCreateDTO()
                .Generate(500)
                .ToList();
            var users = usersDTO.Select(x => x.ToUser()).ToList();
            for (int i = 0; i < 500; i++)
            {
                IdentityResult result = await _userManager.CreateAsync(
                    users[i],
                    usersDTO[i].Password
                );

                IdentityResult roleResult = await _userManager.AddToRolesAsync(
                    user: users[i],
                    roles: ["Student"]
                );
            }

            _context.SaveChanges();
            return Ok(users.Take(10));
        }

        #endregion
    }

    #region google models
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }

    public class UserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; }

        [JsonPropertyName("picture")]
        public string Picture { get; set; }

        [JsonPropertyName("locale")]
        public string Locale { get; set; }
    }
    #endregion
}

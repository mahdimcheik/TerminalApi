using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Notification;
using TerminalApi.Models.Role;
using TerminalApi.Models.User;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    //[ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, NoStore = false)]
    public class UsersController : ControllerBase
    {
        #region Attributes

        private readonly ApiDefaultContext _context;
        private readonly UserManager<UserApp> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SendMailService mailService;
        private readonly IWebHostEnvironment _env;
        private readonly FakerService fakerService;
        private readonly SignInManager<UserApp> signInManager;
        private readonly NotificationService notificationService;
        private readonly AuthService authService;

        public UsersController(
            ApiDefaultContext context,
            UserManager<UserApp> userManager,
            RoleManager<Role> roleManager,
            SendMailService mailService,
            IWebHostEnvironment env,
            FakerService fakerService,
            SignInManager<UserApp> signInManager,
            NotificationService notificationService,
            AuthService authService
        )
        {
            this._context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;
            this.mailService = mailService;
            this._env = env;
            this.fakerService = fakerService;
            this.signInManager = signInManager;
            this.notificationService = notificationService;
            this.authService = authService;
        }

        #endregion

        #region Register update upload
        /// <summary>
        /// Register a new user
        /// </summary>
        /// <response code="200">The mail address was already used but wasn't confirmed. A mail confirmation request has been sent. The account informations are untouched</response>
        /// <response code="201">The user has been created and a email address confirmation request has been sent</response>
        /// <response code="400">User hasn't been created (Invalid form)</response>
        /// <response code="409">The mail address was already used and was confirmed. Nothing happened</response>
        /// <response code="417">The user is registered but the mail hasn't been sent</response>
        [AllowAnonymous]
        [EnableCors]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserCreateDTO model)
        {
            // Vérifier si le modèle de données reçu est valide
            if (!ModelState.IsValid)
            {
                // Si le modèle n'est pas valide, renvoyer une réponse BadRequest avec le modèle d'état non valide
                return BadRequest(
                    new ResponseDTO { Status = 404, Message = "problème de validation" }
                );
            }

            var response = await authService.Register(model);

            if (response.Status == 200 || response.Status == 201)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

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

        [HttpPost("upload-avatar")]
        [Authorize]
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
        /// Log the user in
        /// </summary>
        /// <remarks>Create an JWT with the user roles as its claims</remarks>
        /// <response code="200">Return the user's informations</response>
        /// <response code="400">User not found or bad login informations</response>
        /// <response code="401">User has not confirmed his mail address</response>
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> Login([FromBody] UserLoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    new ResponseDTO
                    {
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
        /// Validate a mail address
        /// </summary>
        /// <response code="302">User mail has been confirmed. Redirected to the corresponding page</response>
        /// <response code="400">Confirmation unsuccessful</response>
        [AllowAnonymous]
        [Route("email-confirmation")]
        [HttpGet]
        public async Task<ActionResult<ResponseDTO>> EmailConfirmation(
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
        #endregion

        #region CurrentUser informations
        [HttpGet("my-informations")]
        [Authorize]
        public async Task<ActionResult<ResponseDTO>> GetMyInformations()
        {
            Console.WriteLine("Called my infos");
            var user = CheckUser.GetUserFromClaim(HttpContext.User, _context);

            if (user == null)
                return BadRequest(
                    new ResponseDTO { Message = "Vous n'êtes pas connecté", Status = 401 }
                );

            var result = user.ToUserResponseDTO();
            var userRoles = await _userManager.GetRolesAsync(user);

            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = new
                    {
                        //Token = await GenerateAccessTokenAsync(user),
                        User = user.ToUserResponseDTO(userRoles),
                    },
                }
            );
        }

        [AllowAnonymous]
        [HttpGet("public-informations")]
        public async Task<ActionResult<ResponseDTO>> GetPublicInformations(
            [FromQuery] string userId
        )
        {
            UserApp? user;
            if (userId.ToLower().Trim().IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Message = "Aucun profil trouvé", Status = 404 }
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
                    new ResponseDTO { Message = "Vous n'êtes pas connecté", Status = 401 }
                );

            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = user.ToUserResponseDTO()
                }
            );
        }
        #endregion

        #region POST AskForPasswordRecoveryMail
        [AllowAnonymous]
        [Route("forgot-password")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> ForgotPassword(
            [FromBody] ForgotPasswordInput model
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO { Message = "Demande refusée", Status = 400 });
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
        [AllowAnonymous]
        [Route("reset-password")]
        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> ChangePassword(
            [FromBody] PasswordRecoveryInput model
        )
        {
            if (!ModelState.IsValid || model.Password != model.PasswordConfirmation)
            {
                return BadRequest(new ResponseDTO { Message = "Demande refusée", Status = 400 });
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
        [Route("refresh-token")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<RefreshTokenOutput>> UpdateRefreshToken(
            [FromBody] RefreshTokenBodyInput values
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    new ResponseDTO
                    {
                        Data = ModelState,
                        Status = 400,
                        Message = "Demande refusée"
                    }
                );

            var result = await authService.UpdateRefreshToken(values, HttpContext);

            if (result.Status == 200 || result.Status == 201)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        #endregion

        //[HttpGet("all")]
        //[Authorize(Roles = "Admin")]
        //public async Task<ActionResult<ResponseDTO>> getAllUsers(
        //    [FromQuery] int first,
        //    [FromQuery] int rows
        //)
        //{
        //    var users = await _context.Users.Skip(first).Take(rows).ToListAsync();
        //    var totalCount = await _context.Users.CountAsync();
        //    return Ok(
        //        new ResponseDTO
        //        {
        //            Message = "Les utilisateurs",
        //            Data = new { users, totalCount },
        //            Status = 200
        //        }
        //    );
        //}

        #region fixture
        [HttpGet("seed")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SeedUsers()
        {
            var usersDTO = fakerService
                .GenerateUserCreateDTO()
                .Generate(500)
                //.Select(x => x.ToUser())
                .ToList();
            var users = usersDTO.Select(x => x.ToUser()).ToList();
            //_context.Users.AddRange(users);
            for (int i = 0; i < 500; i++)
            {
                IdentityResult result = await _userManager.CreateAsync(
                    users[i],
                    usersDTO[i].Password
                );

                // Tenter d'ajouter l'utilisateur aux rôles spécifiés dans le modèle
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

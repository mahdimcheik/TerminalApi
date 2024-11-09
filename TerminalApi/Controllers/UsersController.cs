using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Role;
using TerminalApi.Models.User;
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

        public UsersController(ApiDefaultContext context, UserManager<UserApp> userManager,
            RoleManager<Role> roleManager
        )
        {
            this._context = context;
            this._userManager = userManager;
            this._roleManager = roleManager;

        }

        #endregion

        #region Post Register
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
                return BadRequest(ModelState);
            }

            // Initialiser la chaîne de réponse
            string ResponseContent = string.Empty;

            // Appeler une fonction asynchrone pour vérifier si l'adresse e-mail est déjà utilisée
            bool isEmailAlreadyUsed = await IsEmailAlreadyUsedAsync(model.Email);

            // Vérifier si l'adresse e-mail est déjà utilisée
            if (isEmailAlreadyUsed)
            {
                // Si l'adresse e-mail est déjà utilisée, mettre à jour la réponse et sauter vers l'étiquette UserAlreadyExisted
                ResponseContent = "L'adresse email est déjà utilisée";
                goto UserAlreadyExisted;
            }
            // Vérifier si le demandeur a le droit de créer un compte pro 
            var seekerRoles = HttpContext.User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToList();


            // Utiliser le mot de passe fourni ou générer un mot de passe aléatoire
            bool isPasswordNull = model.Password == null;

            // Créer un nouvel utilisateur en utilisant les données du modèle et la base de données contextuelle
            UserApp newUser = model.ToUser();

            // Obtenir la date actuelle
            DateTime date = DateTime.Now;

            // Tenter de créer un nouvel utilisateur avec le gestionnaire d'utilisateurs
            IdentityResult result = await _userManager.CreateAsync(newUser, model.Password);

            // Tenter d'ajouter l'utilisateur aux rôles spécifiés dans le modèle
            IdentityResult roleResult = await _userManager.AddToRolesAsync(user: newUser, roles: ["Client"]);

            // Vérifier si l'ajout aux rôles a réussi

            //if (roleResult.Succeeded)
            //{
            //    // Si l'ajout aux rôles a réussi, ajouter les revendications correspondantes aux rôles
            //    await _userManager.AddClaimsAsync(user: newUser, claims: model.Roles.Select(x => x != null ? new Claim(type: ClaimTypes.Role, value: x) : null));

            //    // Enregistrer les changements dans le contexte de base de données
            //    await _context.SaveChangesAsync();
            //}

            // Vérifier si la création de l'utilisateur a échoué
            if (!result.Succeeded)
            {
                // Si la création a échoué, ajouter les erreurs au modèle d'état pour retourner une réponse BadRequest
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(key: string.Empty, errorMessage: error.Description);
                }

                // Retourner une réponse BadRequest avec le modèle d'état contenant les erreurs
                return BadRequest(ModelState);
            }

            // Si tout s'est bien déroulé, enregistrer les changements dans le contexte de base de données
            await _context.SaveChangesAsync();

            try
            {


                // Retourne une réponse avec le statut déterminé, l'identifiant de l'utilisateur, le message de réponse et le statut complet
                return Ok(new { UserId = newUser.Id, Message = ResponseContent });
            }
            catch (Exception e)
            {
                // En cas d'exception, afficher la trace et retourner une réponse avec le statut approprié
                Console.WriteLine(e);
                throw new Exception("An error occurred during user creation.", e);
            }

        UserAlreadyExisted:
            return Conflict(ResponseContent);
        }

        private async Task<bool> IsEmailAlreadyUsedAsync(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            return existingUser != null;
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
                return BadRequest(new ResponseDTO { Message = "Connexion échouée", Status = 401, Data = ModelState });


            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return NotFound(new ResponseDTO { Message = "L'utilisateur n'existe pas ", Status = 404 });

            var result = await _userManager.CheckPasswordAsync(user: user, password: model.Password);
            if (!_userManager.CheckPasswordAsync(user: user, password: model.Password).Result)
                return BadRequest(new ResponseDTO { Message = "Connexion échouée", Status = 401 });

            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //    return Unauthorized();

            if (user.RefreshToken == null) // a new refresh token has to be saved
            {
                user.RefreshToken = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();
            }

            HttpContext.Response.Headers.Add(key: "Access-Control-Allow-Credentials", value: "true");
            var userRoles = await _userManager.GetRolesAsync(user);

            return Ok(new ResponseDTO { Message = "Connexion réussite", Status = 200, Data = new { Token = await GenerateAccessTokenAsync(user), User = user.ToUserResponseDTO(userRoles) } });
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private async Task<string> GenerateAccessTokenAsync(UserApp user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY));
            var credentials = new SigningCredentials(key: securityKey, algorithm: SecurityAlgorithms.HmacSha256);

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(type: ClaimTypes.Email, value: user.Email),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(type: ClaimTypes.Role, value: userRole));
            }

            var token = new JwtSecurityToken(
                issuer: EnvironmentVariables.API_BASE_URL,
                audience: EnvironmentVariables.USER_BASE_URL,
                claims: authClaims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            _context.Entry(user).State = EntityState.Modified;

            return new JwtSecurityTokenHandler().WriteToken(token);
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
        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> EmailConfirmation([FromBody] ConfirmAccountInput input)
        {
            UserApp user = await _userManager.FindByIdAsync(input.UserId);

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, input.ConfirmationToken);

            if (result.Succeeded)
            {
                return Ok(new ResponseDTO { Message = "Validation réussite", Status = 200 });

            }

            return BadRequest(new ResponseDTO { Message = "Validation échouée", Status = 400 });
        }
        #endregion

        private async Task<string?> GenerateAccountConfirmationLink(UserApp user)
        {
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            confirmationToken = HttpUtility.UrlEncode(confirmationToken);

            var confirmationLink = EnvironmentVariables.USER_BASE_URL + "/account-confirmation?userId=" + user.Id + "&confirmationToken=" + confirmationToken;

            return confirmationLink;
        }

        #region CurrentUser informations
        [HttpGet("my-informations")]
        public async Task<ActionResult<ResponseDTO>> GetMyInformations()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, _context);

            if (user == null) return BadRequest(new ResponseDTO { Message = "Demande réfusée", Status = 401 });

            var result = user.ToUserResponseDTO();
            var userRoles = await _userManager.GetRolesAsync(user);


            return Ok(new ResponseDTO { Message = "Demande acceptée", Status = 200, Data = new { Token = await GenerateAccessTokenAsync(user), User = user.ToUserResponseDTO(userRoles) } });
        }
        #endregion


    }
}

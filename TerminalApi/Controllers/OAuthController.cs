//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using System.Text.Json;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using TerminalApi.Contexts;
//using TerminalApi.Models;
//using TerminalApi.Utilities;

//namespace TerminalApi.Controllers
//{
//    /// <summary>
//    /// Contrôleur responsable de la gestion de l'authentification OAuth, notamment via Google.
//    /// </summary>
//    [Route("[controller]")]
//    [ApiController]
//    public class OAuthController : ControllerBase
//    {
//        private readonly UserManager<UserApp> userManager;
//        private readonly ApiDefaultContext context;

//        /// <summary>
//        /// Initialise une nouvelle instance du contrôleur OAuth avec les services injectés.
//        /// </summary>
//        /// <param name="userManager">Service de gestion des utilisateurs.</param>
//        /// <param name="context">Contexte de base de données pour les opérations liées aux utilisateurs.</param>
//        public OAuthController(UserManager<UserApp> userManager, ApiDefaultContext context)
//        {
//            this.userManager = userManager;
//            this.context = context;
//        }

//        #region google oAuth

//        /// <summary>
//        /// Callback pour le processus d'authentification Google.
//        /// Échange le code d'autorisation pour un jeton d'accès, récupère les informations utilisateur et génère un jeton JWT.
//        /// </summary>
//        /// <param name="code">Code d'autorisation retourné par Google.</param>
//        /// <param name="state">État optionnel pour la validation de la requête.</param>
//        /// <returns>
//        /// Un objet contenant le jeton JWT si l'authentification réussit.
//        /// Codes HTTP possibles :
//        /// - 200 : Authentification réussie.
//        /// - 400 : Erreur dans le processus d'authentification.
//        /// </returns>
//        [AllowAnonymous]
//        [HttpGet("/google-callback")]
//        public async Task<IActionResult> GoogleCallback(
//            [FromQuery] string code,
//            [FromQuery] string state
//        )
//        {
//            if (string.IsNullOrEmpty(code))
//            {
//                return BadRequest("Réponse d'authentification Google invalide.");
//            }

//            var tokenResponse = await ExchangeCodeForTokenAsync(code);
//            if (tokenResponse == null)
//            {
//                return BadRequest("Échec de l'échange du code pour un jeton.");
//            }

//            var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
//            if (userInfo == null)
//            {
//                return BadRequest("Échec de la récupération des informations utilisateur.");
//            }

//            var user = await userManager.FindByEmailAsync(userInfo.Email);
//            if (user == null)
//            {
//                user = new UserApp { UserName = userInfo.Email, Email = userInfo.Email };
//                await userManager.CreateAsync(user);
//                await userManager.AddLoginAsync(
//                    user,
//                    new UserLoginInfo("Google", userInfo.Id, "Google")
//                );
//            }

//            var token = await GenerateAccessTokenAsync(user);
//            return Ok(new { Token = token });
//        }

//        /// <summary>
//        /// Échange un code d'autorisation Google pour un jeton d'accès.
//        /// </summary>
//        /// <param name="code">Code d'autorisation retourné par Google.</param>
//        /// <returns>
//        /// Un objet <see cref="TokenResponse"/> contenant les jetons d'accès et d'actualisation.
//        /// Codes HTTP possibles :
//        /// - 200 : Échange réussi.
//        /// - 400 : Échec de l'échange.
//        /// </returns>
//        private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
//        {
//            var client = new HttpClient();
//            var request = new HttpRequestMessage(
//                HttpMethod.Post,
//                "https://oauth2.googleapis.com/token"
//            )
//            {
//                Content = new FormUrlEncodedContent(
//                    new Dictionary<string, string>
//                    {
//                            { "code", code },
//                            { "client_id", EnvironmentVariables.ID_CLIENT_GOOGLE },
//                            { "client_secret", EnvironmentVariables.SECRET_CLIENT_GOOGLE },
//                            { "redirect_uri", $"{Request.Scheme}://{Request.Host}/google-callback" },
//                            { "grant_type", "authorization_code" }
//                    }
//                )
//            };

//            var response = await client.SendAsync(request);
//            if (!response.IsSuccessStatusCode)
//            {
//                return null;
//            }

//            var responseContent = await response.Content.ReadAsStringAsync();
//            return JsonSerializer.Deserialize<TokenResponse>(responseContent);
//        }

//        /// <summary>
//        /// Récupère les informations utilisateur à partir d'un jeton d'accès Google.
//        /// </summary>
//        /// <param name="accessToken">Jeton d'accès Google.</param>
//        /// <returns>
//        /// Un objet <see cref="UserInfo"/> contenant les informations utilisateur.
//        /// Codes HTTP possibles :
//        /// - 200 : Récupération réussie.
//        /// - 400 : Échec de la récupération.
//        /// </returns>
//        private async Task<UserInfo> GetUserInfoAsync(string accessToken)
//        {
//            var client = new HttpClient();
//            var response = await client.GetAsync(
//                $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}"
//            );
//            if (!response.IsSuccessStatusCode)
//            {
//                return null;
//            }

//            var responseContent = await response.Content.ReadAsStringAsync();
//            return JsonSerializer.Deserialize<UserInfo>(responseContent);
//        }

//        /// <summary>
//        /// Redirige l'utilisateur vers la page de connexion Google.
//        /// </summary>
//        /// <returns>
//        /// Une redirection vers Google pour l'authentification.
//        /// Codes HTTP possibles :
//        /// - 302 : Redirection réussie.
//        /// </returns>
//        [AllowAnonymous]
//        [HttpGet("google-login")]
//        public IActionResult GoogleLogin()
//        {
//            var redirectUrl = Url.Action("GoogleCallback", "Users");
//            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
//            return Challenge(properties, "Google");
//        }

//        /// <summary>
//        /// Génère un jeton JWT pour un utilisateur authentifié.
//        /// </summary>
//        /// <param name="user">Utilisateur pour lequel le jeton est généré.</param>
//        /// <returns>
//        /// Une chaîne représentant le jeton JWT.
//        /// Codes HTTP possibles :
//        /// - 200 : Jeton généré avec succès.
//        /// - 500 : Erreur interne lors de la génération du jeton.
//        /// </returns>
//        private async Task<string> GenerateAccessTokenAsync(UserApp user)
//        {
//            var securityKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
//            );
//            var credentials = new SigningCredentials(
//                key: securityKey,
//                algorithm: SecurityAlgorithms.HmacSha256
//            );

//            var userRoles = await userManager.GetRolesAsync(user);

//            var authClaims = new List<Claim>
//                {
//                    new Claim(type: ClaimTypes.Email, value: user.Email),
//                };

//            foreach (var userRole in userRoles)
//            {
//                authClaims.Add(new Claim(type: ClaimTypes.Role, value: userRole));
//            }

//            var token = new JwtSecurityToken(
//                issuer: EnvironmentVariables.API_BACK_URL,
//                audience: EnvironmentVariables.API_BACK_URL,
//                claims: authClaims,
//                expires: DateTime.Now.AddMinutes(2),
//                signingCredentials: credentials
//            );

//            context.Entry(user).State = EntityState.Modified;

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

//        #endregion
//    }
//}

﻿using System.IdentityModel.Tokens.Jwt;
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

        public UsersController(
            ApiDefaultContext context,
            UserManager<UserApp> userManager,
            RoleManager<Role> roleManager,
            SendMailService mailService,
            IWebHostEnvironment env,
            FakerService fakerService,
            SignInManager<UserApp> signInManager,
            NotificationService notificationService
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
                return BadRequest(
                    new ResponseDTO { Status = 404, Message = "problème de validation" }
                );
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
                return Conflict(
                    new ResponseDTO { Status = 404, Message = "\"L'email est déjà utilisé\"" }
                );
            }

            // Utiliser le mot de passe fourni ou générer un mot de passe aléatoire
            bool isPasswordNull = model.Password == null;

            // Créer un nouvel utilisateur en utilisant les données du modèle et la base de données contextuelle
            UserApp newUser = model.ToUser();
            newUser.CreatedAt = DateTime.Now;
            newUser.LastModifiedAt = DateTime.Now;
            newUser.ImgUrl =
                @"https://upload.wikimedia.org/wikipedia/commons/thumb/1/12/User_icon_2.svg/1200px-User_icon_2.svg.png";

            // Obtenir la date actuelle
            DateTimeOffset date = DateTimeOffset.Now;

            // Tenter de créer un nouvel utilisateur avec le gestionnaire d'utilisateurs
            IdentityResult result = await _userManager.CreateAsync(newUser, model.Password);

            // Tenter d'ajouter l'utilisateur aux rôles spécifiés dans le modèle
            IdentityResult roleResult = await _userManager.AddToRolesAsync(
                user: newUser,
                roles: ["Student"]
            );

            // Vérifier si la création de l'utilisateur a échoué
            if (!result.Succeeded)
            {
                // Si la création a échoué, ajouter les erreurs au modèle d'état pour retourner une réponse BadRequest
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(key: string.Empty, errorMessage: error.Description);
                }

                // Retourner une réponse BadRequest avec le modèle d'état contenant les erreurs
                return BadRequest(
                    new ResponseDTO
                    {
                        Message = "Création échouée",
                        Status = 401,
                        Data = ModelState,
                    }
                );
            }

            // Si tout s'est bien déroulé, enregistrer les changements dans le contexte de base de données
            await _context.SaveChangesAsync();

            try
            {
                var confirmationLink = await GenerateAccountConfirmationLink(newUser);
                await mailService.ScheduleSendConfirmationEmail(
                    new Models.Mail.Mail
                    {
                        MailBody = confirmationLink,
                        MailSubject = "Mail de confirmation",
                        MailTo = newUser.Email ?? "mahdi.mcheik@hotmail.fr",
                    },
                    confirmationLink ?? ""
                );

                // Retourne une réponse avec le statut déterminé, l'identifiant de l'utilisateur, le message de réponse et le statut complet
                return Ok(
                    new ResponseDTO
                    {
                        Message = "Profil créé",
                        Status = 200,
                        Data = newUser.ToUserResponseDTO(),
                    }
                );
                ;
            }
            catch (Exception e)
            {
                // En cas d'exception, afficher la trace et retourner une réponse avec le statut approprié
                Console.WriteLine(e);
                return Conflict(
                    new ResponseDTO { Status = 400, Message = "Le compte n'est pas créé!!!" }
                );
            }

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

            var user = CheckUser.GetUserFromClaim(HttpContext.User, _context);

            if (user is null)
            {
                return BadRequest(
                    new ResponseDTO
                    {
                        Status = 404,
                        Message = "Le compte n'existe pas ou ne correspond pas",
                    }
                );
            }

            model.ToUser(user);
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                await notificationService.AddNotification(
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = user.Id,
                        SenderId = user.Id,
                        Type = EnumNotificationType.AccountUpdated
                    }
                );
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new ResponseDTO { Status = 401, Message = ex.Message });
            }

            return Ok(
                new ResponseDTO
                {
                    Message = "Profil mis à jour",
                    Status = 200,
                    Data = user.ToUserResponseDTO(),
                }
            );
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
                return BadRequest(
                    new ResponseDTO
                    {
                        Message = "Connexion échouée",
                        Status = 401,
                        Data = ModelState,
                    }
                );

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return NotFound(
                    new ResponseDTO { Message = "L'utilisateur n'existe pas ", Status = 404 }
                );

            var result = await _userManager.CheckPasswordAsync(
                user: user,
                password: model.Password
            );
            if (!_userManager.CheckPasswordAsync(user: user, password: model.Password).Result)
                return BadRequest(new ResponseDTO { Message = "Connexion échouée", Status = 401 });

            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //    return Unauthorized();

            if (user.RefreshToken == null) // a new refresh token has to be saved
            {
                user.RefreshToken = Guid.NewGuid().ToString();
            }

            user.LastLogginAt = DateTime.Now;
            await _context.SaveChangesAsync();
            // to allow cookies sent from the front end
            HttpContext.Response.Headers.Add(
                key: "Access-Control-Allow-Credentials",
                value: "true"
            );
            var userRoles = await _userManager.GetRolesAsync(user);

            return Ok(
                new ResponseDTO
                {
                    Message = "Connexion réussite",
                    Status = 200,
                    Data = new
                    {
                        Token = await GenerateAccessTokenAsync(user),
                        refreshToken = user.RefreshToken,
                        User = user.ToUserResponseDTO(userRoles),
                    },
                }
            );
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
                ),
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out SecurityToken securityToken
            );
            if (
                securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private async Task<string> GenerateAccessTokenAsync(UserApp user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
            );
            var credentials = new SigningCredentials(
                key: securityKey,
                algorithm: SecurityAlgorithms.HmacSha256
            );

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
                issuer: EnvironmentVariables.API_BACK_URL,
                audience: EnvironmentVariables.API_BACK_URL,
                claims: authClaims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

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
        [HttpGet]
        public async Task<ActionResult<ResponseDTO>> EmailConfirmation(
            [FromQuery] string userId,
            [FromQuery] string confirmationToken
        )
        {
            UserApp? user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Message = "Validation échouée", Status = 400 });
            }

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, confirmationToken);

            if (result.Succeeded)
            {
                return Redirect(
                    $"{EnvironmentVariables.API_FRONT_URL}/auth/email-confirmation-success"
                );
            }

            return BadRequest(new ResponseDTO { Message = "Validation échouée", Status = 400 });
        }
        #endregion

        private async Task<string?> GenerateAccountConfirmationLink(UserApp user)
        {
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            confirmationToken = HttpUtility.UrlEncode(confirmationToken);

            var confirmationLink =
                EnvironmentVariables.API_BACK_URL
                + "/users/email-confirmation?userId="
                + user.Id
                + "&confirmationToken="
                + confirmationToken;

            return confirmationLink;
        }

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
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    try
                    {
                        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                        resetToken = HttpUtility.UrlEncode(resetToken);

                        var resetLink =
                            EnvironmentVariables.API_FRONT_URL
                            + "/auth/reset-password?userId="
                            + user.Id
                            + "&resetToken="
                            + resetToken;

                        // Tentative d'envoi de l'e-mail pour la regénération du mot de passe
                        await mailService.ScheduleSendResetEmail(
                            new Models.Mail.Mail
                            {
                                MailSubject = "Mail de réinitialisation",
                                MailTo = user.Email,
                            },
                            resetLink
                        );
                        await notificationService.AddNotification(
                            new Notification
                            {
                                Id = Guid.NewGuid(),
                                RecipientId = user.Id,
                                SenderId = user.Id,
                                Type = EnumNotificationType.PasswordResetDemandAccepted
                            }
                        );

                        return Ok(
                            new ResponseDTO
                            {
                                Message =
                                    "Un email de réinitialisation vient d'être envoyé à cette adresse "
                                    + user.Email,
                                Status = 200,
                                Data = new
                                {
                                    resetToken,
                                    Email = user.Email,
                                    Id = user.Id,
                                },
                            }
                        );
                    }
                    catch
                    {
                        return BadRequest(
                            new ResponseDTO
                            {
                                Message = "Erreur de réinitialisation, réessayez plus tard ",
                                Status = 400,
                            }
                        );
                    }
                }
            }
            return BadRequest(
                new ResponseDTO
                {
                    Message = "Erreur de réinitialisation, réessayez plus tard ",
                    Status = 400,
                }
            );
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
            if (ModelState.IsValid && model.Password == model.PasswordConfirmation)
            {
                UserApp? user = await _userManager.FindByIdAsync(model.UserId);
                if (user is null)
                    return BadRequest(
                        new ResponseDTO { Message = "L'utilisateur n'existe pas", Status = 404 }
                    );
                // var decodedToken = HttpUtility.UrlDecode(model.ResetToken);
                user.RefreshToken = Guid.NewGuid().ToString();
                IdentityResult result = await _userManager.ResetPasswordAsync(
                    user: user,
                    token: model.ResetToken,
                    newPassword: model.Password
                );

                if (result.Succeeded)
                {
                    await _context.SaveChangesAsync();
                    await notificationService.AddNotification(
                        new Notification
                        {
                            Id = Guid.NewGuid(),
                            RecipientId = user.Id,
                            SenderId = user.Id,
                            Type = EnumNotificationType.PasswordChanged
                        }
                    );
                    return Ok(
                        new ResponseDTO
                        {
                            Message = "Mot de passe vient d'être modifié",
                            Status = 201,
                        }
                    );
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(
                            key: string.Empty,
                            errorMessage: error.Description
                        );
                    }
                }
            }
            return BadRequest(
                new ResponseDTO
                {
                    Message = "Problème de validation, votre token est valid ?",
                    Status = 404,
                    Data = ModelState,
                }
            );
        }
        #endregion
        /*
        #region POST Change password
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("/change-password")]
        [HttpPost]
        public IActionResult ChangePassword([FromBody] ChangePasswordInput input)
        {
            string? userEmail = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            User user = _userManager.FindByEmailAsync(userEmail).Result;
            if (user == null)
                return StatusCode(StatusCodes.Status400BadRequest);

            if (input == null)
            {
                return BadRequest();
            }

            if (input.NewPassword != input.NewPasswordConfirmation) return BadRequest("La confirmation doit être identique à l'original");
            if (input.OldPassword == input.NewPassword) return BadRequest("Le nouveau mot de passe doit être différent de l'ancien");

            IdentityResult result = _userManager.ChangePasswordAsync(user: user, currentPassword: input.OldPassword, newPassword: input.NewPassword).Result;

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest("Echec du changement de mot de passe");
        }
        #endregion
        */
        [HttpPost("upload-avatar")]
        [Authorize]
        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("No file uploaded.");
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, _context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            //verifier si le type est image
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (
                !allowedMimeTypes.Contains(file.ContentType)
                || !allowedExtensions.Contains(fileExtension)
            )
            {
                return BadRequest(
                    new ResponseDTO
                    {
                        Status = 404,
                        Message = "le type du ficheir n'est pas autorisé'"
                    }
                );
            }

            // supprimer l' ancien fichier s' il existe
            var oldFilenameFromDB = Path.GetFileName(user.ImgUrl);
            if (user.ImgUrl is not null && !oldFilenameFromDB.IsNullOrEmpty())
            {
                var uploadFolder = Path.Combine(_env.WebRootPath, "images");

                // Define the file name using the user's Guid

                var fullFileName = Path.Combine(uploadFolder, oldFilenameFromDB);
                if (System.IO.File.Exists(fullFileName))
                {
                    System.IO.File.Delete(fullFileName);
                }
            }
            //


            string fileName = Guid.NewGuid() + "_avatar" + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_env.WebRootPath, "images", fileName); // wwwroot + images + filename ???

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            user.ImgUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new { fileName, url });
        }

        [Route("refresh-token")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<RefreshTokenOutput>> UpdateRefreshToken(
            [FromBody] RefreshTokenBodyInput values
        )
        {
            Console.WriteLine("refresh token ");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? userEmail = HttpContext
                .User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)
                ?.Value?.ToLower();
            if (userEmail == null)
            {
                var principal = GetPrincipalFromExpiredToken(values.Token);
                userEmail = principal
                    ?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)
                    ?.Value?.ToLower();
            }

            if (userEmail == null)
                return Unauthorized();

            UserApp? user = _context.Users.FirstOrDefault(x => x.Email.ToLower() == userEmail);

            if (user == null)
                return NotFound();

            if (values.RefreshToken == user.RefreshToken)
            {
                HttpContext.Response.Headers.Add(
                    key: "Access-Control-Allow-Credentials",
                    value: "true"
                );
                //HttpContext.Response.Headers.Add(key: "Authorization", value: accessToken);

                return Ok(
                    new ResponseDTO
                    {
                        Message = "Autorisation renouvelée",
                        Data = new RefreshTokenOutput(user, await GenerateAccessTokenAsync(user)),
                        Status = 200
                    }
                );
            }

            return Redirect("/login"); // todo test ?
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ResponseDTO>> getAllUsers(
            [FromQuery] int first,
            [FromQuery] int rows
        )
        {
            var users = await _context.Users.Skip(first).Take(rows).ToListAsync();
            var totalCount = await _context.Users.CountAsync();
            return Ok(
                new ResponseDTO
                {
                    Message = "Les utilisateurs",
                    Data = new { users, totalCount },
                    Status = 200
                }
            );
        }

        [HttpGet("seed")]
        [AllowAnonymous]
        public ActionResult SeedUsers()
        {
            var users = fakerService
                .GenerateUserCreateDTO()
                .Generate(500)
                .Select(x => x.ToUser())
                .ToList();
            _context.Users.AddRange(users);
            _context.SaveChanges();
            return Ok(users.Take(10));
        }

        [AllowAnonymous]
        [HttpGet("/google-callback")]
        public async Task<IActionResult> GoogleCallback(
            [FromQuery] string code,
            [FromQuery] string state
        )
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Invalid Google authentication response.");
            }

            var tokenResponse = await ExchangeCodeForTokenAsync(code);
            if (tokenResponse == null)
            {
                return BadRequest("Failed to exchange code for token.");
            }

            var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
            if (userInfo == null)
            {
                return BadRequest("Failed to retrieve user information.");
            }

            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            if (user == null)
            {
                user = new UserApp { UserName = userInfo.Email, Email = userInfo.Email };
                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo("Google", userInfo.Id, "Google")
                );
            }

            var token = await GenerateAccessTokenAsync(user);
            return Ok(new { Token = token });
        }

        private async Task<TokenResponse> ExchangeCodeForTokenAsync(string code)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://oauth2.googleapis.com/token"
            )
            {
                Content = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "code", code },
                        { "client_id", EnvironmentVariables.ID_CLIENT_GOOGLE },
                        { "client_secret", EnvironmentVariables.SECRET_CLIENT_GOOGLE },
                        { "redirect_uri", $"{Request.Scheme}://{Request.Host}/google-callback" },
                        { "grant_type", "authorization_code" }
                    }
                )
            };

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TokenResponse>(responseContent);
        }

        private async Task<UserInfo> GetUserInfoAsync(string accessToken)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(
                $"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}"
            );
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserInfo>(responseContent);
        }

        [AllowAnonymous]
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Users");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }
    }

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
}

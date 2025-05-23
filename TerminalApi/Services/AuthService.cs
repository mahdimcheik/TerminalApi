﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.Notification;
using TerminalApi.Models.Role;
using TerminalApi.Models.User;
using TerminalApi.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TerminalApi.Services
{
    public class AuthService
    {
        private readonly ApiDefaultContext context;
        private readonly UserManager<UserApp> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly SignInManager<UserApp> signInManager;
        private readonly SendMailService mailService;
        private readonly NotificationService notificationService;
        private readonly IWebHostEnvironment _env;

        public AuthService(
            ApiDefaultContext context,
            UserManager<UserApp> userManager,
            RoleManager<Role> roleManager,
            SignInManager<UserApp> signInManager,
            SendMailService mailService,
            NotificationService notificationService,
            IWebHostEnvironment env
        )
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.mailService = mailService;
            this.notificationService = notificationService;
            this._env = env;
        }

        public async Task<ResponseDTO> Register(UserCreateDTO model)
        {
            bool isEmailAlreadyUsed = await IsEmailAlreadyUsedAsync(model.Email);

            // Vérifier si l'adresse e-mail est déjà utilisée
            if (isEmailAlreadyUsed)
            {
                // Si l'adresse e-mail est déjà utilisée, mettre à jour la réponse et sauter vers l'étiquette UserAlreadyExisted

                return new ResponseDTO { Status = 404, Message = "\"L'email est déjà utilisé\"" };
            }
            // Créer un nouvel utilisateur en utilisant les données du modèle et la base de données contextuelle
            UserApp newUser = model.ToUser();
            newUser.CreatedAt = DateTime.Now;
            newUser.LastModifiedAt = DateTime.Now;
            newUser.ImgUrl =
                @"https://upload.wikimedia.org/wikipedia/commons/thumb/1/12/User_icon_2.svg/1200px-User_icon_2.svg.png";

            // Obtenir la date actuelle
            DateTimeOffset date = DateTimeOffset.Now;

            // Tenter de créer un nouvel utilisateur avec le gestionnaire d'utilisateurs
            IdentityResult result = await userManager.CreateAsync(newUser, model.Password);

            // Tenter d'ajouter l'utilisateur aux rôles spécifiés dans le modèle
            IdentityResult roleResult = await userManager.AddToRolesAsync(
                user: newUser,
                roles: ["Student"]
            );

            // Vérifier si la création de l'utilisateur a échoué
            if (!result.Succeeded)
            {
                // Si la création a échoué, ajouter les erreurs au modèle d'état pour retourner une réponse BadRequest
                var errors = Enumerable.Empty<string>();
                foreach (var error in result.Errors)
                {
                    errors.Append(error.Description);
                }

                // Retourner une réponse BadRequest avec le modèle d'état contenant les erreurs
                return new ResponseDTO
                {
                    Message = "Création échouée",
                    Status = 401,
                    Data = errors,
                };
            }

            // Si tout s'est bien déroulé, enregistrer les changements dans le contexte de base de données
            await context.SaveChangesAsync();

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
                return new ResponseDTO
                {
                    Message = "Profil créé",
                    Status = 201,
                    Data = newUser.ToUserResponseDTO(),
                };
                ;
            }
            catch (Exception e)
            {
                // En cas d'exception, afficher la trace et retourner une réponse avec le statut approprié
                Console.WriteLine(e);
                return new ResponseDTO { Status = 400, Message = "Le compte n'est pas créé!!!" };
            }
        }

        public async Task<ResponseDTO> Update(UserUpdateDTO model, ClaimsPrincipal UserPrincipal)
        {
            var user = CheckUser.GetUserFromClaim(UserPrincipal, context);
            if (user is null)
            {
                return new ResponseDTO
                {
                    Status = 404,
                    Message = "Le compte n'existe pas ou ne correspond pas",
                };
            }
            model.ToUser(user);
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.SaveChangesAsync();
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
                return new ResponseDTO { Status = 401, Message = ex.Message };
            }

            return new ResponseDTO
            {
                Message = "Profil mis à jour",
                Status = 200,
                Data = user.ToUserResponseDTO(),
            };
        }

        public async Task<ResponseDTO> EmailConfirmation(string userId, string confirmationToken)
        {
            UserApp? user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return new ResponseDTO { Message = "Validation échouée", Status = 400 };
            }

            IdentityResult result = await userManager.ConfirmEmailAsync(user, confirmationToken);

            if (result.Succeeded)
            {
                new ResponseDTO
                {
                    Message =
                        $"{EnvironmentVariables.API_FRONT_URL}/auth/email-confirmation-success",
                    Status = 200
                };
            }

            return new ResponseDTO { Message = "Validation échouée", Status = 400 };
        }

        public async Task<ResponseDTO> ForgotPassword(ForgotPasswordInput model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                try
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
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

                    return new ResponseDTO
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
                    };
                }
                catch
                {
                    return new ResponseDTO
                    {
                        Message = "Erreur de réinitialisation, réessayez plus tard ",
                        Status = 400,
                    };
                }
            }

            return new ResponseDTO
            {
                Message = "Erreur de réinitialisation, réessayez plus tard ",
                Status = 400,
            };
        }

        public async Task<ResponseDTO> ChangePassword(PasswordRecoveryInput model)
        {
            UserApp? user = await userManager.FindByIdAsync(model.UserId);
            if (user is null)
            {
                return new ResponseDTO { Message = "L'utilisateur n'existe pas", Status = 404 };
            }
            // var decodedToken = HttpUtility.UrlDecode(model.ResetToken);
            user.RefreshToken = Guid.NewGuid().ToString();
            IdentityResult result = await userManager.ResetPasswordAsync(
                user: user,
                token: model.ResetToken,
                newPassword: model.Password
            );

            if (result.Succeeded)
            {
                await context.SaveChangesAsync();
                await notificationService.AddNotification(
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = user.Id,
                        SenderId = user.Id,
                        Type = EnumNotificationType.PasswordChanged
                    }
                );
                return new ResponseDTO
                {
                    Message = "Mot de passe vient d'être modifié",
                    Status = 201,
                };
            }

            return new ResponseDTO
            {
                Message = "Problème de validation, votre token est valid ?",
                Status = 404,
            };
        }

        public async Task<ResponseDTO> UploadAvatar(
            IFormFile file,
            ClaimsPrincipal UserPrincipal,
            HttpRequest request
        )
        {
            if (file == null)
            {
                return new ResponseDTO { Message = "Aucun fichier téléversé", Status = 400 };
            }
            var user = CheckUser.GetUserFromClaim(UserPrincipal, context);
            if (user is null)
            {
                return new ResponseDTO { Status = 400, Message = "Demande refusée" };
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
                return new ResponseDTO
                {
                    Status = 404,
                    Message = "le type du ficheir n'est pas autorisé'"
                };
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

            var url = $"{request.Scheme}://{request.Host}/images/{fileName}";
            user.ImgUrl = url;
            await context.SaveChangesAsync();

            return new ResponseDTO
            {
                Message = "Avatar téléversé",
                Status = 200,
                Data = new { fileName, url }
            };
        }

        public async Task<ResponseDTO> UpdateRefreshToken(
            RefreshTokenBodyInput values,
            HttpContext httpContext
        )
        {
            string? userEmail = httpContext
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
                return new ResponseDTO { Status = 401, Message = "Demande refusée" };

            UserApp? user = context.Users.FirstOrDefault(x => x.Email.ToLower() == userEmail);

            if (user == null)
                return new ResponseDTO { Status = 404, Message = "Demande refusée" };

            if (values.RefreshToken == user.RefreshToken)
            {
                httpContext.Response.Headers.Add(
                    key: "Access-Control-Allow-Credentials",
                    value: "true"
                );
                //HttpContext.Response.Headers.Add(key: "Authorization", value: accessToken);

                return new ResponseDTO
                {
                    Message = "Autorisation renouvelée",
                    Data = new RefreshTokenOutput(user, await GenerateAccessTokenAsync(user)),
                    Status = 200
                };
            }

            return new ResponseDTO { Status = 404, Message = "Demande refusée" };
        }

        public async Task<ResponseDTO> Login(UserLoginDTO model, HttpResponse response)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return new ResponseDTO { Message = "L'utilisateur n'existe pas ", Status = 404 };

            var result = await userManager.CheckPasswordAsync(user: user, password: model.Password);
            if (!userManager.CheckPasswordAsync(user: user, password: model.Password).Result)
                return new ResponseDTO { Message = "Connexion échouée", Status = 401 };

            //if (!await _userManager.IsEmailConfirmedAsync(user))
            //    return Unauthorized();

            if (user.RefreshToken == null) // a new refresh token has to be saved
            {
                user.RefreshToken = Guid.NewGuid().ToString();
                
            }

            // à la connection, je crée ou je met à jour le refreshtoken
            var RefreshToken = context.RefreshTokens.FirstOrDefault(x => x.UserId == user.Id);
            if (RefreshToken is null)
            {
                context.RefreshTokens.Add(
                    new RefreshTokens
                    {
                        Id = Guid.NewGuid(),
                        RefreshToken = user.RefreshToken,
                        UserId = user.Id,
                        ExpirationDate = DateTimeOffset.UtcNow.AddDays(7),
                    }
                );
            }
            else
            {
                RefreshToken.RefreshToken = user.RefreshToken;
                RefreshToken.ExpirationDate = DateTimeOffset.UtcNow.AddDays(7);
                context.Entry(RefreshToken).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            user.LastLogginAt = DateTime.Now;
            await context.SaveChangesAsync();
            // to allow cookies sent from the front end
            response.Headers.Add(key: "Access-Control-Allow-Credentials", value: "true");
            var userRoles = await userManager.GetRolesAsync(user);

            return new ResponseDTO
            {
                Message = "Connexion réussite",
                Status = 200,
                Data = new
                {
                    Token = await GenerateAccessTokenAsync(user),
                    refreshToken = user.RefreshToken,
                    User = user.ToUserResponseDTO(userRoles),
                },
            };
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

            var userRoles = await userManager.GetRolesAsync(user);

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

            context.Entry(user).State = EntityState.Modified;

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string?> GenerateAccountConfirmationLink(UserApp user)
        {
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            confirmationToken = HttpUtility.UrlEncode(confirmationToken);

            var confirmationLink =
                EnvironmentVariables.API_BACK_URL
                + "/users/email-confirmation?userId="
                + user.Id
                + "&confirmationToken="
                + confirmationToken;

            return confirmationLink;
        }

        private async Task<bool> IsEmailAlreadyUsedAsync(string email)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            return existingUser != null;
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
    }
}

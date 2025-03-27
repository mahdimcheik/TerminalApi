using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Web;
using TerminalApi.Contexts;
using TerminalApi.Models;
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

        public AuthService(
            ApiDefaultContext context,
            UserManager<UserApp> userManager,
            RoleManager<Role> roleManager,
            SignInManager<UserApp> signInManager,
            SendMailService mailService

        )
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.signInManager = signInManager;
            this.mailService = mailService;
        }

        public async Task<ResponseDTO> Register(UserCreateDTO model)
        {
            bool isEmailAlreadyUsed = await IsEmailAlreadyUsedAsync(model.Email);

            // Vérifier si l'adresse e-mail est déjà utilisée
            if (isEmailAlreadyUsed)
            {
                // Si l'adresse e-mail est déjà utilisée, mettre à jour la réponse et sauter vers l'étiquette UserAlreadyExisted

                return 
                    new ResponseDTO { Status = 404, Message = "\"L'email est déjà utilisé\"" }
                ;
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
                return 
                    new ResponseDTO
                    {
                        Message = "Profil créé",
                        Status = 201,
                        Data = newUser.ToUserResponseDTO(),
                    }
                ;
                ;
            }
            catch (Exception e)
            {
                // En cas d'exception, afficher la trace et retourner une réponse avec le statut approprié
                Console.WriteLine(e);
                return 
                    new ResponseDTO { Status = 400, Message = "Le compte n'est pas créé!!!" }
                ;
            }            
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
    }
}

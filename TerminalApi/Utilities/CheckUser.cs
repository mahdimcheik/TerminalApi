using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.User;

namespace TerminalApi.Utilities
{
    public class CheckUser
    {
        public static UserApp? GetUserFromClaim(
            ClaimsPrincipal userClaim,
            ApiDefaultContext context
        )
        {
            string? userEmail = userClaim
                .Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)
                ?.Value;
            if (userEmail == null)
                return null;
            var user = context
                .Users.Select(x => new UserApp
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ImgUrl = x.ImgUrl,
                    Email = x.Email,
                    DateOfBirth = x.DateOfBirth,
                    Description = x.Description,
                    Title = x.Title,
                    RefreshToken = x.RefreshToken,
                    EmailConfirmed = x.EmailConfirmed
                })
                .FirstOrDefault(x => x.Email == userEmail);

            if (user == null)
                return null;
            return user;
        }

        public static async Task<(UserApp? user, bool isNull)> CheckUserNullByEmail(
            string email,
            UserManager<UserApp> _userManager
        )
        {
            var user = await _userManager.FindByEmailAsync(email);

            return (user, user is null);
        }

        public static async Task<(UserApp? user, bool isNull)> CheckUserNullByUserId(
            string id,
            UserManager<UserApp> _userManager
        )
        {
            var user = await _userManager.FindByIdAsync(id);

            return (user, user is null);
        }
    }
}

using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.User;

namespace TerminalApi.Utilities
{
    public class CheckUser
    {
        public static UserApp? GetUserFromClaim(ClaimsPrincipal userClaim, ApiDefaultContext context)
        {
            string? userEmail = userClaim.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (userEmail == null) return null;
            var user = context.Users.FirstOrDefault(x => x.Email == userEmail);
            if (user == null) return null;
            return user;
        }
        public static async Task<bool> CheckUserNullByEmail(string email, UserManager<UserApp> _userManager)
        {

            var user = await _userManager.FindByEmailAsync(email);

            return user is null;
        }
        public static async Task<bool> CheckUserNullByUserId(string id, UserManager<UserApp> _userManager)
        {

            var user = await _userManager.FindByIdAsync(id);

            return user is null;
        }
    }
}

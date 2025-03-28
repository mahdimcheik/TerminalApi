using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using TerminalApi.Contexts;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;

        public OAuthController (UserManager<UserApp> userManager, ApiDefaultContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }
        #region google oAuth

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

            var user = await userManager.FindByEmailAsync(userInfo.Email);
            if (user == null)
            {
                user = new UserApp { UserName = userInfo.Email, Email = userInfo.Email };
                await userManager.CreateAsync(user);
                await userManager.AddLoginAsync(
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
        #endregion
    }
}

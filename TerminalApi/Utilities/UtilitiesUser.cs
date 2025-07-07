using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TerminalApi.Utilities
{
    public static class UtilitiesUser
    {
        public static ClaimsPrincipal? GetPrincipalFromToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(EnvironmentVariables.JWT_KEY)
                ),
                ValidateLifetime = true, // Change to true to validate token lifetime
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

            // Check if the token is expired
            if (jwtSecurityToken.ValidTo < DateTime.UtcNow)
                throw new SecurityTokenException("Token has expired");

            return principal;
        }
    }
}

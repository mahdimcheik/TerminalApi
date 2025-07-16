using System.Security.Claims;
using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDTO<UserResponseDTO>> Register(UserCreateDTO model);
        Task<ResponseDTO<UserResponseDTO>> ResendConfirmationMail(UserApp newUser);
        Task<ResponseDTO<UserResponseDTO>> Update(UserUpdateDTO model, ClaimsPrincipal UserPrincipal);
        Task<ResponseDTO<string?>> EmailConfirmation(string userId, string confirmationToken);
        Task<ResponseDTO<PasswordResetResponseDTO>> ForgotPassword(ForgotPasswordInput model);
        Task<ResponseDTO<string?>> ChangePassword(PasswordRecoveryInput model);
        Task<ResponseDTO<UserResponseDTO>> UploadAvatar(IFormFile file, ClaimsPrincipal UserPrincipal, HttpRequest request);
        Task<ResponseDTO<LoginOutputDTO>> UpdateRefreshToken(string refreshToken, HttpContext httpContext);
        Task<ResponseDTO<LoginOutputDTO>> Login(UserLoginDTO model, HttpResponse response);
        Task<string> GenerateAccessTokenAsync(UserApp user);
    }
} 
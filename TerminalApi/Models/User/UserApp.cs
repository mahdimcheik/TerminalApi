using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Models.User
{
    public class UserApp : IdentityUser
    {

        [Required]
        public string FirstName { get; set; }

        // Last name of the user, marked as required
        [Required]
        public string LastName { get; set; }

        // Refresh token for authentication
        public string? RefreshToken { get; set; }



    }

    public class UserResponseDTO
    {
        public string Id { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        public IEnumerable<string>? Roles { get; set; }
    }
    public class UserLoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
    public class ConfirmAccountInput
    {
        public string UserId { get; set; }
        public string ConfirmationToken { get; set; }
    }

    public class UserCreateDTO
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

    }

    public static class UserExtension
    {
        public static UserApp ToUser(this UserCreateDTO userDTO)
        {
            return new UserApp
            {
                Email = userDTO.Email,
                UserName = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
            };
        }
        public static UserResponseDTO ToUserResponseDTO(this UserApp userDTO, IEnumerable<string>? roles = null)
        {
            return new UserResponseDTO
            {
                Email = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Id = userDTO.Id,
                Roles = roles
            };
        }
    }

}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TerminalApi.Models.Adresse;

namespace TerminalApi.Models.User
{
    public class UserApp : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        public string? ImgUrl { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }
        public string? RefreshToken { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModifiedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? LastLogginAt { get; set; }
        public ICollection<Address>? Adresses { get; set; }
    }

    public class UserUpdateDTO
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }
    }

    public class UserResponseDTO
    {
        public string Id { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        public string? ImgUrl { get; set; }
        public DateTime? LastLogginAt { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public ICollection<string>? Roles { get; set; }
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

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }
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
                DateOfBirth = userDTO.DateOfBirth,
            };
        }

        public static UserApp ToUser(this UserUpdateDTO userDTO, UserApp user)
        {
            user.DateOfBirth = userDTO.DateOfBirth;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.LastModifiedAt = DateTime.Now;
            return user;
        }

        public static UserResponseDTO ToUserResponseDTO(
            this UserApp userDTO,
            ICollection<string>? roles = null
        )
        {
            return new UserResponseDTO
            {
                Email = userDTO.Email,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                DateOfBirth = userDTO.DateOfBirth,
                LastLogginAt = userDTO.LastLogginAt,
                ImgUrl = userDTO.ImgUrl,
                Id = userDTO.Id,
                Roles = roles,
            };
        }
    }

    public class ForgotPasswordInput
    {
        [Required(ErrorMessage = "Email required")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }

    public class ChangePasswordInput
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordConfirmation { get; set; }
    }

    public class PasswordRecoveryInput
    {
        [Required(ErrorMessage = "UserId required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "ConfirmationToken required")]
        public string ResetToken { get; set; }

        [Required(ErrorMessage = "Password required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "PasswordConfirmation required")]
        public string PasswordConfirmation { get; set; }
    }
}

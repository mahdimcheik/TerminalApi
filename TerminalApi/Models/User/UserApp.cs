using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using TerminalApi.Models;
using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public class UserApp : IdentityUser
    {
        public string FirstName { get; set; }
     
        public string LastName { get; set; }
        public EnumGender Gender { get; set; }
        public string? ImgUrl { get; set; }

        public string? Description { get; set; }
        public string? Title { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? GithubUrl { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset LastModifiedAt { get; set; }

        public DateTimeOffset? LastLogginAt { get; set; }

        // Bannir un utilisateur
        public bool? IsBanned { get; set; } = false;
        public DateTimeOffset? BannedUntilDate { get; set; }

        // navigations properties
        public ICollection<Address>? Adresses { get; set; }
        public ICollection<Slot>? Slots { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Formation>? Formations { get; set; }
        public ICollection<Notification>? NotificationsRecieved { get; set; }
        public ICollection<Notification>? NotificationsCreated { get; set; }
        public ICollection<Order>? Orders { get; set; }

    }

    public class LoginOutputDTO
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserResponseDTO User { get; set; } = null!;
    }
    public class UserUpdateDTO
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? PhoneNumber { get; set; }
        [Column(TypeName = "Text")]
        public string? Description { get; set; }
        public string? Title { get; set; }

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset DateOfBirth { get; set; }
        [Required]
        public EnumGender Gender { get; set; }
    }

    public class UserResponseDTO
    {
        public string Id { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; } = null!;
        public string? ImgUrl { get; set; }
        public string? Description { get; set; }
        public string? Title { get; set; }

        public string?  LinkedinUrl { get; set; }
        public string?  GithubUrl { get; set; }

        public bool IsBanned { get; set; }
        public DateTimeOffset? BannedUntilDate { get; set; }

        public EnumGender Gender { get; set; }
        public DateTimeOffset? LastLogginAt { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; } = false;
        public ICollection<string>? Roles { get; set; }

        public ICollection<Address>? Adresses { get; set; }
        public ICollection<Slot>? Slots { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Formation>? Formations { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<Order>? Orders { get; set; }

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
        public string? PhoneNumber { get; set; }
        [Column(TypeName = "Text")]
        public string? Description { get; set; }
        public string? ImgUrl { get; set; } = @"https://upload.wikimedia.org/wikipedia/commons/thumb/1/12/User_icon_2.svg/1200px-User_icon_2.svg.png";
        public string? Title { get; set; }
        [Required]
        public EnumGender Gender { get; set; } = EnumGender.Autre;

        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset DateOfBirth { get; set; }

        public string? LinkedinUrl { get; set; }
        public string? GithubUrl { get; set; }

        [MustBeTrue]
        public bool? privacyPolicyConsent { get; set; }
  
        [MustBeTrue]
        public bool? dataProcessingConsent { get; set; }
    }

    public class UserBanDTO
    {
        [Required]
        public string UserId { get; set; } = null!;
        [Required]
        public bool IsBanned { get; set; } = false;
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset? BannedUntilDate { get; set; }
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
                Gender = userDTO.Gender,
                PhoneNumber = userDTO.PhoneNumber,
                Description = userDTO.Description,
                Title = userDTO.Title,
                LinkedinUrl = userDTO.LinkedinUrl,
                GithubUrl = userDTO.GithubUrl
            };
        }

        public static UserApp ToUser(this UserUpdateDTO userDTO, UserApp user)
        {
            user.DateOfBirth = userDTO.DateOfBirth;
            user.FirstName = userDTO.FirstName;
            user.Gender = userDTO.Gender;
            user.LastName = userDTO.LastName;
            user.LastModifiedAt = DateTime.Now;
            user.Description = userDTO.Description;
            user.Title = userDTO.Title;
            user.LinkedinUrl = userDTO.LinkedinUrl;
            user.GithubUrl = userDTO.GithubUrl;
            return user;
        }

        public static UserResponseDTO ToUserResponseDTO(
            this UserApp user,
            ICollection<string>? roles = null
        )
        {
            return new UserResponseDTO
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                LastLogginAt = user.LastLogginAt,
                ImgUrl = user.ImgUrl,
                Description = user.Description,
                Title = user.Title,
                Gender = user.Gender,
                Id = user.Id,
                IsBanned = user.IsBanned ?? false,
                BannedUntilDate = user.BannedUntilDate,
                EmailConfirmed = user.EmailConfirmed,
                LinkedinUrl = user.LinkedinUrl,
                GithubUrl = user.GithubUrl,
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

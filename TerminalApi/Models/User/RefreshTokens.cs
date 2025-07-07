using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Models
{
    public class RefreshTokens
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string? RefreshToken { get; set; }
        [Required]
        [Column(TypeName = "timestamp with time zone")]
        public DateTimeOffset ExpirationDate { get; set; }
        [Required]
        [ForeignKey(nameof(User))]
        public string? UserId { get; set; }
        public UserApp? User { get; set; }
    }

    public static class RefreshTokenExtensions
    {
        public static bool IsExpired(this RefreshTokens refreshToken)
        {
            return refreshToken.ExpirationDate < DateTimeOffset.UtcNow;
        }
    }
}

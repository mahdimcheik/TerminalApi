using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TerminalApi.Models.User
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
}

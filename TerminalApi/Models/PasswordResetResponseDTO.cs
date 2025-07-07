namespace TerminalApi.Models
{
    public class PasswordResetResponseDTO
    {
        public string ResetToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
} 
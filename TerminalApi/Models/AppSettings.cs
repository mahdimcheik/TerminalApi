namespace TerminalApi.Models
{
    public class AppSettings
    {
        public TeacherSettings Teacher { get; set; } = new();
        public ApiSettings Api { get; set; } = new();
        public DatabaseSettings Database { get; set; } = new();
        public TokenSettings Token { get; set; } = new();
        public SmtpSettings Smtp { get; set; } = new();
        public MailSettings Mail { get; set; } = new();
        public GoogleSettings Google { get; set; } = new();
        public StripeSettings Stripe { get; set; } = new();
        public HangfireSettings Hangfire { get; set; } = new();
        public CheckoutSettings Checkout { get; set; } = new();
        public CookiesSettings Cookies { get; set; } = new();
        public EnvironmentSettings Environment { get; set; } = new();
    }

    public class TeacherSettings
    {
        public string Guid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ApiSettings
    {
        public string FrontUrl { get; set; } = string.Empty;
        public string BackUrl { get; set; } = string.Empty;
    }

    public class DatabaseSettings
    {
        public string Name { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
    }

    public class TokenSettings
    {
        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string JwtKey { get; set; } = string.Empty;
        public int ValidityMinutes { get; set; } = 60;
    }

    public class SmtpSettings
    {
        public string BrevoKey { get; set; } = string.Empty;
        public string BrevoServer { get; set; } = string.Empty;
        public int BrevoPort { get; set; } = 587;
        public string BrevoLogin { get; set; } = string.Empty;
    }

    public class MailSettings
    {
        public string DoNotReplyMail { get; set; } = string.Empty;
    }

    public class GoogleSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }

    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string SecretEndpointTest { get; set; } = string.Empty;
    }

    public class HangfireSettings
    {
        public int OrderCleaningDelayMinutes { get; set; } = 30;
    }

    public class CheckoutSettings
    {
        public int ExpiryDelayMinutes { get; set; } = 30;
    }

    public class CookiesSettings
    {
        public int ValidityDays { get; set; } = 7;
    }

    public class EnvironmentSettings
    {
        public bool DockerEnvironment { get; set; } = true;
    }
}

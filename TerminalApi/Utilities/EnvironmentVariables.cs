using Microsoft.Extensions.Configuration;
using TerminalApi.Models;

namespace TerminalApi.Utilities
{
    public static class EnvironmentVariables
    {
        private static IConfiguration? _configuration;
        private static AppSettings? _appSettings;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = new AppSettings();
            _configuration.GetSection("AppSettings").Bind(_appSettings);
        }

        private static AppSettings AppSettings => _appSettings ?? throw new InvalidOperationException("EnvironmentVariables not initialized. Call Initialize() first.");

        public static string JWT_KEY => AppSettings.Token.JwtKey;

        public static string? API_BACK_URL => AppSettings.Api.BackUrl;
        public static string? API_FRONT_URL => AppSettings.Api.FrontUrl;
        public static string GOOGLE_API_KEY => AppSettings.Google.ApiKey;

        //DATABASE
        public static string? DB_PORT => AppSettings.Database.Port;
        public static string? DB_HOST => AppSettings.Database.Host;
        public static string? DB_NAME => AppSettings.Database.Name;
        public static string? DB_USER => AppSettings.Database.User;
        public static string? DB_PASSWORD => AppSettings.Database.Password;

        public static string? DB_CONNECTION_STRING => 
            !string.IsNullOrEmpty(AppSettings.Database.ConnectionString) 
                ? AppSettings.Database.ConnectionString 
                : _configuration?.GetConnectionString("DefaultConnection") 
                ?? $"Host={DB_HOST};Port={DB_PORT};Database={DB_NAME};Username={DB_USER};Password={DB_PASSWORD}";

        public static string? DB_PROVIDER => AppSettings.Database.Provider;

        // Environments
        public static bool dockerEnvironment => AppSettings.Environment.DockerEnvironment;
        public static bool devEnvironment => !testEnvironment && !productionEnvironment;
        public static bool testEnvironment =>
            Environment
                .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!
                .ToLower()
                .Contains("test");
        public static bool productionEnvironment =>
            Environment
                .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!
                .ToLower()
                .Contains("prod");

        //Mail
        public static string? SMTP_HostAddress => AppSettings.Smtp.BrevoServer;
        public static string? SMTP_EmailFrom => AppSettings.Smtp.BrevoLogin;
        public static string? SMTP_Password => AppSettings.Smtp.BrevoKey;
        public static int SMTP_Port => AppSettings.Smtp.BrevoPort;

        public static string DO_NO_REPLY_MAIL => AppSettings.Mail.DoNotReplyMail;

        // google oauth
        public static string? ID_CLIENT_GOOGLE => AppSettings.Google.ClientId;
        public static string? SECRET_CLIENT_GOOGLE => AppSettings.Google.ClientSecret;
        public static string? GOOGLE_REDIRECT_URL => AppSettings.Google.RedirectUrl;
        
        public static string TEACHER_ID => AppSettings.Teacher.Guid;
        public static string TEACHER_EMAIL => AppSettings.Teacher.Email;

        //stripe
        public static string? STRIPE_PUBLISHABLEKEY => AppSettings.Stripe.PublishableKey;
        public static string? STRIPE_SECRETKEY => AppSettings.Stripe.SecretKey;
        public static string? STRIPE_SECRET_ENDPOINT_TEST => AppSettings.Stripe.SecretEndpointTest;

        public static int CHECKOUT_EXPIRY_DELAY => AppSettings.Checkout.ExpiryDelayMinutes;

        // hangfire
        public static int HANGFIRE_ORDER_CLEANING_DELAY => AppSettings.Hangfire.OrderCleaningDelayMinutes;

        // Authentification
        public static int COOKIES_VALIDITY_DAYS => AppSettings.Cookies.ValidityDays;
        public static int TOKEN_VALIDITY_MINUTES => AppSettings.Token.ValidityMinutes;
    }
}

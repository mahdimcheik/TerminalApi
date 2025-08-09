using Microsoft.Extensions.Configuration;
using TerminalApi.Models;

namespace TerminalApi.Utilities
{
    public static class EnvironmentVariables
    {        
        // Helper method to get environment variable with fallback to appsettings
        private static string GetEnvVar(string envVarName, string fallbackValue)
        {
            return Environment.GetEnvironmentVariable(envVarName) ?? fallbackValue;
        }

        private static int GetEnvVarInt(string envVarName, int fallbackValue)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            return int.TryParse(envValue, out var result) ? result : fallbackValue;
        }

        private static bool GetEnvVarBool(string envVarName, bool fallbackValue)
        {
            var envValue = Environment.GetEnvironmentVariable(envVarName);
            return bool.TryParse(envValue, out var result) ? result : fallbackValue;
        }

        public static string JWT_KEY => GetEnvVar("JWT_KEY", "");
        public static string TOKEN_AUDIENCE => GetEnvVar("TOKEN_AUDIENCE", "http://localhost:3000");
        public static string TOKEN_ISSUER => GetEnvVar("TOKEN_ISSUER", "https://localhost:5001");

        public static string? API_BACK_URL => GetEnvVar("API_BACK_URL","");
        public static string? API_FRONT_URL => GetEnvVar("API_FRONT_URL", "");

        //DATABASE
        public static string? DB_PORT => GetEnvVar("DB_PORT", "5432");
        public static string? DB_HOST => GetEnvVar("DB_HOST", "localhost");
        public static string? DB_NAME => GetEnvVar("DB_NAME", "leprojet");
        public static string? DB_USER => GetEnvVar("DB_USER", "postgres");
        public static string? DB_PASSWORD => GetEnvVar("DB_PASSWORD", "beecoming");
        public static string? DB_PROVIDER => GetEnvVar("DB_PROVIDER", "PostgreSql");
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
        public static string SMTP_HostAddress => GetEnvVar("SMTP_BREVO_SERVER", "smtp-relay.brevo.com");
        public static string SMTP_EmailFrom => GetEnvVar("SMTP_BREVO_LOGIN", "");
        public static string SMTP_Password => GetEnvVar("SMTP_BREVO_KEY", "");
        public static int SMTP_Port => GetEnvVarInt("SMTP_BREVO_PORT", 587);

        public static string DO_NO_REPLY_MAIL => GetEnvVar("DO_NO_REPLY_MAIL", "ne-pas-repondre@skillhive.com");
        
        public static string TEACHER_ID => GetEnvVar("TEACHER_ID", "44ea5267-31c5-44a6-94a3-bac6efd009c7");
        public static string TEACHER_EMAIL => GetEnvVar("TEACHER_EMAIL", "");
        public static string TEACHER_PASSWORD => GetEnvVar("TEACHER_PASSWORD", "");

        public static string? STRIPE_SECRET_KEY => GetEnvVar("STRIPE_SECRET_KEY", "");
        public static string? STRIPE_PUBLISHABLE_KEY => GetEnvVar("STRIPE_PUBLISHABLE_KEY", "");
        public static string? STRIPE_SECRET_ENDPOINT_TEST => GetEnvVar("STRIPE_SECRET_ENDPOINT_TEST", "");

        public static int CHECKOUT_EXPIRY_DELAY => GetEnvVarInt("CHECKOUT_EXPIRY_DELAY", 15);

        // hangfire
        public static int HANGFIRE_ORDER_CLEANING_DELAY => GetEnvVarInt("HANGFIRE_ORDER_CLEANING_DELAY", 60);

        // Authentification
        public static int COOKIES_VALIDITY_DAYS => GetEnvVarInt("COOKIES_VALIDITY_DAYS", 7);
        public static int TOKEN_VALIDITY_MINUTES => GetEnvVarInt("TOKEN_VALIDATY_MINUTES", 15);
    }
}

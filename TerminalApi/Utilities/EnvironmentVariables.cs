﻿namespace TerminalApi.Utilities
{
    public static class EnvironmentVariables
    {
        public static string JWT_KEY =>
            Environment.GetEnvironmentVariable("JWT_KEY")
            ?? "7e89MVHPO-W-LXh1oQHbrfP0agnN5xqaJ_q5RsH2AgCAP9tyKShZF8Mtpyad2hcc5l4hdyX_2eMw6L0LfllO06ifUEGqAaFlASiVArgYQhxwPuaKCtlqydQ484_eaE6kh5tjWGCH2QeypkJSGTJTw4FyY5p01hiza1HgjQyzcGs";

        public static string? API_BACK_URL =>
            Environment.GetEnvironmentVariable("API_BACK_URL");
        public static string? API_FRONT_URL =>
            Environment.GetEnvironmentVariable("API_FRONT_URL");
        public static string GOOGLE_API_KEY { get; set; } =
            Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";

        //DATABASE
        public static string? DB_PORT => Environment.GetEnvironmentVariable("DB_PORT");
        public static string? DB_HOST => Environment.GetEnvironmentVariable("DB_HOST");
        public static string? DB_NAME => Environment.GetEnvironmentVariable("DB_NAME");
        public static string? DB_USER => Environment.GetEnvironmentVariable("DB_USER");
        public static string? DB_PASSWORD => Environment.GetEnvironmentVariable("DB_PASSWORD");
        public static string? DB_PROVIDER =>
            Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "PostgreSql";

        // Environments
        public static bool dockerEnvironment =>
            !(Environment.GetEnvironmentVariable("dockerEnvironment") == "false");
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
        public static string? SMTP_HostAddress =>
            Environment.GetEnvironmentVariable("SMTP_BREVO_SERVER");
        public static string? SMTP_EmailFrom =>
            Environment.GetEnvironmentVariable("SMTP_BROVO_LOGIN");
        public static string? SMTP_Password => Environment.GetEnvironmentVariable("SMTP_BREVO_KEY");
        public static int SMTP_Port =>
            Int32.Parse(Environment.GetEnvironmentVariable("SMTP_BREVO_PORT"));

        public static string DO_NO_REPLY_MAIL => Environment.GetEnvironmentVariable("DO_NO_REPLY_MAIL") ?? "ne-pas-repondre@inspire.com";
        // google oauth
        public static string ID_CLIENT_GOOGLE => Environment.GetEnvironmentVariable("ID_CLIENT_GOOGLE");
        public static string? SECRET_CLIENT_GOOGLE => Environment.GetEnvironmentVariable("SECRET_CLIENT_GOOGLE");
        public static string? GOOGLE_REDIRECT_URL => Environment.GetEnvironmentVariable("GOOGLE_REDIRECT_URL");
        public static string? TEACHER_ID => Environment.GetEnvironmentVariable("TEACHER_ID");

        //stripe
        public static string? STRIPE_PUBLISHABLEKEY => Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLEKEY");
        public static string? STRIPE_SECRETKEY => Environment.GetEnvironmentVariable("STRIPE_SECRETKEY");
        public static string? STRIPE_SECRET_ENDPOINT_TEST = Environment.GetEnvironmentVariable("STRIPE_SECREt_ENDPOINT_TEST");

        // hangfire
        public static string? HANGFIRE_ORDER_CLEANING_DELAY =>
            Environment.GetEnvironmentVariable("HANGFIRE_ORDER_CLEANING_DELAY");
    }
}

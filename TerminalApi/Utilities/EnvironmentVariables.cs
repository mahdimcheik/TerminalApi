namespace TerminalApi.Utilities
{
    public static class EnvironmentVariables
    {
        public static string? APP_SUFFIX_URL =>
            Environment.GetEnvironmentVariable("APP_SUFFIX_URL");
        public static string JWT_KEY =>
            Environment.GetEnvironmentVariable("JWT_KEY")
            ?? "7e89MVHPO-W-LXh1oQHbrfP0agnN5xqaJ_q5RsH2AgCAP9tyKShZF8Mtpyad2hcc5l4hdyX_2eMw6L0LfllO06ifUEGqAaFlASiVArgYQhxwPuaKCtlqydQ484_eaE6kh5tjWGCH2QeypkJSGTJTw4FyY5p01hiza1HgjQyzcGs";

        public static string? USER_PREFIX_URL =>
            Environment.GetEnvironmentVariable("USER_PREFIX_URL");
        public static string? USER_BASE_URL =>
            string.Format("http://{0}{1}", USER_PREFIX_URL, APP_SUFFIX_URL);

        public static string? API_PREFIX_URL =>
            Environment.GetEnvironmentVariable("API_PREFIX_URL");
        public static string? API_BASE_URL =>
            string.Format("http://{0}{1}", API_PREFIX_URL, APP_SUFFIX_URL);

        public static string API_HERE_KEY { get; set; } =
            Environment.GetEnvironmentVariable("API_HERE_KEY") ?? "";
        public static string GOOGLE_API_KEY { get; set; } =
            Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";

        //DATABASE
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
    }
}

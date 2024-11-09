namespace TerminalApi.Utilities
{
    public static class EnvironmentVariables
    {
        public static string? APP_SUFFIX_URL => Environment.GetEnvironmentVariable("APP_SUFFIX_URL");
        public static string JWT_KEY => Environment.GetEnvironmentVariable("JWT_KEY") ?? "7e89MVHPO-W-LXh1oQHbrfP0agnN5xqaJ_q5RsH2AgCAP9tyKShZF8Mtpyad2hcc5l4hdyX_2eMw6L0LfllO06ifUEGqAaFlASiVArgYQhxwPuaKCtlqydQ484_eaE6kh5tjWGCH2QeypkJSGTJTw4FyY5p01hiza1HgjQyzcGs";

        public static string? USER_PREFIX_URL => Environment.GetEnvironmentVariable("USER_PREFIX_URL");
        public static string? USER_BASE_URL => string.Format("http://{0}{1}", USER_PREFIX_URL, APP_SUFFIX_URL);

        public static string? API_PREFIX_URL => Environment.GetEnvironmentVariable("API_PREFIX_URL");
        public static string? API_BASE_URL => string.Format("http://{0}{1}", API_PREFIX_URL, APP_SUFFIX_URL);

        public static string API_HERE_KEY { get; set; } = Environment.GetEnvironmentVariable("API_HERE_KEY") ?? "";
        public static string GOOGLE_API_KEY { get; set; } = Environment.GetEnvironmentVariable("GOOGLE_API_KEY") ?? "";

        //DATABASE
        public static string? DB_HOST => Environment.GetEnvironmentVariable("DB_HOST");
        public static string? DB_NAME => Environment.GetEnvironmentVariable("DB_NAME");
        public static string? DB_USER => Environment.GetEnvironmentVariable("DB_USER");
        public static string? DB_PASSWORD => Environment.GetEnvironmentVariable("DB_PASSWORD");
        public static string? DB_PROVIDER => Environment.GetEnvironmentVariable("DB_PROVIDER") ?? "PostgreSql";

        // Environments
        public static bool dockerEnvironment => !(Environment.GetEnvironmentVariable("dockerEnvironment") == "false");
        public static bool devEnvironment => !testEnvironment && !productionEnvironment;
        public static bool testEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.ToLower().Contains("test");
        public static bool productionEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.ToLower().Contains("prod");

        //Mail
        public static string? SMTP_HostAddress => Environment.GetEnvironmentVariable("EMAIL_SENDER_HOST_ADDRESS");
        public static string? SMTP_EmailFrom => Environment.GetEnvironmentVariable("EMAIL_SENDER_FROM");
        public static string? SMTP_Password => Environment.GetEnvironmentVariable("EMAIL_SENDER_PASSWORD");
        public static string? SMTP_Port => Environment.GetEnvironmentVariable("EMAIL_SENDER_PORT");

        public static string? IMAP_HOST => Environment.GetEnvironmentVariable("IMAP_HOST");
        public static string? IMAP_PORT => Environment.GetEnvironmentVariable("IMAP_PORT");
        public static string? IMAP_CLIENT_ID => Environment.GetEnvironmentVariable("IMAP_CLIENT_ID");
        public static string? IMAP_SECRET => Environment.GetEnvironmentVariable("IMAP_CLIENT_SECRET");
        public static string? IMAP_TENANT_ID => Environment.GetEnvironmentVariable("IMAP_TENANT_ID");
        public static string? IMAP_EMAIL => Environment.GetEnvironmentVariable("IMAP_EMAIL");

        public static string? API_NAME => Environment.GetEnvironmentVariable("API_NAME");

        // Relais colis
        public static string? RELAIS_COLIS_API_KEY => Environment.GetEnvironmentVariable("RELAIS_COLIS_API_KEY");
        public static string? RELAIS_COLIS_PROPOSITION_RELAIS_URL => Environment.GetEnvironmentVariable("RELAIS_COLIS_PROPOSITION_RELAIS_URL");
        public static string? RELAIS_COLIS_CREATE_SHIPMENT_URL => Environment.GetEnvironmentVariable("RELAIS_COLIS_CREATE_SHIPMENT_URL");
        public static string? RELAIS_COLIS_USER => Environment.GetEnvironmentVariable("RELAIS_COLIS_USER");
        public static string? RELAIS_COLIS_PASSWORD => Environment.GetEnvironmentVariable("RELAIS_COLIS_PASSWORD");

        // DPD
        public static string? DPD_URL => Environment.GetEnvironmentVariable("DPD_URL");
        public static string? DPD_USER => Environment.GetEnvironmentVariable("DPD_USER");
        public static string? DPD_PASSWORD => Environment.GetEnvironmentVariable("DPD_PASSWORD");
        public static string? DPD_CUSTOMER_CENTER_NUMBER => Environment.GetEnvironmentVariable("DPD_CUSTOMER_CENTER_NUMBER");
        public static string? DPD_CUSTOMER_NUMBER => Environment.GetEnvironmentVariable("DPD_CUSTOMER_NUMBER");

        // DHL
        public static string? DHL_URL => Environment.GetEnvironmentVariable("DHL_URL");
        public static string? DHL_USER => Environment.GetEnvironmentVariable("DHL_USER");
        public static string? DHL_PASSWORD => Environment.GetEnvironmentVariable("DHL_PASSWORD");
    }
}

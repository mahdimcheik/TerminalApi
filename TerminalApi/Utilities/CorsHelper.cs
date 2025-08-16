namespace TerminalApi.Utilities
{
    public static class CorsHelper
    {
        public static bool IsOriginAllowed(string origin)
        {
            List<string> localUrls =
                new()
                {
                    "http://localhost",
                    "https://localhost",                    
                    "https://localhost:4200",
                    "http://localhost:4200",
                    "https://localhost:4201",
                    "https://192.168.1.19:4201",
                    "http://192.168.1.19:4201",
                    "http://localhost:4201",
                    "http://localhost:7113",
                    "https://localhost:7113",
                    EnvironmentVariables.API_BACK_URL,
                    EnvironmentVariables.API_FRONT_URL,
                    $"api.${EnvironmentVariables.API_BACK_URL}",
                    $"api-dev.${EnvironmentVariables.API_BACK_URL}",
                    $"api-test.${EnvironmentVariables.API_BACK_URL}",
                    "https://recette-tpeweb.e-transactions.fr",
                    "http://www.skill-hive.fr",
                    "https://www.skill-hive.fr",
                    "http://skill-hive.fr",
                    "https://skill-hive.fr",
                };
            return localUrls.Contains(origin);
        }
    }


}

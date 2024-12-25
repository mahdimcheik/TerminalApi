namespace TerminalApi.Utilities
{
    public static class CorsHelper
    {
        /**
         * This method is used to allow CORS for local development.
         */
        public static bool IsOriginAllowed(string origin)
        {
            // Your logic.
            var localNetworkIps = Enumerable.Range(2, 254).ToArray();
            var localNetworkIpsString = localNetworkIps.Select(ip => $"http://192.168.0.{ip}:8100").ToList();

            List<string> localUrls = new()
        {
            "http://localhost",
            "https://localhost",
            "https://localhost:80",
            "http://localhost:80",
            "https://localhost:443",
            "http://localhost:443",
            "https://localhost:3000",
            "http://localhost:3000",
            "https://localhost:3001",
            "http://localhost:3001",
            "https://localhost:5001",
            "http://localhost:5001",
            "https://localhost:4200",
            "http://localhost:4200",
            "https://localhost:8100",
            "http://localhost:8100",
            "http://localhost:8082",
            "http://192.168.1.39:8082",
            "http://localhost:7113",
            "https://localhost:7113",
            "https://accounts.google.com",
            "https://accounts.google.com/o/oauth2/v2/auth",
            "capacitor://localhost",
            "ionic://localhost",
            EnvironmentVariables.API_BACK_URL,
            EnvironmentVariables.API_FRONT_URL,
            "https://recette-tpeweb.e-transactions.fr"
        };

            localNetworkIpsString.AddRange(localUrls);

            return localNetworkIpsString.Contains(origin);
        }
    }
}

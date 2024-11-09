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
            "capacitor://localhost",
            "ionic://localhost",
            EnvironmentVariables.USER_BASE_URL,
            EnvironmentVariables.API_BASE_URL,
            "https://recette-tpeweb.e-transactions.fr"
        };

            localNetworkIpsString.AddRange(localUrls);

            return localNetworkIpsString.Contains(origin);
        }
    }
}

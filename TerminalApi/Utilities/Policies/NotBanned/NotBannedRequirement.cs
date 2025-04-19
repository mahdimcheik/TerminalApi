using Microsoft.AspNetCore.Authorization;

namespace TerminalApi.Utilities.Policies.NotBanned
{
    public class NotBannedRequirement : IAuthorizationRequirement
    {
        public NotBannedRequirement() { }
    }
}

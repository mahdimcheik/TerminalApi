using Microsoft.AspNetCore.Authorization;
using TerminalApi.Contexts;

namespace TerminalApi.Utilities.Policies.NotBanned
{
    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly ApiDefaultContext apiDefaultContext;

        public NotBannedHandler(
            ApiDefaultContext apiDefaultContext
        )
        {
            this.apiDefaultContext = apiDefaultContext;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            NotBannedRequirement requirement
        )
        {
            if (context.User is null || !context.User.Identity.IsAuthenticated)
            {
                return;
            }

            var user = CheckUser.GetUserFromClaim(context.User, apiDefaultContext);

            if (user != null && !(user.IsBanned ?? false))
            {
                context.Succeed(requirement);
            }
        }
    }
}

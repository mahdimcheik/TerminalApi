using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.User;

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

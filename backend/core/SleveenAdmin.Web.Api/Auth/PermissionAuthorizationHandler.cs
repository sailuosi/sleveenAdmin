using Microsoft.AspNetCore.Authorization;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Web.Api.Auth;

[Singleton]
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (AuthPrincipalHelper.BypassesPermissionCheck(context.User))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.Claims.Any(c =>
                c.Type == PermissionClaimTypes.Permission &&
                string.Equals(c.Value, requirement.RequiredPermission, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

using Hangfire.Dashboard;
using SleveenAdmin.Foundation.Auth;

namespace SleveenAdmin.Web.Api.Scheduling;

public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        return AuthPrincipalHelper.BypassesPermissionCheck(http.User);
    }
}

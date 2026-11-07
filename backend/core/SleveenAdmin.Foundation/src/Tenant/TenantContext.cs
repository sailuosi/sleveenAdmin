using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SleveenAdmin.Foundation.Auth;
using SleveenAdmin.Foundation.DI;

namespace SleveenAdmin.Foundation.Tenant;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext, IScoped
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? TenantId => CurrentUser.GetTenantId(User);

    public Guid? UserId => CurrentUser.GetUserId(User);

    public bool IsSuperAdmin => AuthPrincipalHelper.BypassesPermissionCheck(User);
}

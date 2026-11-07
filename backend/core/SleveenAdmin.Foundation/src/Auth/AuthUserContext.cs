using System.Security.Claims;

namespace SleveenAdmin.Foundation.Auth;

public sealed class AuthUserContext : IAuthUserContext
{
    public Guid UserId { get; init; }
    public string Account { get; init; } = string.Empty;
    public string? RealName { get; init; }
    public Guid? OrgId { get; init; }
    public Guid? TenantId { get; init; }
    public bool IsSuperAdmin { get; init; }

    public static AuthUserContext? FromClaims(ClaimsPrincipal? user)
    {
        var userId = CurrentUser.GetUserId(user);
        var account = CurrentUser.GetAccount(user);
        if (userId is null || string.IsNullOrWhiteSpace(account))
            return null;

        return new AuthUserContext
        {
            UserId = userId.Value,
            Account = account,
            RealName = user?.FindFirstValue("realName"),
            OrgId = Guid.TryParse(user?.FindFirstValue("orgId"), out var orgId) ? orgId : null,
            TenantId = CurrentUser.GetTenantId(user),
            IsSuperAdmin = AuthPrincipalHelper.BypassesPermissionCheck(user)
        };
    }
}

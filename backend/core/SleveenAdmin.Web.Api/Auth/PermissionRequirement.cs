using Microsoft.AspNetCore.Authorization;

namespace SleveenAdmin.Web.Api.Auth;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string requiredPermission)
    {
        RequiredPermission = requiredPermission;
    }

    public string RequiredPermission { get; }
}

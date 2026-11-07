using Microsoft.AspNetCore.Authorization;

namespace SleveenAdmin.Foundation.Auth;

/// <summary>
/// 按钮/API 权限码校验，配合 <see cref="PermissionAuthorizationHandler"/> 使用。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
    public PermissionAttribute(string permission) => Policy = PermissionPolicies.ToPolicyName(permission);
}

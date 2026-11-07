using System.Security.Claims;

namespace SleveenAdmin.Foundation.Auth;

public static class AuthPrincipalHelper
{
    /// <summary>
    /// 内置 admin 账号跳过 Permission 策略与数据权限限制。
    /// </summary>
    public static bool BypassesPermissionCheck(ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true)
            return false;

        var account = CurrentUser.GetAccount(user);
        return string.Equals(account, AuthRoleCodes.AdminAccount, StringComparison.OrdinalIgnoreCase);
    }
}

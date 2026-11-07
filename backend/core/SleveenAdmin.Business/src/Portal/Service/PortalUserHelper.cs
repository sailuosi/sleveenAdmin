using System.Security.Claims;
using SleveenAdmin.Foundation.Auth;

namespace SleveenAdmin.Business.Portal.Service;

internal static class PortalUserHelper
{
    public static Guid? GetUserId(ClaimsPrincipal? user) => CurrentUser.GetUserId(user);

    public static string? GetAccount(ClaimsPrincipal? user) => CurrentUser.GetAccount(user);
}

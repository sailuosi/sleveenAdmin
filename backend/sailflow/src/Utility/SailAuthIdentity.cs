using System.Security.Claims;
using sailflow.biz.query;
using sailflow.engine.Common;
using SleveenAdmin.Foundation.Auth;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>
/// 用 JWT Claims 覆盖流程操作人 / 「我的」查询过滤键，防客户端冒用 UserId。
/// Account = ClaimTypes.Name（与 R3 SysUser.Account 一致）。
/// </summary>
public static class SailAuthIdentity
{
    public static string RequireAccount(ClaimsPrincipal? user)
    {
        var account = CurrentUser.GetAccount(user);
        if (string.IsNullOrWhiteSpace(account))
            throw new UnauthorizedAccessException("login account required");
        return account.Trim();
    }

    public static string? TryRealName(ClaimsPrincipal? user) =>
        user?.FindFirstValue("realName");

    /// <summary>覆盖 FlowCommandContext 操作人。</summary>
    public static void BindRunner(ClaimsPrincipal? user, FlowCommandContext? runner)
    {
        if (runner == null) return;
        var account = RequireAccount(user);
        runner.UserId = account;
        var realName = TryRealName(user);
        if (!string.IsNullOrWhiteSpace(realName))
            runner.UserName = realName;
        else if (string.IsNullOrWhiteSpace(runner.UserName))
            runner.UserName = account;
    }

    /// <summary>
    /// 「我的」待办/已办：仅当客户端传了过滤键时强制为当前 Account；
    /// 空 UserId 留给管理端全量查询。
    /// </summary>
    public static void BindTaskQueryMine(ClaimsPrincipal? user, TaskQuery? query)
    {
        if (query == null) return;
        var account = RequireAccount(user);
        if (!string.IsNullOrWhiteSpace(query.UserId))
            query.UserId = account;
        if (!string.IsNullOrWhiteSpace(query.EndedUserId))
            query.EndedUserId = account;
    }

    /// <summary>「我的流程」：传了 CreatedUserId 则强制当前 Account；空则管理端全量。</summary>
    public static void BindProcessInstanceMine(ClaimsPrincipal? user, ProcessInstanceQuery? query)
    {
        if (query == null) return;
        if (string.IsNullOrWhiteSpace(query.CreatedUserId)) return;
        query.CreatedUserId = RequireAccount(user);
    }

    public static void BindCreator(
        ClaimsPrincipal? user,
        out string creatorId,
        out string creatorName)
    {
        creatorId = RequireAccount(user);
        creatorName = TryRealName(user) ?? creatorId;
    }
}

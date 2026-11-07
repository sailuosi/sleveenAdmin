namespace SleveenAdmin.Saiflow.Utility;

/// <summary>§A：策略跳过/拒绝/超时专用办理人。可用真实系统账号覆盖 Active* 以摆脱魔法人。</summary>
public static class SailPolicyConstants
{
    public const string PolicySkipUserId = "__sail_policy_skip__";
    public const string PolicySkipUserName = "系统(策略跳过)";

    /// <summary>approveType=3 自动拒绝：作业扫到后 RejectProcess。</summary>
    public const string PolicyAutoRejectUserId = "__sail_policy_reject__";
    public const string PolicyAutoRejectUserName = "系统(策略拒绝)";

    public const string TimeoutSystemUserId = "__sail_timeout__";
    public const string TimeoutSystemUserName = "系统(超时策略)";

    /// <summary>
    /// 运行时生效的跳过占位人（默认魔法人）。
    /// 配置 <c>Sail:PolicySkip:UserId</c> 为真实 Account 后新建待办不再写魔法串。
    /// </summary>
    public static string ActiveSkipUserId { get; set; } = PolicySkipUserId;

    public static string ActiveSkipUserName { get; set; } = PolicySkipUserName;

    public static bool IsSkipUser(string? userId) =>
        !string.IsNullOrWhiteSpace(userId)
        && (string.Equals(userId, ActiveSkipUserId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(userId, PolicySkipUserId, StringComparison.OrdinalIgnoreCase));

    public static bool IsAutoRejectUser(string? userId) =>
        !string.IsNullOrWhiteSpace(userId)
        && string.Equals(userId, PolicyAutoRejectUserId, StringComparison.OrdinalIgnoreCase);
}

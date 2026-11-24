using sailflow.assignee;
using sailflow.utils;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// 遗留：曾用于 Filter 清 Performer + 打锚点。新产品路径由
/// <see cref="SailNodePolicyAutoApproveHook"/> 入队，本类型仅保留 AppendAnchor 供兼容调用。
/// </summary>
[Obsolete("Auto-approve enqueue moved to SailNodePolicyAutoApproveHook; do not clear performers.")]
internal static class NodePolicySkipIntent
{
    public static void Apply(AssigneeFilterContext ctx, string strategyCode, string? reason = null)
    {
        // 不再清空 Performer；仅打锚点（兼容旧调用方）
        AppendAnchor(ctx, strategyCode, reason);
    }

    public static void AppendAnchor(
        AssigneeFilterContext ctx,
        string strategyCode,
        string? reason = null)
    {
        var req = ctx.Request;
        if (req == null) return;

        if (ctx.Anchors.Any(a =>
                string.Equals(a.StrategyCode, strategyCode, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.ActivityId, req.ActivityId, StringComparison.OrdinalIgnoreCase)))
            return;

        var preferId = ctx.Performers?.FirstOrDefault()?.UserId
                       ?? SailPolicyConstants.ActiveSkipUserId;
        var preferName = ctx.Performers?.FirstOrDefault()?.UserName
                         ?? SailPolicyConstants.ActiveSkipUserName;

        ctx.Anchors.Add(new AutoApproveAnchor
        {
            StrategyCode = strategyCode,
            Reason = reason ?? "NodePolicy 策略自动通过",
            ProcessId = req.ProcessId,
            Version = req.Version,
            ProcessInstanceId = req.ProcessInstanceId,
            ActivityId = req.ActivityId,
            ActivityCode = req.ActivityCode,
            PreferUserId = preferId,
            PreferUserName = preferName
        });
    }
}

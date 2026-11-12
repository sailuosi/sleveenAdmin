using sailflow.assignee;
using sailflow.engine.Common;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>
/// 策略自动通过意图（R10）：只打 AutoApprove 锚点，不落占位办理人。
/// Mediator 见 ResultAutoApprovePending → 建活动不建待办；Job 走 RunProcessAuto。
/// </summary>
internal static class NodePolicySkipIntent
{
    public static void Apply(AssigneeFilterContext ctx, string strategyCode, string? reason = null)
    {
        ctx.Performers = new PerformerList();
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

        ctx.Anchors.Add(new AutoApproveAnchor
        {
            StrategyCode = strategyCode,
            Reason = reason ?? "NodePolicy 策略自动通过",
            ProcessId = req.ProcessId,
            Version = req.Version,
            ProcessInstanceId = req.ProcessInstanceId,
            ActivityId = req.ActivityId,
            ActivityCode = req.ActivityCode,
            // 审计用；无待办时 Job 用 RunProcessAuto + PreferUserId 作为 Runner.UserId
            PreferUserId = SailPolicyConstants.ActiveSkipUserId,
            PreferUserName = SailPolicyConstants.ActiveSkipUserName
        });
    }
}

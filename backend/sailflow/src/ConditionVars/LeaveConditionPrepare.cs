using sailflow.nextStep;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.ConditionVars;

/// <summary>请假自写表单：BizPK → Conditions["days"]。</summary>
public sealed class LeaveConditionPrepare : NextStepPrepare
{
    public override bool IsMatch(NextStepRouteContext ctx)
    {
        if (ctx?.Conditions == null || ctx.Conditions.ContainsKey("days"))
            return false;
        var channel = ctx.Runner?.BizChannel;
        if (!string.IsNullOrWhiteSpace(channel)
            && (string.Equals(channel, SailLeaveStatus.BizChannel, StringComparison.OrdinalIgnoreCase)
                || channel.Contains("leave", StringComparison.OrdinalIgnoreCase)))
            return true;
        return !string.IsNullOrWhiteSpace(ctx.ProcessCode)
               && ctx.ProcessCode.Contains("leave", StringComparison.OrdinalIgnoreCase);
    }

    protected override void Contribute(NextStepRouteContext ctx)
    {
        var bizPk = ctx.BizPK ?? ctx.Runner?.BizPK;
        if (string.IsNullOrWhiteSpace(bizPk) || ctx.Sail == null)
            return;

        var leave = new SailLeaveService(ctx.Sail);
        leave.EnsureTable();
        var row = leave.Get(bizPk);
        if (row == null)
            return;
        SetIfAbsent(ctx, "days", row.Days.ToString("0.##"));
    }
}

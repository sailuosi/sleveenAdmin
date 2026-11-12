using System.Diagnostics;
using sailflow.assignee;
using sailflow.engine.Common;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// approveType=2/3：有人后仍覆盖为策略跳过/自动拒绝（对齐原装饰器优先语义）。
/// </summary>
public sealed class NodePolicyApproveTypeFilter : IAssigneeResolveFilter
{
    public void Apply(AssigneeFilterContext ctx)
    {
        var approve = NodePolicyAssigneeSupport.GetInt(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagApproveType, 1);
        if (approve == 2)
        {
            if (string.Equals(ctx.Source, PerformerSources.Specific, StringComparison.OrdinalIgnoreCase))
                ctx.RewroteSpecific = true;
            NodePolicySkipIntent.Apply(ctx, "ApproveTypeSkip", "approveType=自动通过");
            Trace.TraceInformation(
                "[Assignee.Host] ApproveType=2 → policy skip + anchor; Activity={0}",
                ctx.Request?.ActivityId);
            return;
        }

        if (approve == 3)
        {
            if (string.Equals(ctx.Source, PerformerSources.Specific, StringComparison.OrdinalIgnoreCase))
                ctx.RewroteSpecific = true;
            ctx.Performers = new PerformerList
            {
                new Performer(
                    SailPolicyConstants.PolicyAutoRejectUserId,
                    SailPolicyConstants.PolicyAutoRejectUserName)
            };
            Trace.TraceInformation(
                "[Assignee.Host] ApproveType=3 → policy reject; Activity={0}",
                ctx.Request?.ActivityId);
        }
    }
}

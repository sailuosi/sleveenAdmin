using System.Diagnostics;
using sailflow.assignee;
using sailflow.engine.Common;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// approveType=3：有人后仍覆盖为策略拒绝人（JobWorker RejectProcess）。
/// approveType=2 自动通过改由 <see cref="SailNodePolicyAutoApproveHook"/> 入队，本 Filter 不再清人/打锚点。
/// </summary>
public sealed class NodePolicyApproveTypeFilter : IAssigneeResolveFilter
{
    public void Apply(AssigneeFilterContext ctx)
    {
        var approve = NodePolicyAssigneeSupport.GetInt(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagApproveType, 1);
        // approveType=2：保留办理人，由发送成功 Hook 入队 AutoApprove
        if (approve == 2)
            return;

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

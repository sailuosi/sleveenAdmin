using saiflow.assignee;
using saiflow.assignee.Builtin;
using saiflow.engine.Assignee;
using saiflow.engine.Common;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>空人=1：策略跳过占位 + AutoApprove 锚点。</summary>
public sealed class NodePolicySkipEmptyAction : IAssigneeEmptyAction
{
    public string Key => EmptyActionKeys.Skip;

    public EmptyActionResult Handle(AssigneeFilterContext ctx)
    {
        NodePolicySkipIntent.Apply(ctx, "EmptySkip", "空人策略：自动通过");
        return EmptyActionResult.WithPerformers(ctx.Performers);
    }
}

/// <summary>空人=3：指定人员（PolicyBag.userIds）；空则 AssignEmptyUsersMissing。</summary>
public sealed class NodePolicyUsersEmptyAction : IAssigneeEmptyAction
{
    public string Key => EmptyActionKeys.Users;

    public EmptyActionResult Handle(AssigneeFilterContext ctx)
    {
        var ids = NodePolicyAssigneeSupport.GetStringArray(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagUserIds);
        var list = new PerformerList();
        foreach (var id in ids)
            list.Add(new Performer(id, id));

        if (list.Count == 0)
        {
            return EmptyActionResult.Reject(
                AssigneeReasonCodes.AssignEmptyUsersMissing,
                "NodePolicy assignEmptyHandlerType=3 but assignEmptyUserIds is empty. ReasonCode="
                + AssigneeReasonCodes.AssignEmptyUsersMissing);
        }

        return EmptyActionResult.WithPerformers(list);
    }
}

/// <summary>空人=4：转流程管理员（PolicyBag.adminUserIds）。</summary>
public sealed class NodePolicyAdminEmptyAction : IAssigneeEmptyAction
{
    public string Key => EmptyActionKeys.Admin;

    public EmptyActionResult Handle(AssigneeFilterContext ctx)
    {
        var admins = NodePolicyAssigneeSupport.GetStringArray(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagAdminUserIds);
        var list = new PerformerList();
        foreach (var id in admins)
            list.Add(new Performer(id, id));

        if (list.Count == 0)
        {
            return EmptyActionResult.Reject(
                AssigneeReasonCodes.ProcessAdminNotConfigured,
                "NodePolicy assignEmptyHandlerType=4 but Sail:ProcessAdminUserIds is empty. ReasonCode="
                + AssigneeReasonCodes.ProcessAdminNotConfigured);
        }

        return EmptyActionResult.WithPerformers(list);
    }
}

/// <summary>approveType=3：落策略拒绝人（JobWorker RejectProcess）。</summary>
public sealed class NodePolicyAutoRejectEmptyAction : IAssigneeEmptyAction
{
    public string Key => NodePolicyEmptyActionKeys.AutoReject;

    public EmptyActionResult Handle(AssigneeFilterContext ctx)
    {
        var list = new PerformerList
        {
            new Performer(
                SailPolicyConstants.PolicyAutoRejectUserId,
                SailPolicyConstants.PolicyAutoRejectUserName)
        };
        return EmptyActionResult.WithPerformers(list);
    }
}

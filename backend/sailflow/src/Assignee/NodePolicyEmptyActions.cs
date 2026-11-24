using sailflow.assignee;
using sailflow.assignee.Builtin;
using sailflow.utils;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// 空人=1：写入策略跳过人作为办理人（留痕迹主体）；自动通过由发送成功 Hook 入队。
/// </summary>
public sealed class NodePolicySkipEmptyAction : IAssigneeEmptyAction
{
    public string Key => EmptyActionKeys.Skip;

    public EmptyActionResult Handle(AssigneeFilterContext ctx)
    {
        var list = new PerformerList
        {
            new Performer(
                SailPolicyConstants.ActiveSkipUserId,
                SailPolicyConstants.ActiveSkipUserName)
        };
        return EmptyActionResult.WithPerformers(list);
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

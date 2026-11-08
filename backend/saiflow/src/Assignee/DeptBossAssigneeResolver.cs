using saiflow.assignee;
using saiflow.assignee.Builtin;
using saiflow.engine.Client;
using saiflow.engine.Common;
using saiflow.engine.Service;
using SleveenAdmin.Foundation.Data;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>
/// 部门领导定范围 Type=<c>deptBoss</c>。Params.roleIds 优先；否则用宿主默认负责人角色袋。
/// 按发起人组织链找人（算法见 <see cref="DeptLeaderLookup"/>）。
/// </summary>
public class DeptBossAssigneeResolver : IAssigneeResolver
{
    public const string TypeCode = "deptBoss";

    readonly string[] _defaultLeaderRoleIds;
    readonly int _masterDbPosition;

    public SailClient? Sail { get; set; }

    public DeptBossAssigneeResolver(
        IEnumerable<string>? defaultLeaderRoleIds = null,
        int masterDbPosition = MooSqlSetup.DefaultPosition)
    {
        _defaultLeaderRoleIds = (defaultLeaderRoleIds ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        _masterDbPosition = masterDbPosition;
    }

    public virtual string Type => TypeCode;

    public virtual PerformerList Resolve(AssigneeResolveContext ctx)
    {
        var list = new PerformerList();
        var starter = ResolveStarterAccount(ctx);
        if (string.IsNullOrWhiteSpace(starter))
            return list;

        var roleIds = ConfigPairParams.GetStringArray(ctx.Config, "roleIds");
        if (roleIds.Length == 0)
            roleIds = _defaultLeaderRoleIds;

        var leaders = new DeptLeaderLookup(roleIds, _masterDbPosition).FindLeaders(starter);
        if (leaders.Count == 0)
            return list;

        var names = SleveenUserIdentity.LoadActiveAccounts();
        foreach (var id in leaders)
        {
            var name = names.TryGetValue(id, out var n) ? n : id;
            list.Add(new Performer(id, name) { GroupRole = GroupRoles.Leader });
        }

        return list;
    }

    protected virtual string? ResolveStarterAccount(AssigneeResolveContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Request?.ProcessInstanceId) || Sail == null)
            return null;
        try
        {
            var starter = new WorkflowService(Sail).GetProcessInitiator(ctx.Request.ProcessInstanceId);
            return string.IsNullOrWhiteSpace(starter?.UserId) ? null : starter!.UserId.Trim();
        }
        catch
        {
            return null;
        }
    }
}

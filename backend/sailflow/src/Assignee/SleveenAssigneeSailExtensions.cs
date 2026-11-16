using Microsoft.Extensions.Configuration;
using sailflow.assignee;
using sailflow.assignee.Hosting;
using sailflow.assignee.Store;
using sailflow.engine.Client;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>宿主侧办理人 Lookup 覆盖（R3）+ NodePolicy 空人/同人（R10 Filter/Empty）。</summary>
public static class SleveenAssigneeSailExtensions
{
    /// <summary>
    /// R3：UnitMap / JobRelative / RoleGroup / deptBoss 覆盖；身份键统一 Account。
    /// 组织人员契约具化见 <c>UseSleveenOrgResource</c>（与 Assignee Lookup 分离）。
    /// </summary>
    public static SailClient UseSleveenOrgLookups(
        this SailClient client,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(client);

        client.useAssigneeResolver(new SleveenUnitMapAssigneeResolver());
        client.useAssigneeResolver(new SleveenJobRelativeAssigneeResolver());
        client.useAssigneeResolver(new SleveenRoleGroupAssigneeResolver());

        var leaderRaw = configuration?["Sail:DeptLeaderRoleIds"]
                        ?? configuration?["Sail:SameAsStarter:LeaderRoleIds"]
                        ?? "";
        client.useAssigneeResolver(
            new DeptBossAssigneeResolver(SplitIds(leaderRaw), MooSqlSetup.DefaultPosition)
            {
                Sail = client
            });
        return client;
    }

    /// <summary>
    /// 实例预选办理人：注册 Type=<c>Preselect</c> + 宿主表 <c>sf_preselect_assignee</c>。
    /// 须在 <see cref="UseSleveenOrgLookups"/> / UseBizAssignees 之后调用。
    /// </summary>
    public static SailClient UseSleveenPreselectAssignees(
        this SailClient client,
        SleveenPreselectAssigneeStore store)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(store);
        return client.UsePreselectAssignees(store);
    }

    /// <summary>
    /// §A / R10：NodePolicy → EmptyActionSelector + Filter。
    /// 空人=1 写入策略人；同人=2 / approveType=2 保留办理人，由
    /// <c>UseNodePolicyAutoApproveHook</c> 在发送成功后入队 AutoApprove。
    /// 同人=3：读 assignStartResolveType → 主管道 Registry（如 deptBoss）。
    /// <list type="bullet">
    /// <item><c>Sail:ProcessAdminUserIds</c> — 空人=4</item>
    /// <item><c>Sail:DeptLeaderRoleIds</c> — deptBoss 默认负责人角色袋</item>
    /// <item><c>Sail:PolicySkip:UserId</c> — 空人跳过办理人 / Job 审计身份</item>
    /// </list>
    /// </summary>
    public static SailClient UseSleveenNodePolicyAssignee(
        this SailClient client,
        IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        if (client.AssigneePipeline is not AssigneePipeline pipeline)
        {
            throw new InvalidOperationException(
                "AssigneePipeline must be AssigneePipeline. Call UseBizAssignees() before UseSleveenNodePolicyAssignee().");
        }

        ApplyPolicySkipIdentity(configuration);

        var adminRaw = configuration?["Sail:ProcessAdminUserIds"]
                       ?? configuration?["Sail:ProcessAdmin:UserIds"]
                       ?? "";
        var adminIds = SplitIds(adminRaw);

        // 若 OrgLookups 未带 configuration，此处再覆盖一次 deptBoss 默认角色袋
        var leaderRaw = configuration?["Sail:DeptLeaderRoleIds"]
                        ?? configuration?["Sail:SameAsStarter:LeaderRoleIds"]
                        ?? "";
        client.useAssigneeResolver(
            new DeptBossAssigneeResolver(SplitIds(leaderRaw), MooSqlSetup.DefaultPosition)
            {
                Sail = client
            });

        pipeline.PolicyBagReader = ctx =>
        {
            var policy = NodePolicyAssigneeSupport.LoadPolicy(client, ctx.Request);
            return NodePolicyAssigneeSupport.BuildPolicyBag(policy, adminIds);
        };

        pipeline.EmptyActionSelector = ctx =>
            NodePolicyAssigneeSupport.SelectEmptyActionKey(ctx.PolicyBag);

        pipeline.RegisterEmptyAction(new NodePolicySkipEmptyAction());
        pipeline.RegisterEmptyAction(new NodePolicyUsersEmptyAction());
        pipeline.RegisterEmptyAction(new NodePolicyAdminEmptyAction());
        pipeline.RegisterEmptyAction(new NodePolicyAutoRejectEmptyAction());

        pipeline.AddFilter(new NodePolicyApproveTypeFilter(), order: 0);
        pipeline.AddFilter(new SameAsStarterFilter(client), order: 10);
        // 委托代批：业务 Filter 之后；再经 Dedup 清重复 UserId
        pipeline.AddFilter(new SurrogateAssigneeFilter(client), order: 50);
        pipeline.AddFilter(new sailflow.assignee.Builtin.DedupUsersFilter(), order: 60);

        pipeline.AnchorStore ??= new HookOutboxAutoApproveAnchorStore();

        return client;
    }

    /// <summary>
    /// 可选：另起引擎侧 AutoApprove Drain。默认由 JobWorker 消费，一般不必再开（双开会浪费 reclaim）。
    /// </summary>
    public static SailClient UseSleveenAssigneeAutoApproveJob(
        this SailClient client,
        Action<sailflow.assignee.Job.AutoApproveAnchorOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        System.Diagnostics.Trace.TraceWarning(
            "[Assignee.Host] UseSleveenAssigneeAutoApproveJob: Host SailNodePolicyJobWorker already drains "
            + "AssigneeAutoApprove. Prefer 单消费者；仅在未挂 JobWorker 时启用 AutoStart.");
        return client.UseAssign(a =>
        {
            a.UseAutoApproveJob(o =>
            {
                o.AutoStart = true;
                configure?.Invoke(o);
            });
        });
    }

    static void ApplyPolicySkipIdentity(IConfiguration? configuration)
    {
        var uid = configuration?["Sail:PolicySkip:UserId"]?.Trim();
        var uname = configuration?["Sail:PolicySkip:UserName"]?.Trim();
        if (!string.IsNullOrWhiteSpace(uid))
            SailPolicyConstants.ActiveSkipUserId = uid!;
        if (!string.IsNullOrWhiteSpace(uname))
            SailPolicyConstants.ActiveSkipUserName = uname!;
        else if (!string.IsNullOrWhiteSpace(uid)
                 && !string.Equals(uid, SailPolicyConstants.PolicySkipUserId, StringComparison.OrdinalIgnoreCase))
            SailPolicyConstants.ActiveSkipUserName = uid!;

        var legacy = configuration?["Sail:PolicySkip:LegacyDrain"]?.Trim();
        if (string.Equals(legacy, "true", StringComparison.OrdinalIgnoreCase)
            || legacy == "1")
            SailPolicyConstants.LegacySkipDrainEnabled = true;
    }

    static string[] SplitIds(string raw) =>
        raw.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}

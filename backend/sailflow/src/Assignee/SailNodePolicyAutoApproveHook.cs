using mooSQL.data;
using sailflow.assignee;
using sailflow.assignee.Store;
using sailflow.data;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Entity;
using sailflow.engine.Event;
using sailflow.engine.Hooks;
using sailflow.engine.Logging;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// 发送成功后按 NodePolicy 入队 AutoApprove（同人/空人/approveType）。
/// 仅订阅 Started/Continued/Resent，自然排除 SendBack 等 OA 原语。禁止同步 RunProcess。
/// </summary>
public sealed class SailNodePolicyAutoApproveHook(SailClient sail) : FlowHookBase
{
    public const string CommentSameAsStarter = "同人策略：自动通过";
    public const string CommentEmptySkip = "空人策略：自动通过";
    public const string CommentApproveType = "节点策略：自动通过";

    public override string Name => "sail.nodepolicy.autoapprove";
    public override string Title => "节点策略自动通过入队";

    public override void Handle(FlowHookContext ctx)
    {
        if (ctx.EventType is not (
            EventFireTypeEnum.OnProcessStarted
            or EventFireTypeEnum.OnProcessContinued
            or EventFireTypeEnum.OnProcessResent))
            return;

        if (string.IsNullOrWhiteSpace(ctx.ProcessInstanceId))
            return;

        try
        {
            EnqueueForReadyWaitingTasks(ctx);
        }
        catch (Exception ex)
        {
            sail.Log?.Warning(
                SailLogCategories.SyncHook,
                "name={0} PI={1}: {2}",
                Name, ctx.ProcessInstanceId, ex.Message);
            /* 入队失败不阻断主发送 */
        }
    }

    void EnqueueForReadyWaitingTasks(FlowHookContext ctx)
    {
        var kit = ctx.Kit ?? sail.UseRunSQL();
        var tasks = LoadReadyWaitingTasks(kit, ctx.ProcessInstanceId, ctx.ActivityInstanceId);
        if (tasks.Count == 0)
            return;

        string? starterId = null;
        var byActivity = tasks.GroupBy(t => t.ActivityId ?? "", StringComparer.OrdinalIgnoreCase);
        var anchors = new List<AutoApproveAnchor>();

        foreach (var group in byActivity)
        {
            var sample = group.First();
            var policy = LoadPolicy(kit, ctx, sample);
            if (policy == null)
                continue;

            var list = group.ToList();
            string? strategy = null;
            string? reason = null;

            if (policy.ApproveType == 2)
            {
                strategy = "ApproveTypeSkip";
                reason = CommentApproveType;
            }
            else if (policy.AssignStartUserHandlerType == 2)
            {
                starterId ??= ResolveStarterId(kit, ctx.ProcessInstanceId);
                if (!string.IsNullOrWhiteSpace(starterId)
                    && list.All(t =>
                        string.Equals(t.SignId, starterId, StringComparison.OrdinalIgnoreCase)))
                {
                    strategy = "SameAsStarterSkip";
                    reason = CommentSameAsStarter;
                }
            }

            if (strategy == null
                && policy.AssignEmptyHandlerType == 1
                && list.All(t => SailPolicyConstants.IsSkipUser(t.SignId)))
            {
                strategy = "EmptySkip";
                reason = CommentEmptySkip;
            }

            if (strategy == null)
                continue;

            foreach (var task in list)
            {
                if (string.IsNullOrWhiteSpace(task.TaskId))
                    continue;

                anchors.Add(new AutoApproveAnchor
                {
                    StrategyCode = strategy,
                    Reason = reason,
                    ProcessId = task.ProcessId ?? ctx.ProcessId,
                    Version = ctx.Version,
                    ProcessInstanceId = task.ProcessInstanceId ?? ctx.ProcessInstanceId,
                    ActivityId = task.ActivityId,
                    ActivityCode = ctx.ActivityCode,
                    ActivityInstanceId = task.ActivityInstanceId,
                    TaskId = task.TaskId,
                    PreferUserId = task.SignId,
                    PreferUserName = task.SignName
                });
            }
        }

        if (anchors.Count == 0)
            return;

        new HookOutboxAutoApproveAnchorStore().Enqueue(sail, anchors, ctx.Kit);
        sail.Log?.Message(
            SailLogCategories.SyncHook,
            "name={0} enqueued {1} anchor(s) PI={2} Event={3}",
            Name, anchors.Count, ctx.ProcessInstanceId, ctx.EventType);
    }

    static List<TaskEntity> LoadReadyWaitingTasks(
        SQLBuilder kit,
        string processInstanceId,
        string? excludeActivityInstanceId)
    {
        var clip = kit.useClip();
        clip.from<TaskEntity>(out var t)
            .where(() => t.ProcessInstanceId, processInstanceId)
            .where(() => t.TaskState, (short)TaskStateEnum.Waiting);
        var waiting = clip.select(t).queryList()?.ToList() ?? new List<TaskEntity>();
        if (waiting.Count == 0)
            return waiting;

        var readyIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var aiClip = kit.useClip();
        aiClip.from<ActivityInstanceEntity>(out var a)
            .where(() => a.ProcessInstanceId, processInstanceId)
            .where(() => a.ActivityState, (short)ActivityStateEnum.Ready);
        foreach (var ai in aiClip.select(a).queryList() ?? Array.Empty<ActivityInstanceEntity>())
        {
            if (!string.IsNullOrWhiteSpace(ai.ActivityInstanceId))
                readyIds.Add(ai.ActivityInstanceId);
        }

        return waiting
            .Where(task =>
                !string.IsNullOrWhiteSpace(task.ActivityInstanceId)
                && readyIds.Contains(task.ActivityInstanceId)
                && (string.IsNullOrWhiteSpace(excludeActivityInstanceId)
                    || !string.Equals(
                        task.ActivityInstanceId,
                        excludeActivityInstanceId,
                        StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    SailNodePolicyDto? LoadPolicy(SQLBuilder kit, FlowHookContext ctx, TaskEntity task)
    {
        var processId = task.ProcessId ?? ctx.ProcessId;
        var activityId = task.ActivityId;
        if (string.IsNullOrWhiteSpace(processId) || string.IsNullOrWhiteSpace(activityId))
            return null;

        var version = ctx.Version;
        if (string.IsNullOrWhiteSpace(version) && !string.IsNullOrWhiteSpace(ctx.ProcessInstanceId))
        {
            var pi = kit.useRepo<ProcessInstanceEntity, string>().GetById(ctx.ProcessInstanceId);
            version = pi?.Version;
        }

        if (string.IsNullOrWhiteSpace(version))
            version = "1";

        var file = new WorkflowService(sail).GetProcessFile(processId, version);
        return SailNodePolicyCatalog.ParseForActivity(file?.XmlContent, activityId);
    }

    static string? ResolveStarterId(SQLBuilder kit, string processInstanceId)
    {
        var pi = kit.useRepo<ProcessInstanceEntity, string>().GetById(processInstanceId);
        return string.IsNullOrWhiteSpace(pi?.CreatedUserId) ? null : pi!.CreatedUserId;
    }
}

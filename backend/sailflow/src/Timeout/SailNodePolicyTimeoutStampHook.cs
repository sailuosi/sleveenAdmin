using mooSQL.data;
using sailflow.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Event;
using sailflow.engine.Hooks;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Timeout;

/// <summary>
/// §A：可办时刻按 NodePolicy 写入待办 <c>ActivatedDateTime</c> / <c>OverdueDateTime</c>
///（及对应活动实例 Overdue；与 Boundaries 取更严）。
/// Sync 时必须用 <see cref="FlowHookContext.Kit"/> 同事务，禁止另开连接。
/// </summary>
public sealed class SailNodePolicyTimeoutStampHook(SailClient sail) : IFlowHook
{
    public string Name => "sail.nodepolicy.timeout.stamp";

    public void Handle(FlowHookContext ctx)
    {
        if (ctx.EventType is not (
            EventFireTypeEnum.OnActivityCreated
            or EventFireTypeEnum.OnActivityReady
            or EventFireTypeEnum.OnTaskCreated))
            return;
        if (string.IsNullOrWhiteSpace(ctx.ProcessId))
            return;

        try
        {
            if (ctx.EventType == EventFireTypeEnum.OnTaskCreated)
                StampOnTaskCreated(ctx);
            else
                StampOnActivity(ctx);
        }
        catch
        {
            /* 打戳失败不阻断流转 */
        }
    }

    /// <summary>优先用流转事务 kit；无 kit（如 Outbox 回放）再开连接。</summary>
    SQLBuilder ResolveKit(FlowHookContext ctx) => ctx.Kit ?? sail.UseRunSQL();

    void StampOnActivity(FlowHookContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.ActivityInstanceId) || string.IsNullOrWhiteSpace(ctx.ActivityId))
            return;

        var kit = ResolveKit(ctx);
        var policy = LoadPolicy(kit, ctx);
        if (policy == null || !policy.TimeoutHandlerEnable)
            return;

        var inst = kit.useRepo<ActivityInstanceEntity, string>().GetById(ctx.ActivityInstanceId);
        if (inst == null) return;

        var isReady = inst.ActivityState == (short)ActivityStateEnum.Ready;
        if (ctx.EventType == EventFireTypeEnum.OnActivityCreated && !isReady)
            return; // 串行挂起子节点：不写 Activated/Due
        if (!isReady && ctx.EventType != EventFireTypeEnum.OnActivityReady)
            return;

        var activated = ctx.EventType == EventFireTypeEnum.OnActivityReady
            ? DateTime.UtcNow
            : (inst.CreatedDateTime ?? DateTime.UtcNow);
        var due = SailNodePolicyTimeout.ComputeOverdue(activated, policy);
        if (due == null) return;

        ApplyActivityOverdue(kit, inst, due.Value);
        StampWaitingTasks(kit, ctx.ActivityInstanceId, activated, due.Value);
    }

    void StampOnTaskCreated(FlowHookContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.TaskId))
            return;

        var kit = ResolveKit(ctx);
        var task = kit.useRepo<TaskEntity, string>().GetById(ctx.TaskId);
        if (task == null || string.IsNullOrWhiteSpace(task.ActivityInstanceId))
            return;

        var inst = kit.useRepo<ActivityInstanceEntity, string>().GetById(task.ActivityInstanceId);
        if (inst == null) return;
        if (inst.ActivityState != (short)ActivityStateEnum.Ready)
            return; // 未可办：Activated/Due 保持 null

        var policy = LoadPolicy(kit, ctx, task.ProcessInstanceId, task.ActivityId, task.ProcessId);
        if (policy == null || !policy.TimeoutHandlerEnable)
            return;

        // 转办/委托新人：从创建时刻起算；普通创建且 AI 已 Ready 亦同
        var activated = DateTime.UtcNow;
        var due = SailNodePolicyTimeout.ComputeOverdue(activated, policy);
        if (due == null) return;

        task.ActivatedDateTime = activated;
        task.OverdueDateTime = PickEarlier(task.OverdueDateTime, due.Value);
        kit.useRepo<TaskEntity>().Update(task);
        ApplyActivityOverdue(kit, inst, due.Value);
    }

    SailNodePolicyDto? LoadPolicy(
        SQLBuilder kit,
        FlowHookContext ctx,
        string? processInstanceId = null,
        string? activityId = null,
        string? processId = null)
    {
        processId ??= ctx.ProcessId;
        activityId ??= ctx.ActivityId;
        processInstanceId ??= ctx.ProcessInstanceId;
        if (string.IsNullOrWhiteSpace(processId) || string.IsNullOrWhiteSpace(activityId))
            return null;

        var version = ctx.Version;
        if (string.IsNullOrWhiteSpace(version) && !string.IsNullOrWhiteSpace(processInstanceId))
        {
            // 同事务读未提交的流程实例，禁止另开连接
            var pi = kit.useRepo<ProcessInstanceEntity, string>().GetById(processInstanceId);
            version = pi?.Version;
        }
        if (string.IsNullOrWhiteSpace(version))
            version = "1";

        // 流程定义文件已提交，可用独立连接
        var file = new WorkflowService(sail).GetProcessFile(processId, version);
        return SailNodePolicyCatalog.ParseForActivity(file?.XmlContent, activityId);
    }

    static void ApplyActivityOverdue(SQLBuilder kit, ActivityInstanceEntity inst, DateTime due)
    {
        // 已有 Boundaries 逾期时：取更早的截止（更严）
        if (inst.OverdueDateTime.HasValue && inst.OverdueDateTime.Value < due)
            return;

        inst.OverdueDateTime = due;
        kit.useRepo<ActivityInstanceEntity>().Update(inst);
    }

    static DateTime PickEarlier(DateTime? existing, DateTime candidate) =>
        existing.HasValue && existing.Value < candidate ? existing.Value : candidate;

    void StampWaitingTasks(SQLBuilder kit, string activityInstanceId, DateTime activated, DateTime due)
    {
        var clip = kit.useClip();
        clip.from<TaskEntity>(out var t);
        clip.where(() => t.ActivityInstanceId, activityInstanceId);
        clip.where(() => t.TaskState, (short)TaskStateEnum.Waiting);
        var tasks = clip.select(t).queryList() ?? new List<TaskEntity>();
        var taskRepo = kit.useRepo<TaskEntity>();
        foreach (var task in tasks)
        {
            task.ActivatedDateTime = activated;
            task.OverdueDateTime = PickEarlier(task.OverdueDateTime, due);
            taskRepo.Update(task);
        }
    }
}

using mooSQL.data;
using sailflow.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Event;
using sailflow.engine.Hooks;
using sailflow.engine.Service;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Timeout;

/// <summary>
/// §A：可办时刻按 NodePolicy 写入待办 <c>ActivatedDateTime</c> / <c>OverdueDateTime</c>
///（及对应活动实例 Overdue；与 Boundaries 取更严）。
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

    void StampOnActivity(FlowHookContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.ActivityInstanceId) || string.IsNullOrWhiteSpace(ctx.ActivityId))
            return;

        var policy = LoadPolicy(ctx.ProcessId, ctx.ProcessInstanceId, ctx.ActivityId);
        if (policy == null || !policy.TimeoutHandlerEnable)
            return;

        var kit = sail.UseRunSQL();
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

        var kit = sail.UseRunSQL();
        var task = kit.useRepo<TaskEntity, string>().GetById(ctx.TaskId);
        if (task == null || string.IsNullOrWhiteSpace(task.ActivityInstanceId))
            return;

        var inst = kit.useRepo<ActivityInstanceEntity, string>().GetById(task.ActivityInstanceId);
        if (inst == null) return;
        if (inst.ActivityState != (short)ActivityStateEnum.Ready)
            return; // 未可办：Activated/Due 保持 null

        var activityId = string.IsNullOrWhiteSpace(ctx.ActivityId) ? task.ActivityId : ctx.ActivityId;
        var processId = string.IsNullOrWhiteSpace(ctx.ProcessId) ? task.ProcessId : ctx.ProcessId;
        var policy = LoadPolicy(processId!, task.ProcessInstanceId, activityId!);
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

    SailNodePolicyDto? LoadPolicy(string processId, string? processInstanceId, string activityId)
    {
        var wf = new WorkflowService(sail);
        var pi = !string.IsNullOrWhiteSpace(processInstanceId)
            ? wf.GetProcessInstance(processInstanceId)
            : null;
        var version = string.IsNullOrWhiteSpace(pi?.Version) ? "1" : pi!.Version;
        var file = wf.GetProcessFile(processId, version);
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

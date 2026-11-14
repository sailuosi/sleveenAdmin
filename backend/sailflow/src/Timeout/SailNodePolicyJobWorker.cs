using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mooSQL.data;
using sailflow.assignee.Job;
using sailflow.data;
using sailflow.engine.Entity;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Result;
using sailflow.engine.Hooks;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Timeout;

/// <summary>
/// §A Host 作业：AutoApprove 锚点代办 / 自动拒绝 / 超时扫描（提醒走 TaskNotifier）。
/// 策略跳过主路径：Outbox Kind=AssigneeAutoApprove；遗留无锚点的 skip 待办仍兜底扫一次。
/// </summary>
public sealed class SailNodePolicyJobWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<SailNodePolicyJobWorker> logger) : BackgroundService
{
    static readonly ConcurrentDictionary<string, int> RemindCounts = new(StringComparer.Ordinal);
    static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Sail NodePolicy job worker started (autoApprove/reject + timeout)");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var sail = scope.ServiceProvider.GetRequiredService<SailClient>();
                var wf = scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                await DrainAutoApproveAnchorsAsync(sail, stoppingToken);
                DrainLegacyPolicySkips(sail, wf, stoppingToken);
                DrainPolicyAutoRejects(sail, wf, stoppingToken);
                DrainTimeouts(sail, wf, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "NodePolicy job tick failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>R10：消费 AssigneeAutoApprove Outbox（与发送同事务入队的锚点）。</summary>
    async Task DrainAutoApproveAnchorsAsync(SailClient sail, CancellationToken ct)
    {
        try
        {
            var worker = new AutoApproveAnchorWorker(sail);
            var n = await worker.ProcessPendingAsync(cancellationToken: ct).ConfigureAwait(false);
            if (n > 0)
                logger.LogDebug("AutoApprove anchors processed={Count}", n);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "AutoApprove drain failed");
        }
    }

    /// <summary>
    /// 遗留兜底：无锚点仍 Waiting 的策略跳过待办（魔法人或 ActiveSkip）。
    /// </summary>
    void DrainLegacyPolicySkips(SailClient sail, IWorkflowService wf, CancellationToken ct)
    {
        List<TaskEntity> tasks;
        try
        {
            var skipIds = new[]
                {
                    SailPolicyConstants.ActiveSkipUserId,
                    SailPolicyConstants.PolicySkipUserId
                }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            tasks = new List<TaskEntity>();
            foreach (var skipId in skipIds)
            {
                var clip = sail.UseRunSQL().useClip();
                clip.from<TaskEntity>(out var t);
                clip.where(() => t.AssignedUserId, skipId);
                clip.where(() => t.TaskState, (short)TaskStateEnum.Waiting);
                var batch = clip.select(t).queryList() ?? new List<TaskEntity>();
                tasks.AddRange(batch);
            }

            tasks = tasks
                .GroupBy(x => x.TaskId)
                .Select(g => g.First())
                .Take(20)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "query legacy policy-skip tasks failed");
            return;
        }

        foreach (var task in tasks)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(task.TaskId)) continue;
            try
            {
                var uid = string.IsNullOrWhiteSpace(task.AssignedUserId)
                    ? SailPolicyConstants.ActiveSkipUserId
                    : task.AssignedUserId;
                var uname = string.IsNullOrWhiteSpace(task.AssignedUserName)
                    ? SailPolicyConstants.ActiveSkipUserName
                    : task.AssignedUserName;
                var runner = BuildRunnerFromTask(sail, task, uid!, uname!);
                runner.DynamicVariables ??= new Dictionary<string, string>();
                runner.DynamicVariables["sail.policySkip"] = "1";
                runner.DynamicVariables["sail.comment"] = "策略自动通过（legacy skip 兜底）";
                // Job 路径用同步 API，避免伪 async 线程池切换（草案阶段 0）
                var exec = wf.RunProcess(runner);
                if (exec.Status != WfExecutedStatus.Success)
                    logger.LogWarning("LegacyPolicySkip Run failed task={TaskId}: {Msg}", task.TaskId, exec.Message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "LegacyPolicySkip Run exception task={TaskId}", task.TaskId);
            }
        }
    }

    void DrainPolicyAutoRejects(SailClient sail, IWorkflowService wf, CancellationToken ct)
    {
        List<TaskEntity> tasks;
        try
        {
            var clip = sail.UseRunSQL().useClip();
            clip.from<TaskEntity>(out var t);
            clip.where(() => t.AssignedUserId, SailPolicyConstants.PolicyAutoRejectUserId);
            clip.where(() => t.TaskState, (short)TaskStateEnum.Waiting);
            tasks = clip.select(t).queryList().Take(20).ToList();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "query policy-reject tasks failed");
            return;
        }

        foreach (var task in tasks)
        {
            ct.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(task.TaskId)) continue;
            try
            {
                var runner = BuildRunnerFromTask(sail, task,
                    SailPolicyConstants.PolicyAutoRejectUserId,
                    SailPolicyConstants.PolicyAutoRejectUserName);
                runner.DynamicVariables ??= new Dictionary<string, string>();
                runner.DynamicVariables["sail.policyReject"] = "1";
                runner.DynamicVariables["sail.comment"] = "approveType=自动拒绝";
                var exec = wf.RejectProcess(runner);
                if (exec.Status != WfExecutedStatus.Success)
                    logger.LogWarning("PolicyReject failed task={TaskId}: {Msg}", task.TaskId, exec.Message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "PolicyReject exception task={TaskId}", task.TaskId);
            }
        }
    }

    void DrainTimeouts(SailClient sail, IWorkflowService wf, CancellationToken ct)
    {
        List<TaskEntity> dueTasks;
        try
        {
            var now = DateTime.UtcNow;
            var clip = sail.UseRunSQL().useClip();
            clip.from<TaskEntity>(out var t);
            clip.where(() => t.TaskState, (short)TaskStateEnum.Waiting);
            var waiting = clip.select(t).queryList() ?? new List<TaskEntity>();
            // 主路径：已落库的 task.overdue_datetime；旧行无 Due 时回退 Created+策略（Handle 内）
            dueTasks = waiting
                .Where(x => !string.IsNullOrWhiteSpace(x.TaskId))
                .Where(x => x.OverdueDateTime.HasValue && ToUtc(x.OverdueDateTime.Value) <= now)
                .Take(20)
                .ToList();

            // 兼容旧数据：Waiting 无 Due 的少量行，仍交给按活动策略回退
            if (dueTasks.Count < 20)
            {
                var legacy = waiting
                    .Where(x => !string.IsNullOrWhiteSpace(x.TaskId) && !x.OverdueDateTime.HasValue)
                    .Take(20 - dueTasks.Count)
                    .ToList();
                dueTasks.AddRange(legacy);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "query overdue tasks failed");
            return;
        }

        foreach (var task in dueTasks)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                HandleOverdueTask(sail, wf, task);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "timeout handle failed task={Id}", task.TaskId);
            }
        }
    }

    void HandleOverdueTask(SailClient sail, IWorkflowService wf, TaskEntity task)
    {
        if (string.IsNullOrWhiteSpace(task.ActivityInstanceId))
            return;

        var act = sail.UseRunSQL().useRepo<ActivityInstanceEntity, string>()
            .GetById(task.ActivityInstanceId);
        if (act == null || act.ActivityState != (short)ActivityStateEnum.Ready)
            return;

        var pi = string.IsNullOrWhiteSpace(act.ProcessInstanceId)
            ? null
            : wf.GetProcessInstance(act.ProcessInstanceId);
        var version = string.IsNullOrWhiteSpace(pi?.Version) ? "1" : pi!.Version;
        var file = wf.GetProcessFile(act.ProcessId, version);
        var policy = SailNodePolicyCatalog.ParseForActivity(file?.XmlContent, act.ActivityId);
        if (!policy.TimeoutHandlerEnable)
            return;

        // 有 Due 只信字段；无 Due 旧行才回退 Created+span
        if (!task.OverdueDateTime.HasValue)
        {
            var span = SailNodePolicyTimeout.ToTimeSpan(policy.TimeDuration, policy.TimeUnit);
            if (span == null || !IsLegacyTaskDue(task, act, span.Value))
                return;
        }
        else if (ToUtc(task.OverdueDateTime.Value) > DateTime.UtcNow)
        {
            return;
        }

        switch (policy.TimeoutHandlerType)
        {
            case 1: // 提醒
                Remind(sail, act, new List<TaskEntity> { task }, policy);
                break;
            case 2: // 自动同意
            {
                var runner = BuildRunnerFromTask(sail, task,
                    SailPolicyConstants.TimeoutSystemUserId,
                    SailPolicyConstants.TimeoutSystemUserName);
                runner.DynamicVariables ??= new Dictionary<string, string>();
                runner.DynamicVariables["sail.timeout"] = "agree";
                runner.DynamicVariables["sail.comment"] = "超时策略：自动同意";
                runner.UserId = task.AssignedUserId;
                runner.UserName = string.IsNullOrWhiteSpace(task.AssignedUserName)
                    ? SailPolicyConstants.TimeoutSystemUserName
                    : task.AssignedUserName;
                var exec = wf.RunProcess(runner);
                if (exec.Status != WfExecutedStatus.Success)
                    logger.LogWarning("Timeout agree failed task={Id}: {Msg}", task.TaskId, exec.Message);
                break;
            }
            case 3: // 自动拒绝
            {
                var runner = BuildRunnerFromTask(sail, task,
                    task.AssignedUserId,
                    task.AssignedUserName ?? SailPolicyConstants.TimeoutSystemUserName);
                runner.DynamicVariables ??= new Dictionary<string, string>();
                runner.DynamicVariables["sail.timeout"] = "reject";
                runner.DynamicVariables["sail.comment"] = "超时策略：自动拒绝";
                var exec = wf.RejectProcess(runner);
                if (exec.Status != WfExecutedStatus.Success)
                    logger.LogWarning("Timeout reject failed task={Id}: {Msg}", task.TaskId, exec.Message);
                break;
            }
        }
    }

    static bool IsLegacyTaskDue(TaskEntity task, ActivityInstanceEntity act, TimeSpan span)
    {
        var created = task.ActivatedDateTime ?? task.CreatedDateTime ?? act.CreatedDateTime ?? DateTime.UtcNow;
        var dueAt = ToUtc(created).Add(span);
        return dueAt <= DateTime.UtcNow;
    }

    static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

    void Remind(
        SailClient sail,
        ActivityInstanceEntity act,
        List<TaskEntity> tasks,
        SailNodePolicyDto policy)
    {
        var max = policy.MaxRemindCount > 0 ? policy.MaxRemindCount : 1;
        var kit = sail.UseRunSQL();
        foreach (var task in tasks)
        {
            if (string.IsNullOrWhiteSpace(task.TaskId)) continue;
            var count = RemindCounts.AddOrUpdate(task.TaskId, 1, (_, c) => c + 1);
            if (count > max)
                continue;

            logger.LogInformation(
                "Timeout remind #{Count}/{Max} task={TaskId} user={User} activity={Act}",
                count, max, task.TaskId, task.AssignedUserId, task.ActivityId);
            try
            {
                sail.CapabilityDispatcher.FireTaskLifecycle(
                    kit, task, act, null, TaskNotifyKind.Created);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "timeout remind notifier failed task={Id}", task.TaskId);
            }
        }
    }

    static FlowCommandContext BuildRunnerFromTask(SailClient sail, TaskEntity task, string userId, string userName)
    {
        string version = "1";
        try
        {
            if (!string.IsNullOrWhiteSpace(task.ProcessInstanceId))
            {
                var pi = new WorkflowService(sail).GetProcessInstance(task.ProcessInstanceId);
                if (!string.IsNullOrWhiteSpace(pi?.Version))
                    version = pi!.Version;
            }
        }
        catch { /* keep 1 */ }

        return new FlowCommandContext
        {
            UserId = userId,
            UserName = userName,
            ProcessId = task.ProcessId,
            Version = version,
            BizChannel = task.BizChannel,
            BizPK = task.BizPK,
            TaskId = task.TaskId
        };
    }
}

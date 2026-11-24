using mooSQL.data;
using sailflow.assignee;
using sailflow.assignee.Store;
using sailflow.engine.Client;
using sailflow.utils;
using sailflow.data;
using sailflow.events;
using sailflow.hooks;
using sailflow.log;
using SleveenAdmin.Sailflow.Entity;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 流程级 autoApprovalType 去重：命中则入队 AutoApprove（与 NodePolicy 同人正交）。
/// </summary>
public sealed class SailProcessExtraDedupHook(SailClient sail) : FlowHookBase
{
    public const string CommentOnce = "流程去重：曾办过自动通过";
    public const string CommentConsecutive = "流程去重：连续同人自动通过";

    public override string Name => "sail.process.extra.dedup";
    public override string Title => "流程去重自动通过";

    public override void Handle(FlowHookContext ctx)
    {
        if (ctx.EventType is not (
            EventFireTypeEnum.OnProcessStarted
            or EventFireTypeEnum.OnProcessContinued
            or EventFireTypeEnum.OnProcessResent))
            return;
        if (string.IsNullOrWhiteSpace(ctx.ProcessInstanceId) || string.IsNullOrWhiteSpace(ctx.ProcessId))
            return;

        try
        {
            var kit = ctx.Kit ?? sail.UseRunSQL();
            var version = string.IsNullOrWhiteSpace(ctx.Version) ? ResolveVersion(kit, ctx) : ctx.Version!;
            var extra = SailProcessExtraService.Parse(LoadExtraJson(kit, ctx.ProcessId, version));
            if (extra.AutoApprovalType is not (1 or 2))
                return;

            var waiting = LoadReadyWaitingTasks(kit, ctx.ProcessInstanceId, ctx.ActivityInstanceId);
            if (waiting.Count == 0)
                return;

            var completed = LoadCompletedTasks(kit, ctx.ProcessInstanceId);
            var anchors = new List<AutoApproveAnchor>();

            foreach (var task in waiting)
            {
                if (string.IsNullOrWhiteSpace(task.TaskId) || string.IsNullOrWhiteSpace(task.SignId))
                    continue;

                string? reason = null;
                if (extra.AutoApprovalType == 1)
                {
                    if (completed.Any(c =>
                            string.Equals(c.EndedUserId, task.SignId, StringComparison.OrdinalIgnoreCase)))
                        reason = CommentOnce;
                }
                else if (extra.AutoApprovalType == 2)
                {
                    var prev = completed
                        .Where(c => !string.Equals(
                            c.ActivityInstanceId, task.ActivityInstanceId, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(c => c.CompletedAt ?? DateTime.MinValue)
                        .FirstOrDefault();
                    if (prev != null
                        && string.Equals(prev.EndedUserId, task.SignId, StringComparison.OrdinalIgnoreCase))
                        reason = CommentConsecutive;
                }

                if (reason == null)
                    continue;

                anchors.Add(new AutoApproveAnchor
                {
                    StrategyCode = extra.AutoApprovalType == 1 ? "ProcessDedupOnce" : "ProcessDedupConsecutive",
                    Reason = reason,
                    ProcessId = task.ProcessId ?? ctx.ProcessId,
                    Version = version,
                    ProcessInstanceId = task.ProcessInstanceId ?? ctx.ProcessInstanceId,
                    ActivityId = task.ActivityId,
                    ActivityCode = ctx.ActivityCode,
                    ActivityInstanceId = task.ActivityInstanceId,
                    TaskId = task.TaskId,
                    PreferUserId = task.SignId,
                    PreferUserName = task.SignName
                });
            }

            if (anchors.Count == 0)
                return;

            new HookOutboxAutoApproveAnchorStore().Enqueue(sail, anchors, ctx.Kit);
            sail.Log?.Message(
                SailLogCategories.SyncHook,
                "name={0} enqueued {1} anchor(s) PI={2} Type={3}",
                Name, anchors.Count, ctx.ProcessInstanceId, extra.AutoApprovalType);
        }
        catch (Exception ex)
        {
            sail.Log?.Warning(
                SailLogCategories.SyncHook,
                "name={0} PI={1}: {2}",
                Name, ctx.ProcessInstanceId, ex.Message);
        }
    }

    static string ResolveVersion(SQLBuilder kit, FlowHookContext ctx)
    {
        var pi = kit.useRepo<ProcessInstanceEntity, string>().GetById(ctx.ProcessInstanceId);
        return string.IsNullOrWhiteSpace(pi?.Version) ? "1" : pi!.Version!;
    }

    static string? LoadExtraJson(SQLBuilder kit, string processId, string version)
    {
        var clip = kit.useClip();
        clip.from<SailProcessMetaEntity>(out var m);
        clip.where(() => m.ProcessId, processId);
        clip.where(() => m.Version, version);
        return clip.select(m).queryList().FirstOrDefault()?.ExtraJson;
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

    static List<TaskEntity> LoadCompletedTasks(SQLBuilder kit, string processInstanceId)
    {
        var clip = kit.useClip();
        clip.from<TaskEntity>(out var t)
            .where(() => t.ProcessInstanceId, processInstanceId)
            .where(() => t.TaskState, (short)TaskStateEnum.Completed);
        return clip.select(t).queryList()?.ToList() ?? new List<TaskEntity>();
    }
}

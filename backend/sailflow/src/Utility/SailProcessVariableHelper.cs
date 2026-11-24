using Microsoft.Extensions.Logging;
using mooSQL.data;
using sailflow.utils;
using sailflow.data;
using sailflow.service;

namespace SleveenAdmin.Sailflow.Utility;

/// <summary>
/// 动态表单 Conditions ↔ 流程运行变量（wf_proc_variable）。
/// 引擎 Start/Run 只用 Conditions 做路由，不会自动落库；Host 负责持久化与回填。
/// </summary>
public static class SailProcessVariableHelper
{
    public const string ActCommentPrefix = "__sail.act.comment.";
    public const string ActSignPrefix = "__sail.act.sign.";

    /// <summary>把 runner.Conditions 写入流程作用域变量（同名覆盖）。</summary>
    public static void PersistConditions(
        IWorkflowService wf,
        FlowCommandContext? runner,
        string? processInstanceId,
        ILogger? logger = null)
    {
        if (wf == null || runner?.Conditions == null || runner.Conditions.Count == 0)
            return;
        if (string.IsNullOrWhiteSpace(processInstanceId))
            return;

        foreach (var kv in runner.Conditions)
        {
            if (string.IsNullOrWhiteSpace(kv.Key))
                continue;
            if (string.IsNullOrEmpty(kv.Value))
                continue;

            UpsertVariable(wf, runner, processInstanceId, kv.Key, kv.Value, logger);
        }

        PersistActivityOpinion(wf, runner, processInstanceId, logger);
    }

    static void PersistActivityOpinion(
        IWorkflowService wf,
        FlowCommandContext runner,
        string processInstanceId,
        ILogger? logger)
    {
        string? comment = null;
        string? sign = null;
        foreach (var kv in runner.Conditions!)
        {
            if (string.Equals(kv.Key, "comment", StringComparison.OrdinalIgnoreCase))
                comment = kv.Value;
            else if (string.Equals(kv.Key, "signPicUrl", StringComparison.OrdinalIgnoreCase))
                sign = kv.Value;
        }
        if (string.IsNullOrWhiteSpace(comment) && string.IsNullOrWhiteSpace(sign))
            return;

        string? actId = null;
        if (!string.IsNullOrWhiteSpace(runner.ActivityInstanceId))
            actId = runner.ActivityInstanceId.Trim();
        if (string.IsNullOrWhiteSpace(actId) && !string.IsNullOrWhiteSpace(runner.TaskId))
        {
            try
            {
                actId = wf.GetTaskView(runner.TaskId)?.ActivityInstanceId;
            }
            catch
            {
                /* ignore */
            }
        }
        // 办结后 TaskView 可能已无 ActivityInstanceId：回落全局 comment/sign 键仍写入，供时间线按活动补齐前再被覆盖
        if (string.IsNullOrWhiteSpace(actId))
            return;

        if (!string.IsNullOrWhiteSpace(comment))
            UpsertVariable(wf, runner, processInstanceId, ActCommentPrefix + actId, comment!, logger);
        if (!string.IsNullOrWhiteSpace(sign))
            UpsertVariable(wf, runner, processInstanceId, ActSignPrefix + actId, sign!, logger);
    }

    static void UpsertVariable(
        IWorkflowService wf,
        FlowCommandContext runner,
        string processInstanceId,
        string name,
        string value,
        ILogger? logger)
    {
        try
        {
            var exist = wf.GetProcessVariable(new ProcessVariableQuery
            {
                VariableSceope = ProcessVariableScopeEnum.Process,
                ProcessInstanceId = processInstanceId,
                Name = name
            });

            if (exist != null && !string.IsNullOrWhiteSpace(exist.ProcessVariableId))
            {
                exist.Value = value;
                exist.BizPK = runner.BizPK ?? exist.BizPK;
                exist.ProcessId = runner.ProcessId ?? exist.ProcessId;
                exist.UpdatedDateTime = DateTime.UtcNow;
                wf.SaveProcessVariable(exist);
            }
            else
            {
                wf.SaveProcessVariable(new ProcessVariableEntity
                {
                    VariableScope = ProcessVariableScopeEnum.Process.ToString(),
                    BizPK = runner.BizPK,
                    ProcessId = runner.ProcessId,
                    ProcessInstanceId = processInstanceId,
                    Name = name,
                    Value = value,
                    UpdatedDateTime = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "PersistConditions failed: processInstanceId={ProcessInstanceId}, name={Name}",
                processInstanceId,
                name);
        }
    }

    /// <summary>读取实例流程变量为表单值字典（供 Workbench 回填）。</summary>
    public static Dictionary<string, string> LoadFormValues(
        IWorkflowService wf,
        string? processInstanceId,
        ILogger? logger = null)
        => LoadFormValues(wf, kit: null, processInstanceId, logger);

    /// <summary>
    /// 同事务读取流程变量。Sync 钩子须传 <paramref name="kit"/>，禁止另开连接。
    /// </summary>
    public static Dictionary<string, string> LoadFormValues(
        IWorkflowService wf,
        SQLBuilder? kit,
        string? processInstanceId,
        ILogger? logger = null)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (wf == null || string.IsNullOrWhiteSpace(processInstanceId))
            return map;

        try
        {
            var query = new ProcessVariableQuery { ProcessInstanceId = processInstanceId };
            var list = kit != null
                ? wf.GetProcessVariableList(kit, query)
                : wf.GetProcessVariableList(query);
            if (list == null)
                return map;

            foreach (var row in list)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.Name))
                    continue;
                var isProcess = string.Equals(
                    row.VariableScope,
                    ProcessVariableScopeEnum.Process.ToString(),
                    StringComparison.OrdinalIgnoreCase);
                if (map.ContainsKey(row.Name!) && !isProcess)
                    continue;
                map[row.Name!] = row.Value ?? "";
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(
                ex,
                "LoadFormValues failed: processInstanceId={ProcessInstanceId}",
                processInstanceId);
        }

        return map;
    }

    /// <summary>从流程变量抽出活动意见/签名（key=ActivityInstanceId）。</summary>
    public static Dictionary<string, ActivityOpinionDto> LoadActivityOpinions(
        Dictionary<string, string>? formValues)
    {
        var map = new Dictionary<string, ActivityOpinionDto>(StringComparer.OrdinalIgnoreCase);
        if (formValues == null || formValues.Count == 0)
            return map;

        foreach (var kv in formValues)
        {
            if (kv.Key.StartsWith(ActCommentPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var id = kv.Key[ActCommentPrefix.Length..];
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (!map.TryGetValue(id, out var op))
                    map[id] = op = new ActivityOpinionDto { ActivityInstanceId = id };
                op.Comment = kv.Value;
            }
            else if (kv.Key.StartsWith(ActSignPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var id = kv.Key[ActSignPrefix.Length..];
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (!map.TryGetValue(id, out var op))
                    map[id] = op = new ActivityOpinionDto { ActivityInstanceId = id };
                op.SignPicUrl = kv.Value;
            }
        }
        return map;
    }

    /// <summary>
    /// 无按活动键时，把全局 comment/signPicUrl 挂到最近办结的人工活动（兼容落库时 Task 已无 ActivityInstanceId）。
    /// </summary>
    public static void AttachGlobalOpinionFallback(
        Dictionary<string, ActivityOpinionDto> map,
        Dictionary<string, string>? formValues,
        IEnumerable<ActivityInstanceEntity>? activities)
    {
        if (map == null || map.Count > 0 || formValues == null || activities == null)
            return;

        string? comment = null;
        string? sign = null;
        foreach (var kv in formValues)
        {
            if (string.Equals(kv.Key, "comment", StringComparison.OrdinalIgnoreCase))
                comment = kv.Value;
            else if (string.Equals(kv.Key, "signPicUrl", StringComparison.OrdinalIgnoreCase))
                sign = kv.Value;
        }
        if (string.IsNullOrWhiteSpace(comment) && string.IsNullOrWhiteSpace(sign))
            return;

        // ActivityType 4=UserTask；ActivityState 4=Completed（与 TaskView 一致）
        var hit = activities
            .Where(a => a != null && !string.IsNullOrWhiteSpace(a.ActivityInstanceId))
            .Where(a => a.ActivityType == 4 && a.ActivityState == 4)
            .OrderByDescending(a => a.EndedDateTime ?? a.CreatedDateTime)
            .FirstOrDefault();
        if (hit == null)
            return;

        map[hit.ActivityInstanceId!] = new ActivityOpinionDto
        {
            ActivityInstanceId = hit.ActivityInstanceId!,
            Comment = comment,
            SignPicUrl = sign
        };
    }

    /// <summary>从执行结果或 runner 解析实例 Id。</summary>
    public static string? ResolveProcessInstanceId(
        IWorkflowService wf,
        FlowCommandContext? runner,
        string? processInstanceIdStarted)
    {
        if (!string.IsNullOrWhiteSpace(processInstanceIdStarted))
            return processInstanceIdStarted.Trim();
        if (!string.IsNullOrWhiteSpace(runner?.TaskId))
        {
            try
            {
                var task = wf.GetTaskView(runner.TaskId);
                if (!string.IsNullOrWhiteSpace(task?.ProcessInstanceId))
                    return task!.ProcessInstanceId!.Trim();
            }
            catch
            {
                /* ignore */
            }
        }

        return null;
    }
}

public sealed class ActivityOpinionDto
{
    public string ActivityInstanceId { get; set; } = "";
    public string? Comment { get; set; }
    public string? SignPicUrl { get; set; }
}

using Microsoft.Extensions.Logging;
using saiflow.engine.Common;
using saiflow.engine.Entity;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.Utility;

/// <summary>
/// 动态表单 Conditions ↔ 流程运行变量（wf_process_variable）。
/// 引擎 Start/Run 只用 Conditions 做路由，不会自动落库；Host 负责持久化与回填。
/// </summary>
public static class SailProcessVariableHelper
{
    /// <summary>把 runner.Conditions 写入流程作用域变量（同名覆盖）。</summary>
    public static void PersistConditions(
        IWorkflowService wf,
        FlowCommandContext? runner,
        int processInstanceId,
        ILogger? logger = null)
    {
        if (wf == null || runner?.Conditions == null || runner.Conditions.Count == 0)
            return;
        if (processInstanceId <= 0)
            return;

        foreach (var kv in runner.Conditions)
        {
            if (string.IsNullOrWhiteSpace(kv.Key))
                continue;
            // ProcessVariableManager.SaveVariable 拒绝空 Value — Host 侧跳过
            if (string.IsNullOrEmpty(kv.Value))
                continue;

            try
            {
                var exist = wf.GetProcessVariable(new ProcessVariableQuery
                {
                    VariableSceope = ProcessVariableScopeEnum.Process,
                    ProcessInstanceId = processInstanceId,
                    Name = kv.Key
                });

                if (exist != null && exist.Id is > 0)
                {
                    exist.Value = kv.Value;
                    exist.AppInstanceId = runner.AppInstanceId ?? exist.AppInstanceId;
                    exist.ProcessId = runner.ProcessId ?? exist.ProcessId;
                    exist.UpdatedDateTime = DateTime.UtcNow;
                    wf.SaveProcessVariable(exist);
                }
                else
                {
                    // 自增主键 Id 保持 null（勿写 0），交给 SaveVariable / AUTOINCREMENT
                    wf.SaveProcessVariable(new ProcessVariableEntity
                    {
                        Id = null,
                        VariableScope = ProcessVariableScopeEnum.Process.ToString(),
                        AppInstanceId = runner.AppInstanceId,
                        ProcessId = runner.ProcessId,
                        ProcessInstanceId = processInstanceId,
                        Name = kv.Key,
                        Value = kv.Value,
                        UpdatedDateTime = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                // 单字段失败不阻断发起/办理，但必须可见
                logger?.LogError(
                    ex,
                    "PersistConditions failed: processInstanceId={ProcessInstanceId}, name={Name}",
                    processInstanceId,
                    kv.Key);
            }
        }
    }

    /// <summary>读取实例流程变量为表单值字典（供 Workbench 回填）。</summary>
    public static Dictionary<string, string> LoadFormValues(
        IWorkflowService wf,
        int processInstanceId,
        ILogger? logger = null)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (wf == null || processInstanceId <= 0)
            return map;

        try
        {
            var list = wf.GetProcessVariableList(new ProcessVariableQuery
            {
                ProcessInstanceId = processInstanceId
            });
            if (list == null)
                return map;

            foreach (var row in list)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.Name))
                    continue;
                // 流程作用域优先；同名活动变量不覆盖已有流程变量
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

    /// <summary>从执行结果或 runner 解析实例 Id。</summary>
    public static int ResolveProcessInstanceId(
        IWorkflowService wf,
        FlowCommandContext? runner,
        int processInstanceIdStarted)
    {
        if (processInstanceIdStarted > 0)
            return processInstanceIdStarted;
        if (runner?.TaskId is > 0)
        {
            try
            {
                var task = wf.GetTaskView(runner.TaskId.Value);
                if (task?.ProcessInstanceId is > 0)
                    return task.ProcessInstanceId.Value;
            }
            catch
            {
                /* ignore */
            }
        }

        return 0;
    }
}

using sailflow.utils;
using sailflow.data;
using sailflow.result;
using sailflow.service;
using SleveenAdmin.Sailflow.Dto;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Operations;

/// <summary>ExtraJson 对 Withdraw / 发起人 Close(撤销) 的服务端闸。</summary>
public static class SailExtraOperationGate
{
    public static WfExecutedResult? TryReject(
        string buttonId,
        FlowCommandContext runner,
        IWorkflowService wf,
        SailProcessExtraService extraSvc)
    {
        if (string.IsNullOrWhiteSpace(buttonId) || runner == null || extraSvc == null || wf == null)
            return null;

        var (processId, version, processInstanceId, starterId) =
            ResolveProcess(runner, wf);
        if (string.IsNullOrWhiteSpace(processId))
            return null;

        var extra = extraSvc.Get(processId, version);

        if (string.Equals(buttonId, "Withdraw", StringComparison.OrdinalIgnoreCase))
        {
            if (!extra.AllowWithdrawTask)
                return Fail("WithdrawNotAllowed", "流程未允许审批人撤回任务。");
            return null;
        }

        if (string.Equals(buttonId, "Close", StringComparison.OrdinalIgnoreCase))
        {
            // 发起人撤销：受 allowCancel 约束；非发起人视为管理关闭，放行
            var userId = runner.UserId ?? "";
            if (!string.IsNullOrWhiteSpace(starterId)
                && string.Equals(userId, starterId, StringComparison.OrdinalIgnoreCase)
                && !extra.AllowCancelRunningProcess)
            {
                return Fail("CancelNotAllowed", "流程未允许发起人撤销审批中的申请。");
            }
        }

        return null;
    }

    static (string? processId, string? version, string? processInstanceId, string? starterId)
        ResolveProcess(FlowCommandContext runner, IWorkflowService wf)
    {
        string? processId = runner.ProcessId;
        string? version = runner.Version;
        string? processInstanceId = null;
        string? starterId = null;

        if (!string.IsNullOrWhiteSpace(runner.TaskId))
        {
            try
            {
                var task = wf.GetTaskView(runner.TaskId);
                if (task != null)
                {
                    processId ??= task.ProcessId;
                    version ??= task.Version;
                    processInstanceId = task.ProcessInstanceId;
                }
            }
            catch { /* ignore */ }
        }

        if (string.IsNullOrWhiteSpace(processInstanceId)
            && !string.IsNullOrWhiteSpace(runner.ProcessInstanceId))
            processInstanceId = runner.ProcessInstanceId;

        if (string.IsNullOrWhiteSpace(processInstanceId)
            && runner.Conditions != null
            && runner.Conditions.TryGetValue("ProcessInstanceId", out var piCond)
            && !string.IsNullOrWhiteSpace(piCond))
            processInstanceId = piCond;

        if (!string.IsNullOrWhiteSpace(processInstanceId))
        {
            try
            {
                var pi = wf.GetProcessInstance(processInstanceId);
                if (pi != null)
                {
                    processId ??= pi.ProcessId;
                    version ??= pi.Version;
                    starterId = pi.CreatedUserId;
                }
            }
            catch { /* ignore */ }
        }

        return (processId, version, processInstanceId, starterId);
    }

    static WfExecutedResult Fail(string reason, string message)
    {
        var fail = WfExecutedResult.Default();
        fail.Status = WfExecutedStatus.Exception;
        fail.Message = message;
        fail.ReasonCode = reason;
        return fail;
    }
}

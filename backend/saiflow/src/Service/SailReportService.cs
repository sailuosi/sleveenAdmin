using mooSQL.data;
using saiflow.data;
using saiflow.engine.Entity;
using saiflow.engine.Manager;
using saiflow.engine.Client;
using saiflow.engine.Common;
using SleveenAdmin.Saiflow.Dto;
using SleveenAdmin.Saiflow.Entity;

namespace SleveenAdmin.Saiflow.Service;

/// <summary>流程报表聚合（只读查询）。</summary>
public class SailReportService
{
    private readonly SailClient _sail;

    public SailReportService(SailClient sail) =>
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));

    public WorkflowReportOverviewDto GetOverview(int top = 10)
    {
        var dto = new WorkflowReportOverviewDto();
        var kit = _sail.UseRunSQL();

        var processes = kit.useClip().from<ProcessEntity>(out var p).select(p).queryList().ToList();
        dto.ProcessDefinitionTotal = processes.Count;
        dto.ProcessDefinitionEnabled = processes.Count(x => x.Status == 1);
        dto.ProcessDefinitionDisabled = processes.Count(x => x.Status != 1);

        var instances = kit.useClip().from<ProcessInstanceEntity>(out var pi).select(pi).queryList().ToList();
        dto.InstanceTotal = instances.Count;
        dto.InstanceRunning = instances.Count(x => x.ProcessState == (short)ProcessStateEnum.Running
            || x.ProcessState == (short)ProcessStateEnum.Ready);
        dto.InstanceCompleted = instances.Count(x => x.ProcessState == (short)ProcessStateEnum.Completed);
        dto.InstanceSuspended = instances.Count(x => x.ProcessState == (short)ProcessStateEnum.Suspended);
        dto.InstanceOther = dto.InstanceTotal - dto.InstanceRunning - dto.InstanceCompleted - dto.InstanceSuspended;
        if (dto.InstanceOther < 0) dto.InstanceOther = 0;

        // 待办：运行中流程上的 Ready/Running 活动
        try
        {
            var taskClip = kit.useClip();
            taskClip.from<TaskViewEntity>(out var t);
            taskClip.where(() => t.ProcessState, (short)ProcessStateEnum.Running);
            var readyTasks = taskClip.select(t).queryList().ToList();
            dto.TaskReady = readyTasks.Count(x =>
            {
                var state = x.ActivityState ?? 0;
                return state == 1 || state == 2;
            });
        }
        catch
        {
            dto.TaskReady = 0;
        }

        var since = DateTime.Now.AddDays(-7);
        try
        {
            var doneClip = kit.useClip();
            doneClip.from<TaskViewEntity>(out var td);
            doneClip.where(() => td.TaskState, (short)4);
            dto.TaskCompletedRecent = doneClip.select(td).queryList()
                .Count(x => x.EndedDateTime != null && x.EndedDateTime >= since);
        }
        catch
        {
            dto.TaskCompletedRecent = 0;
        }

        try
        {
            var outbox = new HookOutboxManager(_sail);
            dto.OutboxDead = outbox.CountByStatus(kit, HookOutboxStatus.Dead);
        }
        catch
        {
            dto.OutboxDead = 0;
        }

        try
        {
            kit.useDBInit().autoMode().createTable<SailLeaveEntity>();
            var leaveClip = kit.useClip();
            leaveClip.from<SailLeaveEntity>(out var lv);
            leaveClip.where(() => lv.Status, SailLeaveStatus.Running);
            dto.LeaveRunning = leaveClip.count();
        }
        catch
        {
            dto.LeaveRunning = 0;
        }

        var nameMap = processes
            .GroupBy(x => x.ProcessId ?? "")
            .ToDictionary(g => g.Key, g => g.First().ProcessName ?? g.Key);

        top = top <= 0 ? 10 : Math.Min(top, 50);
        dto.TopProcesses = instances
            .GroupBy(x => new { x.ProcessId, x.Version })
            .Select(g => new ProcessInstanceRankItem
            {
                ProcessId = g.Key.ProcessId,
                Version = g.Key.Version,
                ProcessName = nameMap.TryGetValue(g.Key.ProcessId ?? "", out var n) ? n : g.Key.ProcessId,
                InstanceCount = g.Count()
            })
            .OrderByDescending(x => x.InstanceCount)
            .Take(top)
            .ToList();

        return dto;
    }
}

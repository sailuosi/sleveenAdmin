namespace SleveenAdmin.Saiflow.Dto;

/// <summary>流程报表总览（独立查询，不造第二引擎）。</summary>
public class WorkflowReportOverviewDto
{
    public int ProcessDefinitionTotal { get; set; }
    public int ProcessDefinitionEnabled { get; set; }
    public int ProcessDefinitionDisabled { get; set; }

    public int InstanceTotal { get; set; }
    public int InstanceRunning { get; set; }
    public int InstanceCompleted { get; set; }
    public int InstanceSuspended { get; set; }
    public int InstanceOther { get; set; }

    public int TaskReady { get; set; }
    public int TaskCompletedRecent { get; set; }

    public int OutboxDead { get; set; }
    public int LeaveRunning { get; set; }

    public List<ProcessInstanceRankItem> TopProcesses { get; set; } = new();
}

public class ProcessInstanceRankItem
{
    public string? ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public string? Version { get; set; }
    public int InstanceCount { get; set; }
}

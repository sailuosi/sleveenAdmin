using saiflow.engine.Entity;
using SleveenAdmin.Saiflow.Entity;

namespace SleveenAdmin.Saiflow.Dto;

/// <summary>流程编辑聚合：定义行 + 产品元数据。</summary>
public class ProcessEditBundle
{
    public ProcessEntity? Process { get; set; }
    public SailProcessMetaEntity? Meta { get; set; }
    /// <summary>是否已有 BPMN（流程设计步骤校验）</summary>
    public bool HasBpmn { get; set; }
}

/// <summary>保存流程编辑（基本信息 + 表单 + 更多设置）。</summary>
public class ProcessEditSaveRequest
{
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public string? ProcessName { get; set; }
    public string? ProcessCode { get; set; }
    public string? Description { get; set; }
    public string? AppType { get; set; }
    public string? Icon { get; set; }
    public byte? Status { get; set; }

    public int FormType { get; set; } = 10;
    public string? FormId { get; set; }
    public string? FormCustomCreatePath { get; set; }
    public string? FormCustomViewPath { get; set; }
    public int Visible { get; set; } = 1;
    public int StartUserType { get; set; }
    public string? StartUserIds { get; set; }
    public string? StartDeptIds { get; set; }
    public string? ManagerUserIds { get; set; }
    public string? ExtraJson { get; set; }
}

using saiflow.engine.Entity;
using saiflow.engine.Common;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Dto;

/// <summary>办理工作台聚合（对标 n_flowable get-approval-detail，语义走 Sail）。</summary>
public class ApprovalWorkbenchDto
{
    public TaskViewEntity? Task { get; set; }
    public ProcessInstanceEntity? ProcessInstance { get; set; }
    public List<NodeView> NextNodes { get; set; } = new();
    /// <summary>可退回节点（S3）。</summary>
    public List<NodeView> PreviousNodes { get; set; } = new();
    public List<ActivityInstanceEntity> ActivityInstances { get; set; } = new();
    public List<ActivityInstanceEntity> ReadyActivities { get; set; } = new();
    /// <summary>表单 conf（S2 form-create）。</summary>
    public object? FormConf { get; set; }
    /// <summary>表单运行时值（来自 wf_process_variable 流程作用域）。</summary>
    public Dictionary<string, string> FormValues { get; set; } = new();
    /// <summary>
    /// 字段权限列表（§C）：含 Mode + 兼容 IsReadOnly/IsNotVisible。
    /// NodePolicy.fieldsPermission 优先；否则 Form 活动权限表。
    /// </summary>
    public object? FieldPermissions { get; set; }
    /// <summary>§C：field → editable|readonly|hidden|masked。</summary>
    public Dictionary<string, string> FieldModes { get; set; } = new();
    /// <summary>当前任务节点 sf:ButtonsSetting（缺省返回码表默认）。</summary>
    public List<SailButtonSettingDto> ButtonsSetting { get; set; } = SailButtonsCatalog.Merge(null);

    /// <summary>§G：当前活动是否带退回源（可 Resend 返送）。</summary>
    public bool HasBackSrc { get; set; }

    /// <summary>§G：退回源活动实例 Id（有则 HasBackSrc=true）。</summary>
    public int? BackSrcActivityInstanceId { get; set; }

    /// <summary>§J 维①：Prev | Starter | History（来自节点 NodePolicy）。</summary>
    public string SendBackTarget { get; set; } = "Starter";

    /// <summary>§J 维②：ForwardNormal | ReturnToSender。</summary>
    public string ResubmitMode { get; set; } = "ReturnToSender";

    /// <summary>节点产品策略摘要（空人/超时/退回两维等）。</summary>
    public SailNodePolicyDto? NodePolicy { get; set; }
}

/// <summary>工作台查询入参。</summary>
public class ApprovalWorkbenchQuery
{
    public int? TaskId { get; set; }
    public int? ProcessInstanceId { get; set; }
    /// <summary>显式表单 Id；缺省时尝试用实例 AppInstanceCode（发起时写入的 formId）。</summary>
    public string? FormId { get; set; }
    public IDictionary<string, string>? Conditions { get; set; }
}

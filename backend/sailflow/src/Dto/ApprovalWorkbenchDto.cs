using sailflow.engine.Entity;
using sailflow.engine.Common;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Dto;

/// <summary>办理工作台聚合（对标 n_flowable get-approval-detail，语义走 Sail）。</summary>
public class ApprovalWorkbenchDto
{
    public TaskViewEntity? Task { get; set; }
    public ProcessInstanceEntity? ProcessInstance { get; set; }
    public List<NextActivityNodeDto> NextNodes { get; set; } = new();
    /// <summary>
    /// 当前任务是否为会签/加签最后一人（非 MI 子任务恒为 true）。
    /// 非末人办理时前端应禁用下一节点选人，可直接发送完成本席。
    /// </summary>
    public bool IsLastTask { get; set; } = true;
    /// <summary>可退回节点（S3）。</summary>
    public List<NodeView> PreviousNodes { get; set; } = new();
    public List<ActivityInstanceEntity> ActivityInstances { get; set; } = new();
    public List<ActivityInstanceEntity> ReadyActivities { get; set; } = new();
    /// <summary>表单 conf（S2 form-create）。</summary>
    public object? FormConf { get; set; }
    /// <summary>表单运行时值（来自 wf_proc_variable 流程作用域）。</summary>
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
    public string? BackSrcActInstanceId { get; set; }

    /// <summary>
    /// §G：退回人 UserId（取自 BackSrc 活动的 EndedUserId，勿用 CreatedUser*）。
    /// </summary>
    public string? BackSrcUserId { get; set; }

    /// <summary>§G：退回人显示名。</summary>
    public string? BackSrcUserName { get; set; }

    /// <summary>当前节点未办结、可减签的办理人（不含本人）。</summary>
    public List<RemovableAssigneeDto> RemovableAssignees { get; set; } = new();

    /// <summary>当前节点全部未办结办理人（含本人；改派预填 / 催办候选）。</summary>
    public List<RemovableAssigneeDto> CurrentAssignees { get; set; } = new();

    /// <summary>§J 维①：Prev | Starter | History（节点优先；Extra 非空可覆盖）。</summary>
    public string SendBackTarget { get; set; } = "Starter";

    /// <summary>§J 维②：ForwardNormal | ReturnToSender（节点优先；Extra 非空可覆盖）。</summary>
    public string ResubmitMode { get; set; } = "ReturnToSender";

    /// <summary>节点产品策略摘要（空人/超时/退回两维等）。</summary>
    public SailNodePolicyDto? NodePolicy { get; set; }

    /// <summary>办理人角色：null/0=常规；1=沟通参与者。</summary>
    public int? AssigneeRole { get; set; }

    /// <summary>当前审批待办下是否仍有未关闭的沟通参与者。</summary>
    public bool ConsultPending { get; set; }

    /// <summary>打开中的沟通参与者数量（ConsultPending 时 &gt; 0）。</summary>
    public int OpenParticipantCount { get; set; }

    /// <summary>流程 ExtraJson 运行时摘要（闸控 / 打印 / 展示）。</summary>
    public SailProcessExtraRuntimeDto? Extra { get; set; }

    /// <summary>流程摘要（变量 __sail_summary__）。</summary>
    public string? Summary { get; set; }

    /// <summary>活动办结意见/签名（key=ActivityInstanceId）。</summary>
    public Dictionary<string, ActivityOpinionDto> ActivityOpinions { get; set; } = new();
}

public sealed class RemovableAssigneeDto
{
    public string TaskId { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
}

/// <summary>Workbench 下发的 Extra 运行时字段（避免整包泄漏 URL 密钥时可再收紧）。</summary>
public sealed class SailProcessExtraRuntimeDto
{
    public bool AllowCancelRunningProcess { get; set; } = true;
    public bool AllowWithdrawTask { get; set; }
    public int AutoApprovalType { get; set; }
    public bool PrintTemplateEnable { get; set; }
    public string? PrintTemplate { get; set; }
    /// <summary>流程级退回覆盖（非空才生效）。</summary>
    public string? SendBackTarget { get; set; }
    public string? ResubmitMode { get; set; }
}

/// <summary>工作台查询入参。</summary>
public class ApprovalWorkbenchQuery
{
    public string? TaskId { get; set; }
    public string? ProcessInstanceId { get; set; }
    /// <summary>
    /// 显式表单 Id（Guid 字符串；兼容旧数字 Id）。
    /// 缺省：实例 BizCode（发起写入）→ 再回落 sf_process_meta.FormId。
    /// </summary>
    public string? FormId { get; set; }
    public IDictionary<string, string>? Conditions { get; set; }
}

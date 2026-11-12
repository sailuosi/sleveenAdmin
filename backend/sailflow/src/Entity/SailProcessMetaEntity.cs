using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>
/// 流程产品元数据（表单绑定 / 可见性 / 发起范围 / 更多设置 JSON）。
/// 对标 n_flowable 模型表单四步中「基本信息扩展 + 表单设计 + 更多设置」。
/// </summary>
[SooTable(Name = "sf_process_meta", Caption = "流程产品元数据", Type = DBTableType.Table)]
public class SailProcessMetaEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "process_id", Length = 64)]
    public string ProcessId { get; set; } = "";

    [SooColumn(Name = "version", Length = 32)]
    public string Version { get; set; } = "1";

    /// <summary>10=流程表单(NORMAL) 20=业务表单(CUSTOM) 0=未配置</summary>
    [SooColumn(Name = "form_type")]
    public int FormType { get; set; } = 10;

    [SooColumn(Name = "form_id", Length = 64)]
    public string FormId { get; set; } = "";

    [SooColumn(Name = "form_custom_create_path", Length = 255)]
    public string FormCustomCreatePath { get; set; } = "";

    [SooColumn(Name = "form_custom_view_path", Length = 255)]
    public string FormCustomViewPath { get; set; } = "";

    /// <summary>发起页是否可见 1/0</summary>
    [SooColumn(Name = "visible")]
    public int Visible { get; set; } = 1;

    /// <summary>0全员 1指定人员 2指定部门</summary>
    [SooColumn(Name = "start_user_type")]
    public int StartUserType { get; set; }

    [SooColumn(Name = "start_user_ids", Length = 2000)]
    public string StartUserIds { get; set; } = "[]";

    [SooColumn(Name = "start_dept_ids", Length = 2000)]
    public string StartDeptIds { get; set; } = "[]";

    [SooColumn(Name = "manager_user_ids", Length = 2000)]
    public string ManagerUserIds { get; set; } = "[]";

    /// <summary>更多设置整包 JSON（撤销/撤回/编码规则/去重/标题/摘要/通知/打印模板）</summary>
    [SooColumn(Name = "extra_json")]
    public string ExtraJson { get; set; } = "{}";

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

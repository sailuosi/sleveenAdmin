using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>
/// 流程实例预选办理人（产品表；按 PI + ActivityId 存人，供 Resolve Type=Preselect）。
/// </summary>
[SooTable(Name = "sf_preselect_assignee", Caption = "流程预选办理人", Type = DBTableType.Table)]
public class SailPreselectAssigneeEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "process_instance_id", Length = 64)]
    public string ProcessInstanceId { get; set; } = "";

    /// <summary>活动定义 Id（BPMN element id）。</summary>
    [SooColumn(Name = "activity_id", Length = 128)]
    public string ActivityId { get; set; } = "";

    /// <summary>用户编码（Account，R3）。</summary>
    [SooColumn(Name = "user_id", Length = 64)]
    public string UserId { get; set; } = "";

    [SooColumn(Name = "user_name", Length = 128)]
    public string UserName { get; set; } = "";

    [SooColumn(Name = "group_id", Length = 64)]
    public string? GroupId { get; set; }

    [SooColumn(Name = "group_name", Length = 128)]
    public string? GroupName { get; set; }

    [SooColumn(Name = "group_role", Length = 32)]
    public string? GroupRole { get; set; }

    [SooColumn(Name = "unit_id", Length = 64)]
    public string? UnitId { get; set; }

    [SooColumn(Name = "unit_code", Length = 64)]
    public string? UnitCode { get; set; }

    [SooColumn(Name = "unit_name", Length = 128)]
    public string? UnitName { get; set; }

    [SooColumn(Name = "sort_no")]
    public int SortNo { get; set; }

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

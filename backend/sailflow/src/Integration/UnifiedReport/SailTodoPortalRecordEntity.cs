using mooSQL.data;

namespace SleveenAdmin.Saiflow.Integration.UnifiedReport;

/// <summary>待办推送集成平台流水（对齐 szkfpt primary_todo_portal_record）。</summary>
[SooTable(Name = "sf_todo_portal_record", Caption = "待办推送集成平台记录", Type = DBTableType.Table)]
public class SailTodoPortalRecordEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "task_id", Length = 64)]
    public string? TaskId { get; set; }

    [SooColumn(Name = "schedule_id", Length = 64)]
    public string ScheduleId { get; set; } = "";

    [SooColumn(Name = "to_maincode", Length = 128)]
    public string ToMaincode { get; set; } = "";

    [SooColumn(Name = "to_nickname", Length = 128)]
    public string ToNickname { get; set; } = "";

    /// <summary>推送待办 / 更新待办 / 删除待办</summary>
    [SooColumn(Name = "op_type", Length = 32)]
    public string OpType { get; set; } = "";

    /// <summary>Y / N</summary>
    [SooColumn(Name = "is_success", Length = 8)]
    public string IsSuccess { get; set; } = "Y";

    [SooColumn(Name = "result", Length = 2000)]
    public string? Result { get; set; }

    [SooColumn(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }
}

using mooSQL.data;

namespace SleveenAdmin.Saiflow.Entity;

/// <summary>请假业务样例（产品表；流程结束经 Hook 回写 Status）。</summary>
[SooTable(Name = "sf_demo_leave", Caption = "请假样例", Type = DBTableType.Table)]
public class SailLeaveEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "user_id", Length = 64)]
    public string UserId { get; set; } = "";

    [SooColumn(Name = "user_name", Length = 128)]
    public string UserName { get; set; } = "";

    [SooColumn(Name = "days")]
    public decimal Days { get; set; }

    [SooColumn(Name = "reason", Length = 500)]
    public string Reason { get; set; } = "";

    [SooColumn(Name = "start_date")]
    public DateTime? StartDate { get; set; }

    [SooColumn(Name = "end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>1 审批中 · 2 通过 · 3 驳回 · 4 关闭</summary>
    [SooColumn(Name = "status")]
    public int Status { get; set; } = 1;

    [SooColumn(Name = "process_id", Length = 64)]
    public string ProcessId { get; set; } = "";

    [SooColumn(Name = "process_version", Length = 32)]
    public string ProcessVersion { get; set; } = "1";

    [SooColumn(Name = "process_instance_id", Length = 64)]
    public string ProcessInstanceId { get; set; } = "";

    [SooColumn(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }
}

public static class SailLeaveStatus
{
    public const int Running = 1;
    public const int Approved = 2;
    public const int Rejected = 3;
    public const int Closed = 4;

    public const string AppName = "SailLeave";
}

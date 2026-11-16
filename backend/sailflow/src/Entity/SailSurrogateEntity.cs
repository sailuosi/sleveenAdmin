using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>
/// 委托代批配置（产品表）：建任务 Filter 按 Grantor 并入 Agent，写 ProxySignId。
/// ProcessId 空=全部流程；精确 ProcessId 优先于全局空。
/// </summary>
[SooTable(Name = "sf_surrogate", Caption = "委托代批", Type = DBTableType.Table)]
public class SailSurrogateEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    /// <summary>委托人（原人 SignId / Account）。</summary>
    [SooColumn(Name = "grantor_id", Length = 64)]
    public string GrantorId { get; set; } = "";

    [SooColumn(Name = "grantor_name", Length = 128)]
    public string? GrantorName { get; set; }

    /// <summary>代理人 SignId / Account。</summary>
    [SooColumn(Name = "agent_id", Length = 64)]
    public string AgentId { get; set; } = "";

    [SooColumn(Name = "agent_name", Length = 128)]
    public string? AgentName { get; set; }

    /// <summary>流程定义 Id；空=全部流程。</summary>
    [SooColumn(Name = "process_id", Length = 64)]
    public string? ProcessId { get; set; }

    /// <summary>1=启用；其它=停用。</summary>
    [SooColumn(Name = "enabled")]
    public int Enabled { get; set; } = 1;

    [SooColumn(Name = "start_at")]
    public DateTime? StartAt { get; set; }

    [SooColumn(Name = "end_at")]
    public DateTime? EndAt { get; set; }

    /// <summary>代理人票权；默认 1（计权仍认根）。</summary>
    [SooColumn(Name = "weight")]
    public decimal Weight { get; set; } = 1m;

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [SooColumn(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }
}

using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>
/// 流程归属 Auth* 宿主附表（NuGet ProcessEntity 未暴露 p_auth_* 时使用；语义同 AuthType 合同）。
/// </summary>
[SooTable(Name = "sf_process_auth", Caption = "流程归属Auth", Type = DBTableType.Table)]
public class SailProcessAuthEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "process_id", Length = 64)]
    public string ProcessId { get; set; } = "";

    [SooColumn(Name = "version", Length = 32)]
    public string Version { get; set; } = "1";

    [SooColumn(Name = "auth_type", Length = 20)]
    public string AuthType { get; set; } = "";

    [SooColumn(Name = "auth_code", Length = 200)]
    public string AuthCode { get; set; } = "";

    [SooColumn(Name = "auth_id", Length = 64)]
    public string AuthId { get; set; } = "";

    [SooColumn(Name = "auth_level")]
    public int? AuthLevel { get; set; }

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

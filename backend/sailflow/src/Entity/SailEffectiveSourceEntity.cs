using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>按业务标识 + 单位的发起来源路由（发布即生效维护）。</summary>
[SooTable(Name = "sf_effective_source", Caption = "流程生效来源", Type = DBTableType.Table)]
public class SailEffectiveSourceEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    /// <summary>空字符串表示平台公共来源。</summary>
    [SooColumn(Name = "unit_id", Length = 64)]
    public string UnitId { get; set; } = "";

    [SooColumn(Name = "business_type", Length = 64)]
    public string BusinessType { get; set; } = "";

    /// <summary>PUBLIC | UNIT | PAUSED</summary>
    [SooColumn(Name = "source", Length = 20)]
    public string Source { get; set; } = "PUBLIC";

    [SooColumn(Name = "process_id", Length = 64)]
    public string ProcessId { get; set; } = "";

    [SooColumn(Name = "version", Length = 32)]
    public string Version { get; set; } = "1";

    [SooColumn(Name = "revision")]
    public int Revision { get; set; }

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

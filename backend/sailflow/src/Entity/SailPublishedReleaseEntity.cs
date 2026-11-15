using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>流程发布快照（宿主表；引擎仍用 wf_process Status）。</summary>
[SooTable(Name = "sf_published_release", Caption = "流程发布快照", Type = DBTableType.Table)]
public class SailPublishedReleaseEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "process_id", Length = 64)]
    public string ProcessId { get; set; } = "";

    [SooColumn(Name = "version", Length = 32)]
    public string Version { get; set; } = "1";

    [SooColumn(Name = "business_type", Length = 64)]
    public string BusinessType { get; set; } = "";

    /// <summary>UNIT 归属单位；PUBLIC 为空。</summary>
    [SooColumn(Name = "unit_id", Length = 64)]
    public string UnitId { get; set; } = "";

    [SooColumn(Name = "auth_type", Length = 20)]
    public string AuthType { get; set; } = "";

    [SooColumn(Name = "bpmn_sha", Length = 64)]
    public string BpmnSha { get; set; } = "";

    [SooColumn(Name = "meta_json")]
    public string MetaJson { get; set; } = "{}";

    [SooColumn(Name = "form_snapshot")]
    public string FormSnapshot { get; set; } = "{}";

    /// <summary>1 可用 0 停用</summary>
    [SooColumn(Name = "enabled")]
    public int Enabled { get; set; } = 1;

    [SooColumn(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }
}

using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>流程编码规则序号（process_id + 日期桶）。</summary>
[SooTable(Name = "sf_process_id_seq", Caption = "流程编码序号", Type = DBTableType.Table)]
public class SailProcessIdSeqEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    [SooColumn(Name = "process_id", Length = 64)]
    public string ProcessId { get; set; } = "";

    /// <summary>日期中缀桶：空 / yyyyMMdd / yyyyMMddHH 等。</summary>
    [SooColumn(Name = "bucket", Length = 32)]
    public string Bucket { get; set; } = "";

    [SooColumn(Name = "seq")]
    public int Seq { get; set; }

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

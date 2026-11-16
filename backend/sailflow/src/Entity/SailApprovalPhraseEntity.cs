using mooSQL.data;

namespace SleveenAdmin.Sailflow.Entity;

/// <summary>用户审批常用语（对标 szkfpt ApprovalCommonPhrase）。</summary>
[SooTable(Name = "sf_approval_phrase", Caption = "审批常用语", Type = DBTableType.Table)]
public class SailApprovalPhraseEntity
{
    [SooColumn(Name = "id", IsPrimaryKey = true, Length = 64)]
    public string Id { get; set; } = "";

    /// <summary>所属用户 Account。</summary>
    [SooColumn(Name = "user_id", Length = 64)]
    public string UserId { get; set; } = "";

    [SooColumn(Name = "content", Length = 500)]
    public string Content { get; set; } = "";

    [SooColumn(Name = "sort_order")]
    public int SortOrder { get; set; }

    /// <summary>1=启用。</summary>
    [SooColumn(Name = "enabled")]
    public int Enabled { get; set; } = 1;

    [SooColumn(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [SooColumn(Name = "created_at")]
    public DateTime? CreatedAt { get; set; }
}

using mooSQL.data;

namespace SleveenAdmin.Officeman.Ebook.DBLayer;

[SooTable(EbookSchema.TableNote, Caption = "创作笔记")]
public class Sa_EbNote
{
    [SooColumn("Sa_EbNoteOID", IsPrimaryKey = true)]
    public Guid Sa_EbNoteOID { get; set; }

    [SooColumn("SYS_Created", CanBeNull = true)]
    public DateTime? SYS_Created { get; set; }

    [SooColumn("SYS_LAST_UPD", CanBeNull = true)]
    public DateTime? SYS_LAST_UPD { get; set; }

    [SooColumn("SYS_Deleted", CanBeNull = true)]
    public int? SYS_Deleted { get; set; }

    [SooColumn("SYS_CreatedBy", CanBeNull = true)]
    public Guid? SYS_CreatedBy { get; set; }

    [SooColumn("SYS_ORG", CanBeNull = true)]
    public Guid? SYS_ORG { get; set; }

    [SooColumn("SYS_LAST_UPD_BY", CanBeNull = true)]
    public Guid? SYS_LAST_UPD_BY { get; set; }

    [SooColumn("En_TopicOID", CanBeNull = true)]
    public Guid? En_TopicOID { get; set; }

    [SooColumn("En_Title", Length = 200, CanBeNull = true)]
    public string? En_Title { get; set; }

    [SooColumn("En_Content", Length = -1, CanBeNull = true)]
    public string? En_Content { get; set; }

    [SooColumn("En_CataOID", CanBeNull = true)]
    public Guid? En_CataOID { get; set; }

    [SooColumn("En_Status", Length = 20, CanBeNull = true)]
    public string? En_Status { get; set; }

    [SooColumn("En_PublishedAt", CanBeNull = true)]
    public DateTime? En_PublishedAt { get; set; }

    /// <summary>原文链接（用于爆款拆解入口）</summary>
    [SooColumn("En_SourceUrl", Length = 500, CanBeNull = true)]
    public string? En_SourceUrl { get; set; }

    [SooColumn("En_LikeCount", Length = 40, CanBeNull = true)]
    public string? En_LikeCount { get; set; }

    [SooColumn("En_BookmarkCount", Length = 40, CanBeNull = true)]
    public string? En_BookmarkCount { get; set; }

    [SooColumn("En_CommentCount", Length = 40, CanBeNull = true)]
    public string? En_CommentCount { get; set; }

    [SooColumn("En_ViewCount", Length = 40, CanBeNull = true)]
    public string? En_ViewCount { get; set; }

    [SooColumn("En_ShareCount", Length = 40, CanBeNull = true)]
    public string? En_ShareCount { get; set; }

    /// <summary>拆解报告 JSON：亮点/结构/借鉴点/运营建议等</summary>
    [SooColumn("En_ReportJson", Length = -1, CanBeNull = true)]
    public string? En_ReportJson { get; set; }
}

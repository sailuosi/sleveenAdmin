using mooSQL.data;

namespace SleveenAdmin.Officeman.Ebook.DBLayer;

[SooTable(EbookSchema.TableMat, Caption = "创作素材")]
public class Sa_EbMat
{
    [SooColumn("Sa_EbMatOID", IsPrimaryKey = true)]
    public Guid Sa_EbMatOID { get; set; }

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

    [SooColumn("Em_Title", Length = 200, CanBeNull = true)]
    public string? Em_Title { get; set; }

    [SooColumn("Em_Platform", Length = 40, CanBeNull = true)]
    public string? Em_Platform { get; set; }

    [SooColumn("Em_SourceUrl", Length = 500, CanBeNull = true)]
    public string? Em_SourceUrl { get; set; }

    [SooColumn("Em_SourceAuthor", Length = 100, CanBeNull = true)]
    public string? Em_SourceAuthor { get; set; }

    [SooColumn("Em_SourceDate", CanBeNull = true)]
    public DateTime? Em_SourceDate { get; set; }

    [SooColumn("Em_LikeCount", Length = 40, CanBeNull = true)]
    public string? Em_LikeCount { get; set; }

    [SooColumn("Em_BookmarkCount", Length = 40, CanBeNull = true)]
    public string? Em_BookmarkCount { get; set; }

    [SooColumn("Em_Summary", Length = 2000, CanBeNull = true)]
    public string? Em_Summary { get; set; }

    [SooColumn("Em_LearningPoints", Length = 4000, CanBeNull = true)]
    public string? Em_LearningPoints { get; set; }

    [SooColumn("Em_CataOID", CanBeNull = true)]
    public Guid? Em_CataOID { get; set; }

    [SooColumn("Em_Status", Length = 20, CanBeNull = true)]
    public string? Em_Status { get; set; }
}

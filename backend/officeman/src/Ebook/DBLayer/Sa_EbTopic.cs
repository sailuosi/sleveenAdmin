using mooSQL.data;

namespace SleveenAdmin.Officeman.Ebook.DBLayer;

[SooTable(EbookSchema.TableTopic, Caption = "创作选题")]
public class Sa_EbTopic
{
    [SooColumn("Sa_EbTopicOID", IsPrimaryKey = true)]
    public Guid Sa_EbTopicOID { get; set; }

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

    [SooColumn("Et_Title", Length = 200, CanBeNull = true)]
    public string? Et_Title { get; set; }

    [SooColumn("Et_Desc", Length = 2000, CanBeNull = true)]
    public string? Et_Desc { get; set; }

    [SooColumn("Et_Audience", Length = 200, CanBeNull = true)]
    public string? Et_Audience { get; set; }

    [SooColumn("Et_Format", Length = 20, CanBeNull = true)]
    public string? Et_Format { get; set; }

    [SooColumn("Et_Platforms", Length = 500, CanBeNull = true)]
    public string? Et_Platforms { get; set; }

    [SooColumn("Et_ExpectAt", CanBeNull = true)]
    public DateTime? Et_ExpectAt { get; set; }

    [SooColumn("Et_CataOID", CanBeNull = true)]
    public Guid? Et_CataOID { get; set; }

    [SooColumn("Et_Pipeline", Length = 20, CanBeNull = true)]
    public string? Et_Pipeline { get; set; }

    [SooColumn("Et_Stage", Length = 40, CanBeNull = true)]
    public string? Et_Stage { get; set; }

    [SooColumn("Et_Status", Length = 20, CanBeNull = true)]
    public string? Et_Status { get; set; }
}

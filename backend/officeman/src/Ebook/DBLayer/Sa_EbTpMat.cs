using mooSQL.data;

namespace SleveenAdmin.Officeman.Ebook.DBLayer;

[SooTable(EbookSchema.TableTpMat, Caption = "选题素材关联")]
public class Sa_EbTpMat
{
    [SooColumn("Sa_EbTpMatOID", IsPrimaryKey = true)]
    public Guid Sa_EbTpMatOID { get; set; }

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

    [SooColumn("Tm_TopicOID", CanBeNull = true)]
    public Guid? Tm_TopicOID { get; set; }

    [SooColumn("Tm_MatOID", CanBeNull = true)]
    public Guid? Tm_MatOID { get; set; }

    [SooColumn("Tm_Idx", CanBeNull = true)]
    public int? Tm_Idx { get; set; }
}

using mooSQL.data;

namespace SleveenAdmin.Officeman.Ebook.DBLayer;

[SooTable(EbookSchema.TableCata, Caption = "创作分类")]
public class Sa_EbCata
{
    [SooColumn("Sa_EbCataOID", IsPrimaryKey = true)]
    public Guid Sa_EbCataOID { get; set; }

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

    [SooColumn("Ec_Pid", CanBeNull = true)]
    public Guid? Ec_Pid { get; set; }

    [SooColumn("Ec_Name", Length = 100, CanBeNull = true)]
    public string? Ec_Name { get; set; }

    [SooColumn("Ec_Code", Length = 50, CanBeNull = true)]
    public string? Ec_Code { get; set; }

    [SooColumn("Ec_Scope", Length = 20, CanBeNull = true)]
    public string? Ec_Scope { get; set; }

    [SooColumn("Ec_Idx", CanBeNull = true)]
    public int? Ec_Idx { get; set; }

    [SooColumn("Ec_Remark", Length = 200, CanBeNull = true)]
    public string? Ec_Remark { get; set; }
}

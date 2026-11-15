using mooSQL.data;

namespace SleveenAdmin.System.Hr.DBLayer;

/// <summary>人力行政组织（岗位 OrgSrc=1 的组织主数据）。</summary>
[SooTable(Name = "HR_Organize", Caption = "人力组织", Type = DBTableType.Table)]
public class HROrganize
{
    [SooColumn(Name = "HR_OrganizeOID", IsPrimaryKey = true)]
    public Guid? HR_OrganizeOID { get; set; }

    [SooColumn(Name = "SYS_Created")]
    public DateTime? SYS_Created { get; set; }

    [SooColumn(Name = "SYS_LAST_UPD")]
    public DateTime? SYS_LAST_UPD { get; set; }

    [SooColumn(Name = "SYS_Deleted")]
    public bool? SYS_Deleted { get; set; }

    [SooColumn(Name = "O_OrgNo", Length = 50)]
    public string? O_OrgNo { get; set; }

    [SooColumn(Name = "O_OrgName", Length = 300)]
    public string? O_OrgName { get; set; }

    [SooColumn(Name = "O_NickName", Length = 300)]
    public string? O_NickName { get; set; }

    [SooColumn(Name = "O_Active")]
    public bool? O_Active { get; set; }

    [SooColumn(Name = "O_Note", Length = 500)]
    public string? O_Note { get; set; }

    [SooColumn(Name = "O_OrgCode", Length = 50)]
    public string? O_OrgCode { get; set; }

    [SooColumn(Name = "O_ClassCode", Length = 200)]
    public string? O_ClassCode { get; set; }

    [SooColumn(Name = "O_Leaf")]
    public bool? O_Leaf { get; set; }

    [SooColumn(Name = "O_Lv")]
    public int? O_Lv { get; set; }

    [SooColumn(Name = "O_Idx")]
    public int? O_Idx { get; set; }

    [SooColumn(Name = "HR_Parent_FK", CanBeNull = true)]
    public Guid? HR_Parent_FK { get; set; }
}

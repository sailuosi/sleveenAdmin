using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePageField, Caption = "页面字段")]
public class SysPageField
{
    [SooColumn("Spf_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spf_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spf_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spf_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spf_PageSelectId", CanBeNull = true)]
    public Guid? PageSelectId { get; set; }

    [SooColumn("PF_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("PF_Field", Length = 100, CanBeNull = true)]
    public string? Field { get; set; }

    [SooColumn("PF_Type", Length = 20, CanBeNull = true)]
    public string? Type { get; set; }

    [SooColumn("PF_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("PF_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

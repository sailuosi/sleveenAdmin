using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePageConnect, Caption = "页面关联")]
public class SysPageConnect
{
    [SooColumn("Spcn_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spcn_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spcn_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spcn_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spcn_PortPageId", CanBeNull = true)]
    public Guid? PortPageId { get; set; }

    [SooColumn("PCN_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("PCN_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [SooColumn("PCN_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("PCN_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

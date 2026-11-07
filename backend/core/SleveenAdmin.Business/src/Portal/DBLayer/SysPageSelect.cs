using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePageSelect, Caption = "页面选择器")]
public class SysPageSelect
{
    [SooColumn("Sps_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sps_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Sps_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Sps_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Sps_PortPageId", CanBeNull = true)]
    public Guid? PortPageId { get; set; }

    [SooColumn("PS_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("PS_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [SooColumn("PS_Sql", CanBeNull = true)]
    public string? Sql { get; set; }

    [SooColumn("PS_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("PS_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

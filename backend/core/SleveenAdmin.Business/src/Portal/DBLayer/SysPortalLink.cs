using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortalLink, Caption = "门户链接")]
public class SysPortalLink
{
    [SooColumn("Spl_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spl_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spl_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spl_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spl_PortalId", CanBeNull = true)]
    public Guid? PortalId { get; set; }

    [SooColumn("PL_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("PL_Url", Length = 500, CanBeNull = true)]
    public string? Url { get; set; }

    [SooColumn("PL_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("PL_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

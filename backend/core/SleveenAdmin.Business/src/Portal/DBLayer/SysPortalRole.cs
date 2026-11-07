using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortalRole, Caption = "门户角色")]
public class SysPortalRole
{
    [SooColumn("Spr_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spr_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spr_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spr_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spr_PortalId", CanBeNull = true)]
    public Guid? PortalId { get; set; }

    [SooColumn("PR_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("PR_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [SooColumn("PR_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("PR_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

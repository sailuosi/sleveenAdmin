using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TableAppletCata, Caption = "门户组件分类")]
public class SysAppletCata
{
    [SooColumn("Sac_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sac_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Sac_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Sac_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Sac_Pid", CanBeNull = true)]
    public Guid? Pid { get; set; }

    [SooColumn("AC_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("AC_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [SooColumn("AC_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("AC_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

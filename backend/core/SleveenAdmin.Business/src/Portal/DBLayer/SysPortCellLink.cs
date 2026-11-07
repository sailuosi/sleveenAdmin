using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortCellLink, Caption = "门户区块组件关联")]
public class SysPortCellLink
{
    [SooColumn("Spcl_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spcl_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spcl_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spcl_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spcl_PortalId", CanBeNull = true)]
    public Guid? PortalId { get; set; }

    [SooColumn("Spcl_PortCellId", CanBeNull = true)]
    public Guid? PortCellId { get; set; }

    [SooColumn("Spcl_AppletId", CanBeNull = true)]
    public Guid? AppletId { get; set; }

    [SooColumn("CL_Link", Length = 50, CanBeNull = true)]
    public string? Link { get; set; }

    [SooColumn("CL_Key", CanBeNull = true)]
    public string? Key { get; set; }

    [SooColumn("CL_Title", Length = 50, CanBeNull = true)]
    public string? Title { get; set; }

    [SooColumn("CL_TitleAlign", Length = 20, CanBeNull = true)]
    public string? TitleAlign { get; set; }

    [SooColumn("CL_TitleStyle", CanBeNull = true)]
    public string? TitleStyle { get; set; }

    [SooColumn("CL_Type", Length = 20, CanBeNull = true)]
    public string? Type { get; set; }

    [SooColumn("CL_Para", Length = 2000, CanBeNull = true)]
    public string? Para { get; set; }

    [SooColumn("CL_Para2", CanBeNull = true)]
    public string? Para2 { get; set; }

    [SooColumn("CL_ParaVal", CanBeNull = true)]
    public string? ParaVal { get; set; }

    [SooColumn("CL_BodyStyle", CanBeNull = true)]
    public string? BodyStyle { get; set; }

    [SooColumn("CL_Icon", Length = 50, CanBeNull = true)]
    public string? Icon { get; set; }

    [SooColumn("CL_Tool", Length = 50, CanBeNull = true)]
    public string? Tool { get; set; }

    [SooColumn("CL_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("CL_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [SooColumn("CL_Slot", Length = 50, CanBeNull = true)]
    public string? Slot { get; set; }

    [SooColumn("CL_Color", Length = 50, CanBeNull = true)]
    public string? Color { get; set; }

    [SooColumn("CL_Theme", Length = 50, CanBeNull = true)]
    public string? Theme { get; set; }

    [SooColumn("CL_Disabled", CanBeNull = true)]
    public bool? Disabled { get; set; }

    [SooColumn("CL_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

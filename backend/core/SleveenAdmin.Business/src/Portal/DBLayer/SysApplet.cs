using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TableApplet, Caption = "门户组件")]
public class SysApplet
{
    [SooColumn("Sa_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Sa_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Sa_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Sa_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Sa_AppletCataId", CanBeNull = true)]
    public Guid? AppletCataId { get; set; }

    [SooColumn("A_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [SooColumn("A_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("A_Key", Length = 100, CanBeNull = true)]
    public string? Key { get; set; }

    [SooColumn("A_Type", Length = 20, CanBeNull = true)]
    public string? Type { get; set; }

    [SooColumn("A_Title", Length = 100, CanBeNull = true)]
    public string? Title { get; set; }

    [SooColumn("A_TitleAlign", Length = 20, CanBeNull = true)]
    public string? TitleAlign { get; set; }

    [SooColumn("A_Para", CanBeNull = true)]
    public string? Para { get; set; }

    [SooColumn("A_Url", Length = 500, CanBeNull = true)]
    public string? Url { get; set; }

    [SooColumn("A_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("A_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

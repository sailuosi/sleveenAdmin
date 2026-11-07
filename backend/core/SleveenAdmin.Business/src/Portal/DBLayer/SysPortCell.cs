using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortCell, Caption = "门户区块")]
public class SysPortCell
{
    [SooColumn("Spc_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Spc_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [SooColumn("Spc_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [SooColumn("Spc_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [SooColumn("Spc_PortalId", CanBeNull = true)]
    public Guid? PortalId { get; set; }

    [SooColumn("PC_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [SooColumn("PC_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [SooColumn("PC_Name", Length = 50, CanBeNull = true)]
    public string? Name { get; set; }

    [SooColumn("PC_X", Length = 50, CanBeNull = true)]
    public string? X { get; set; }

    [SooColumn("PC_Y", Length = 50, CanBeNull = true)]
    public string? Y { get; set; }

    [SooColumn("PC_W", Length = 50, CanBeNull = true)]
    public string? W { get; set; }

    [SooColumn("PC_H", Length = 50, CanBeNull = true)]
    public string? H { get; set; }

    [SooColumn("PC_I", Length = 50, CanBeNull = true)]
    public string? I { get; set; }

    [SooColumn("PC_Margin", Length = 50, CanBeNull = true)]
    public string? Margin { get; set; }

    [SooColumn("PC_Padding", Length = 50, CanBeNull = true)]
    public string? Padding { get; set; }

    [SooColumn("PC_Style", CanBeNull = true)]
    public string? Style { get; set; }

    [SooColumn("PC_Css", Length = 50, CanBeNull = true)]
    public string? Css { get; set; }

    [SooColumn("PC_Theme", Length = 50, CanBeNull = true)]
    public string? Theme { get; set; }

    [SooColumn("PC_Key", Length = 50, CanBeNull = true)]
    public string? Key { get; set; }

    [SooColumn("PC_Many", Length = 10, CanBeNull = true)]
    public string? Many { get; set; }

    [SooColumn("PC_TitleAlign", Length = 20, CanBeNull = true)]
    public string? TitleAlign { get; set; }

    [SooColumn("PC_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }

    [SooColumn("PC_CellPara", CanBeNull = true)]
    public string? CellPara { get; set; }
}

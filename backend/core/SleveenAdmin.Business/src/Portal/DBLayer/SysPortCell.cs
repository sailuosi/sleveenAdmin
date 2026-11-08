using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortCell, Caption = "门户区块")]
public class SysPortCell
{
    [JsonPropertyName("spcId")]
    [SooColumn("Spc_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("SYS_Created")]
    [SooColumn("Spc_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    [SooColumn("Spc_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [JsonPropertyName("SYS_Deleted")]
    [SooColumn("Spc_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [JsonPropertyName("spPortalId")]
    [SooColumn("Spc_PortalId", CanBeNull = true)]
    public Guid? PortalId { get; set; }

    [JsonPropertyName("PC_Idx")]
    [SooColumn("PC_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [JsonPropertyName("PC_IsOn")]
    [SooColumn("PC_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [JsonPropertyName("PC_Name")]
    [SooColumn("PC_Name", Length = 50, CanBeNull = true)]
    public string? Name { get; set; }

    [JsonPropertyName("PC_X")]
    [SooColumn("PC_X", Length = 50, CanBeNull = true)]
    public string? X { get; set; }

    [JsonPropertyName("PC_Y")]
    [SooColumn("PC_Y", Length = 50, CanBeNull = true)]
    public string? Y { get; set; }

    [JsonPropertyName("PC_W")]
    [SooColumn("PC_W", Length = 50, CanBeNull = true)]
    public string? W { get; set; }

    [JsonPropertyName("PC_H")]
    [SooColumn("PC_H", Length = 50, CanBeNull = true)]
    public string? H { get; set; }

    [JsonPropertyName("PC_I")]
    [SooColumn("PC_I", Length = 50, CanBeNull = true)]
    public string? I { get; set; }

    [JsonPropertyName("PC_Margin")]
    [SooColumn("PC_Margin", Length = 50, CanBeNull = true)]
    public string? Margin { get; set; }

    [JsonPropertyName("PC_Padding")]
    [SooColumn("PC_Padding", Length = 50, CanBeNull = true)]
    public string? Padding { get; set; }

    [JsonPropertyName("PC_Style")]
    [SooColumn("PC_Style", CanBeNull = true)]
    public string? Style { get; set; }

    [JsonPropertyName("PC_Css")]
    [SooColumn("PC_Css", Length = 50, CanBeNull = true)]
    public string? Css { get; set; }

    [JsonPropertyName("PC_Theme")]
    [SooColumn("PC_Theme", Length = 50, CanBeNull = true)]
    public string? Theme { get; set; }

    [JsonPropertyName("PC_Key")]
    [SooColumn("PC_Key", Length = 50, CanBeNull = true)]
    public string? Key { get; set; }

    [JsonPropertyName("PC_Many")]
    [SooColumn("PC_Many", Length = 10, CanBeNull = true)]
    public string? Many { get; set; }

    [JsonPropertyName("PC_TitleAlign")]
    [SooColumn("PC_TitleAlign", Length = 20, CanBeNull = true)]
    public string? TitleAlign { get; set; }

    [JsonPropertyName("PC_Remark")]
    [SooColumn("PC_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }

    [JsonPropertyName("PC_CellPara")]
    [SooColumn("PC_CellPara", CanBeNull = true)]
    public string? CellPara { get; set; }
}

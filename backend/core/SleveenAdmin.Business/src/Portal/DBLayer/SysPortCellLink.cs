using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortCellLink, Caption = "门户区块组件关联")]
public class SysPortCellLink
{
    [JsonPropertyName("spclId")]
    [SooColumn("Spcl_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("SYS_Created")]
    [SooColumn("Spcl_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    [SooColumn("Spcl_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [JsonPropertyName("SYS_Deleted")]
    [SooColumn("Spcl_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [JsonPropertyName("spPortalId")]
    [SooColumn("Spcl_PortalId", CanBeNull = true)]
    public Guid? PortalId { get; set; }

    [JsonPropertyName("spPortCellId")]
    [SooColumn("Spcl_PortCellId", CanBeNull = true)]
    public Guid? PortCellId { get; set; }

    [JsonPropertyName("saAppletId")]
    [SooColumn("Spcl_AppletId", CanBeNull = true)]
    public Guid? AppletId { get; set; }

    [JsonPropertyName("CL_Link")]
    [SooColumn("CL_Link", Length = 50, CanBeNull = true)]
    public string? Link { get; set; }

    [JsonPropertyName("CL_Key")]
    [SooColumn("CL_Key", CanBeNull = true)]
    public string? Key { get; set; }

    [JsonPropertyName("CL_Title")]
    [SooColumn("CL_Title", Length = 50, CanBeNull = true)]
    public string? Title { get; set; }

    [JsonPropertyName("CL_TitleAlign")]
    [SooColumn("CL_TitleAlign", Length = 20, CanBeNull = true)]
    public string? TitleAlign { get; set; }

    [JsonPropertyName("CL_TitleStyle")]
    [SooColumn("CL_TitleStyle", CanBeNull = true)]
    public string? TitleStyle { get; set; }

    [JsonPropertyName("CL_Type")]
    [SooColumn("CL_Type", Length = 20, CanBeNull = true)]
    public string? Type { get; set; }

    [JsonPropertyName("CL_Para")]
    [SooColumn("CL_Para", Length = 2000, CanBeNull = true)]
    public string? Para { get; set; }

    [JsonPropertyName("CL_Para2")]
    [SooColumn("CL_Para2", CanBeNull = true)]
    public string? Para2 { get; set; }

    [JsonPropertyName("CL_ParaVal")]
    [SooColumn("CL_ParaVal", CanBeNull = true)]
    public string? ParaVal { get; set; }

    [JsonPropertyName("CL_BodyStyle")]
    [SooColumn("CL_BodyStyle", CanBeNull = true)]
    public string? BodyStyle { get; set; }

    [JsonPropertyName("CL_Icon")]
    [SooColumn("CL_Icon", Length = 50, CanBeNull = true)]
    public string? Icon { get; set; }

    [JsonPropertyName("CL_Tool")]
    [SooColumn("CL_Tool", Length = 50, CanBeNull = true)]
    public string? Tool { get; set; }

    [JsonPropertyName("CL_Idx")]
    [SooColumn("CL_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [JsonPropertyName("CL_IsOn")]
    [SooColumn("CL_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [JsonPropertyName("CL_Slot")]
    [SooColumn("CL_Slot", Length = 50, CanBeNull = true)]
    public string? Slot { get; set; }

    [JsonPropertyName("CL_Color")]
    [SooColumn("CL_Color", Length = 50, CanBeNull = true)]
    public string? Color { get; set; }

    [JsonPropertyName("CL_Theme")]
    [SooColumn("CL_Theme", Length = 50, CanBeNull = true)]
    public string? Theme { get; set; }

    [JsonPropertyName("CL_Disabled")]
    [SooColumn("CL_Disabled", CanBeNull = true)]
    public bool? Disabled { get; set; }

    [JsonPropertyName("CL_Remark")]
    [SooColumn("CL_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}

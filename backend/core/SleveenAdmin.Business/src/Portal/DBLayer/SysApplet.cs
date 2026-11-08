using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TableApplet, Caption = "门户组件")]
public class SysApplet
{
    [JsonPropertyName("saId")]
    [SooColumn("Sa_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("SYS_Created")]
    [SooColumn("Sa_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    [SooColumn("Sa_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [JsonPropertyName("SYS_Deleted")]
    [SooColumn("Sa_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [JsonPropertyName("saAppletCataId")]
    [SooColumn("Sa_AppletCataId", CanBeNull = true)]
    public Guid? AppletCataId { get; set; }

    [JsonPropertyName("A_IsOn")]
    [SooColumn("A_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [JsonPropertyName("A_Name")]
    [SooColumn("A_Name", Length = 100, CanBeNull = true)]
    public string? Name { get; set; }

    [JsonPropertyName("A_Key")]
    [SooColumn("A_Key", Length = 100, CanBeNull = true)]
    public string? Key { get; set; }

    [JsonPropertyName("A_Type")]
    [SooColumn("A_Type", Length = 20, CanBeNull = true)]
    public string? Type { get; set; }

    [JsonPropertyName("A_Title")]
    [SooColumn("A_Title", Length = 100, CanBeNull = true)]
    public string? Title { get; set; }

    [JsonPropertyName("A_TitleAlign")]
    [SooColumn("A_TitleAlign", Length = 20, CanBeNull = true)]
    public string? TitleAlign { get; set; }

    [JsonPropertyName("A_Para")]
    [SooColumn("A_Para", CanBeNull = true)]
    public string? Para { get; set; }

    [JsonPropertyName("A_Url")]
    [SooColumn("A_Url", Length = 500, CanBeNull = true)]
    public string? Url { get; set; }

    [JsonPropertyName("A_Idx")]
    [SooColumn("A_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [JsonPropertyName("A_Remark")]
    [SooColumn("A_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }
}


using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortPage, Caption = "门户页面")]
public class SysPortPage
{
    [JsonPropertyName("sppId")]
    [SooColumn("Spp_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("SYS_Created")]
    [SooColumn("Spp_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    [SooColumn("Spp_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [JsonPropertyName("SYS_Deleted")]
    [SooColumn("Spp_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [JsonPropertyName("spOrgId")]
    [SooColumn("Spp_OrgId", CanBeNull = true)]
    public Guid? OrgId { get; set; }

    [JsonPropertyName("PP_Title")]
    [SooColumn("PP_Title", Length = 500, CanBeNull = true)]
    public string? Title { get; set; }

    [JsonPropertyName("PP_Theme")]
    [SooColumn("PP_Theme", Length = 10, CanBeNull = true)]
    public string? Theme { get; set; }

    [JsonPropertyName("PP_Disabled")]
    [SooColumn("PP_Disabled", Length = 10, CanBeNull = true)]
    public string? Disabled { get; set; }

    [JsonPropertyName("PP_Style")]
    [SooColumn("PP_Style", CanBeNull = true)]
    public string? Style { get; set; }

    [JsonPropertyName("PP_Css")]
    [SooColumn("PP_Css", Length = 500, CanBeNull = true)]
    public string? Css { get; set; }

    [JsonPropertyName("PP_Pose")]
    [SooColumn("PP_Pose", Length = 10, CanBeNull = true)]
    public string? Pose { get; set; }

    [JsonPropertyName("PP_Code")]
    [SooColumn("PP_Code", Length = 50, CanBeNull = true)]
    public string? Code { get; set; }

    [JsonPropertyName("PP_AuthCode")]
    [SooColumn("PP_AuthCode", Length = 50, CanBeNull = true)]
    public string? AuthCode { get; set; }

    [JsonPropertyName("PP_Para")]
    [SooColumn("PP_Para", CanBeNull = true)]
    public string? Para { get; set; }

    [JsonPropertyName("PP_Note")]
    [SooColumn("PP_Note", Length = 500, CanBeNull = true)]
    public string? Note { get; set; }

    [JsonPropertyName("PP_Dept")]
    [SooColumn("PP_Dept", Length = 200, CanBeNull = true)]
    public string? Dept { get; set; }

    [JsonPropertyName("PP_IsOn")]
    [SooColumn("PP_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [JsonPropertyName("PP_OrgName")]
    [SooColumn("PP_OrgName", Length = 500, CanBeNull = true)]
    public string? OrgName { get; set; }

    [JsonPropertyName("PP_Idx")]
    [SooColumn("PP_Idx", CanBeNull = true)]
    public int? Idx { get; set; }
}

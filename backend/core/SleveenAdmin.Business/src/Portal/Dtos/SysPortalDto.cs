using System.Text.Json.Serialization;

namespace SleveenAdmin.Business.Portal.Dtos;

/// <summary>门户列表/详情；属性名对齐 minehub 前端（P_Name / spId）。</summary>
public sealed class SysPortalDto
{
    [JsonPropertyName("spId")]
    public Guid SpId { get; set; }

    [JsonPropertyName("spOrgId")]
    public Guid? SpOrgId { get; set; }

    [JsonPropertyName("spUserId")]
    public Guid? SpUserId { get; set; }

    [JsonPropertyName("spPortPageId")]
    public Guid? SpPortPageId { get; set; }

    [JsonPropertyName("spSubjectId")]
    public Guid? SpSubjectId { get; set; }

    [JsonPropertyName("P_IsOn")]
    public bool? PIsOn { get; set; }

    [JsonPropertyName("P_Name")]
    public string? PName { get; set; }

    [JsonPropertyName("P_Note")]
    public string? PNote { get; set; }

    [JsonPropertyName("P_Theme")]
    public string? PTheme { get; set; }

    [JsonPropertyName("P_Idx")]
    public int? PIdx { get; set; }

    [JsonPropertyName("P_Group")]
    public string? PGroup { get; set; }

    [JsonPropertyName("P_ShowTitle")]
    public string? PShowTitle { get; set; }

    [JsonPropertyName("P_Style")]
    public string? PStyle { get; set; }

    [JsonPropertyName("P_Icon")]
    public string? PIcon { get; set; }

    [JsonPropertyName("P_Css")]
    public string? PCss { get; set; }

    [JsonPropertyName("P_Display")]
    public string? PDisplay { get; set; }

    [JsonPropertyName("P_ColNum")]
    public string? PColNum { get; set; }

    [JsonPropertyName("P_Rows")]
    public string? PRows { get; set; }

    [JsonPropertyName("P_Editable")]
    public bool? PEditable { get; set; }

    [JsonPropertyName("P_Remark")]
    public string? PRemark { get; set; }

    [JsonPropertyName("P_Task")]
    public string? PTask { get; set; }

    [JsonPropertyName("P_Key")]
    public string? PKey { get; set; }

    [JsonPropertyName("P_Pulic")]
    public string? PPulic { get; set; }

    [JsonPropertyName("P_Para")]
    public string? PPara { get; set; }

    [JsonPropertyName("PP_Title")]
    public string? PpTitle { get; set; }

    [JsonPropertyName("OrgName")]
    public string? OrgName { get; set; }

    [JsonPropertyName("SYS_Created")]
    public DateTime? SysCreated { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    public DateTime? SysLastUpd { get; set; }
}

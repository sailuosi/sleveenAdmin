using System.Text.Json.Serialization;
using mooSQL.data;
using SleveenAdmin.Business.Portal.Dtos;

namespace SleveenAdmin.Business.Portal.DBLayer;

[SooTable(PortalSchema.TablePortal, Caption = "门户配置")]
public class SysPortal
{
    /// <summary>主键；前端表单字段 spId</summary>
    [JsonPropertyName("spId")]
    [SooColumn("Sp_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("SYS_Created")]
    [SooColumn("Sp_CreateTime", CanBeNull = true)]
    public DateTime? CreateTime { get; set; }

    [JsonPropertyName("SYS_LAST_UPD")]
    [SooColumn("Sp_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }

    [JsonPropertyName("SYS_Deleted")]
    [SooColumn("Sp_Deleted", CanBeNull = true)]
    public bool? Deleted { get; set; }

    [JsonPropertyName("spOrgId")]
    [SooColumn("Sp_OrgId", CanBeNull = true)]
    public Guid? OrgId { get; set; }

    [JsonPropertyName("spUserId")]
    [SooColumn("Sp_UserId", CanBeNull = true)]
    public Guid? UserId { get; set; }

    [JsonPropertyName("spPortPageId")]
    [SooColumn("Sp_PortPageId", CanBeNull = true)]
    public Guid? PortPageId { get; set; }

    [JsonPropertyName("spSubjectId")]
    [SooColumn("Sp_SubjectId", CanBeNull = true)]
    public Guid? SubjectId { get; set; }

    [JsonPropertyName("P_IsOn")]
    [SooColumn("P_IsOn", CanBeNull = true)]
    public bool? IsOn { get; set; }

    [JsonPropertyName("P_Name")]
    [SooColumn("P_Name", Length = 50, CanBeNull = true)]
    public string? Name { get; set; }

    [JsonPropertyName("P_Note")]
    [SooColumn("P_Note", Length = 50, CanBeNull = true)]
    public string? Note { get; set; }

    [JsonPropertyName("P_Theme")]
    [SooColumn("P_Theme", Length = 50, CanBeNull = true)]
    public string? Theme { get; set; }

    [JsonPropertyName("P_Idx")]
    [SooColumn("P_Idx", CanBeNull = true)]
    public int? Idx { get; set; }

    [JsonPropertyName("P_Group")]
    [SooColumn("P_Group", Length = 50, CanBeNull = true)]
    public string? Group { get; set; }

    [JsonPropertyName("P_ShowTitle")]
    [SooColumn("P_ShowTitle", Length = 10, CanBeNull = true)]
    public string? ShowTitle { get; set; }

    [JsonPropertyName("P_Style")]
    [SooColumn("P_Style", CanBeNull = true)]
    public string? Style { get; set; }

    [JsonPropertyName("P_Icon")]
    [SooColumn("P_Icon", Length = 50, CanBeNull = true)]
    public string? Icon { get; set; }

    [JsonPropertyName("P_Css")]
    [SooColumn("P_Css", Length = 50, CanBeNull = true)]
    public string? Css { get; set; }

    [JsonPropertyName("P_Display")]
    [SooColumn("P_Display", Length = 10, CanBeNull = true)]
    public string? Display { get; set; }

    [JsonPropertyName("P_ColNum")]
    [JsonConverter(typeof(FlexibleStringJsonConverter))]
    [SooColumn("P_ColNum", Length = 50, CanBeNull = true)]
    public string? ColNum { get; set; }

    [JsonPropertyName("P_Rows")]
    [JsonConverter(typeof(FlexibleStringJsonConverter))]
    [SooColumn("P_Rows", Length = 50, CanBeNull = true)]
    public string? Rows { get; set; }

    [JsonPropertyName("P_Editable")]
    [SooColumn("P_Editable", CanBeNull = true)]
    public bool? Editable { get; set; }

    [JsonPropertyName("P_Remark")]
    [SooColumn("P_Remark", Length = 200, CanBeNull = true)]
    public string? Remark { get; set; }

    [JsonPropertyName("P_Task")]
    [SooColumn("P_Task", Length = 10, CanBeNull = true)]
    public string? Task { get; set; }

    [JsonPropertyName("P_Key")]
    [SooColumn("P_Key", Length = 50, CanBeNull = true)]
    public string? Key { get; set; }

    [JsonPropertyName("P_Pulic")]
    [SooColumn("P_Pulic", Length = 10, CanBeNull = true)]
    public string? Pulic { get; set; }

    [JsonPropertyName("P_Para")]
    [SooColumn("P_Para", Length = 500, CanBeNull = true)]
    public string? Para { get; set; }
}

using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.System.SysPost.DBLayer;

/// <summary>岗位（同构 minehub HH_DivWorker / 单位管理员）。</summary>
[SooTable(Name = "Sys_Post", Caption = "岗位", Type = DBTableType.Table)]
public class SysPostEntity
{
    [JsonPropertyName("P_Id")]
    [SooColumn(Name = "P_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("P_Name")]
    [SooColumn(Name = "P_Name", Length = 50)]
    public string? Name { get; set; }

    [JsonPropertyName("P_Card")]
    [SooColumn(Name = "P_Card", Length = 100)]
    public string? Card { get; set; }

    [JsonPropertyName("P_IsOn")]
    [SooColumn(Name = "P_IsOn")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("P_Note")]
    [SooColumn(Name = "P_Note", Length = 500)]
    public string? Note { get; set; }

    [JsonPropertyName("P_Remark")]
    [SooColumn(Name = "P_Remark", Length = 50)]
    public string? Remark { get; set; }

    [JsonPropertyName("P_Order")]
    [SooColumn(Name = "P_Order")]
    public int? Order { get; set; }

    [JsonPropertyName("P_Status")]
    [SooColumn(Name = "P_Status", Length = 10)]
    public string? Status { get; set; }

    [JsonPropertyName("P_IsMain")]
    [SooColumn(Name = "P_IsMain")]
    public bool? IsMain { get; set; }

    [JsonPropertyName("P_IsDuty")]
    [SooColumn(Name = "P_IsDuty")]
    public bool? IsDuty { get; set; }

    [JsonPropertyName("P_GiveMan")]
    [SooColumn(Name = "P_GiveMan", Length = 50)]
    public string? GiveMan { get; set; }

    [JsonPropertyName("P_GiveManCode")]
    [SooColumn(Name = "P_GiveManCode", Length = 50)]
    public string? GiveManCode { get; set; }

    [JsonPropertyName("P_UserId")]
    [SooColumn(Name = "P_UserId", CanBeNull = true)]
    public Guid? UserId { get; set; }

    [JsonPropertyName("P_OrgId")]
    [SooColumn(Name = "P_OrgId", CanBeNull = true)]
    public Guid? OrgId { get; set; }

    [JsonPropertyName("P_OrgSrc")]
    [SooColumn(Name = "P_OrgSrc", Length = 20)]
    public string? OrgSrc { get; set; }

    [JsonPropertyName("P_OrgName")]
    [SooColumn(Name = "P_OrgName", Length = 300)]
    public string? OrgName { get; set; }

    [JsonPropertyName("P_Deleted")]
    [SooColumn(Name = "P_Deleted")]
    public bool? Deleted { get; set; }

    [JsonPropertyName("P_Created")]
    [SooColumn(Name = "P_Created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("P_Updated")]
    [SooColumn(Name = "P_Updated")]
    public DateTime? Updated { get; set; }

    [JsonPropertyName("P_CreatedBy")]
    [SooColumn(Name = "P_CreatedBy", CanBeNull = true)]
    public Guid? CreatedBy { get; set; }

    [JsonPropertyName("P_UpdatedBy")]
    [SooColumn(Name = "P_UpdatedBy", CanBeNull = true)]
    public Guid? UpdatedBy { get; set; }
}

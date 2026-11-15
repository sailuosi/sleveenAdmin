using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.System.SysPost.DBLayer;

[SooTable(Name = "Sys_PostType", Caption = "岗位类别", Type = DBTableType.Table)]
public class SysPostType
{
    [JsonPropertyName("Pt_Id")]
    [SooColumn(Name = "Pt_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("Pt_IsOn")]
    [SooColumn(Name = "Pt_IsOn")]
    public bool? IsOn { get; set; }

    [JsonPropertyName("Pt_Title")]
    [SooColumn(Name = "Pt_Title", Length = 100)]
    public string? Title { get; set; }

    [JsonPropertyName("Pt_Note")]
    [SooColumn(Name = "Pt_Note", Length = 500)]
    public string? Note { get; set; }

    [JsonPropertyName("Pt_Remark")]
    [SooColumn(Name = "Pt_Remark", Length = 50)]
    public string? Remark { get; set; }

    [JsonPropertyName("Pt_Created")]
    [SooColumn(Name = "Pt_Created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("Pt_CreateMan")]
    [SooColumn(Name = "Pt_CreateMan", Length = 50)]
    public string? CreateMan { get; set; }

    [JsonPropertyName("Pt_Order")]
    [SooColumn(Name = "Pt_Order")]
    public int? Order { get; set; }

    [JsonPropertyName("Pt_CreateUserId")]
    [SooColumn(Name = "Pt_CreateUserId", CanBeNull = true)]
    public Guid? CreateUserId { get; set; }

    [JsonPropertyName("Pt_OSrc")]
    [SooColumn(Name = "Pt_OSrc", Length = 20)]
    public string? OSrc { get; set; }

    [JsonPropertyName("Pt_IncludeChild")]
    [SooColumn(Name = "Pt_IncludeChild")]
    public bool? IncludeChild { get; set; }

    [JsonPropertyName("Pt_Cust")]
    [SooColumn(Name = "Pt_Cust", Length = 50)]
    public string? Cust { get; set; }
}

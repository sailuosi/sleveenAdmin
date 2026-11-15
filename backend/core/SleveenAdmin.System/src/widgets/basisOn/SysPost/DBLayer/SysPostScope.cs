using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.System.SysPost.DBLayer;

[SooTable(Name = "Sys_PostScope", Caption = "岗位授权范围", Type = DBTableType.Table)]
public class SysPostScope
{
    [JsonPropertyName("Ps_Id")]
    [SooColumn(Name = "Ps_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("Ps_InSrc")]
    [SooColumn(Name = "Ps_InSrc", Length = 10)]
    public string? InSrc { get; set; }

    [JsonPropertyName("Ps_PostTypeId")]
    [SooColumn(Name = "Ps_PostTypeId", CanBeNull = true)]
    public Guid? PostTypeId { get; set; }

    [JsonPropertyName("Ps_PostId")]
    [SooColumn(Name = "Ps_PostId", CanBeNull = true)]
    public Guid? PostId { get; set; }

    [JsonPropertyName("Ps_OrgSrc")]
    [SooColumn(Name = "Ps_OrgSrc", Length = 20)]
    public string? OrgSrc { get; set; }

    [JsonPropertyName("Ps_OrgId")]
    [SooColumn(Name = "Ps_OrgId", CanBeNull = true)]
    public Guid? OrgId { get; set; }

    [JsonPropertyName("Ps_UserId")]
    [SooColumn(Name = "Ps_UserId", CanBeNull = true)]
    public Guid? UserId { get; set; }
}

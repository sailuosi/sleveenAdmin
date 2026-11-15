using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.System.SysPost.DBLayer;

[SooTable(Name = "Sys_PostTypeChild", Caption = "下级岗位类别", Type = DBTableType.Table)]
public class SysPostTypeChild
{
    [JsonPropertyName("Pc_Id")]
    [SooColumn(Name = "Pc_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("Pc_BossId")]
    [SooColumn(Name = "Pc_BossId", CanBeNull = true)]
    public Guid? BossId { get; set; }

    [JsonPropertyName("Pc_PostTypeId")]
    [SooColumn(Name = "Pc_PostTypeId", CanBeNull = true)]
    public Guid? PostTypeId { get; set; }
}

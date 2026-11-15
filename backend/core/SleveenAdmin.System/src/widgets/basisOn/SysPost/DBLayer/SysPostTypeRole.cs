using System.Text.Json.Serialization;
using mooSQL.data;

namespace SleveenAdmin.System.SysPost.DBLayer;

[SooTable(Name = "Sys_PostTypeRole", Caption = "岗位类别角色", Type = DBTableType.Table)]
public class SysPostTypeRole
{
    [JsonPropertyName("Pr_Id")]
    [SooColumn(Name = "Pr_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [JsonPropertyName("Pr_PostTypeId")]
    [SooColumn(Name = "Pr_PostTypeId", CanBeNull = true)]
    public Guid? PostTypeId { get; set; }

    [JsonPropertyName("Pr_DutyItemId")]
    [SooColumn(Name = "Pr_DutyItemId", CanBeNull = true)]
    public Guid? DutyItemId { get; set; }
}

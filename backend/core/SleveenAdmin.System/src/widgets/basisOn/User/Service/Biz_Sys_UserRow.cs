using System.Text.Json.Serialization;
using mooSQL.data;
using SleveenAdmin.System.User.DBLayer;

namespace SleveenAdmin.System.User.Service;

[SooTable(Name = "SysUser", Alias = "mtb", Type = DBTableType.Select)]
public class BizSysUserRow : SysUser
{
    [JsonPropertyName("password")]
    public string? PasswordInput { get; set; }
}

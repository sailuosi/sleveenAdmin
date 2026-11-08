using System.Text.Json.Serialization;
using mooSQL.data;
using SleveenAdmin.System.User.DBLayer;

namespace SleveenAdmin.System.User.Service;

[SooTable(Name = "SysUser", Alias = "mtb", Type = DBTableType.Select)]
public class BizSysUserRow : SysUser
{
    /// <summary>哈希密码不参与 JSON；写库仍走此列。</summary>
    [JsonIgnore]
    [SooColumn("Su_Password", Length = 256, CanBeNull = false)]
    public new string Password
    {
        get => base.Password;
        set => base.Password = value;
    }

    /// <summary>前端提交的明文密码（JSON: password）。</summary>
    [JsonPropertyName("password")]
    public string? PasswordInput { get; set; }
}

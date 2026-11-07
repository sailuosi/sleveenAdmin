using mooSQL.data;

namespace SleveenAdmin.System.User.DBLayer;

[SooTable("SysUser", Caption = "系统用户")]
public class SysUser
{
    [SooColumn("Su_Id", IsPrimaryKey = true)]
    public Guid Id { get; set; }

    [SooColumn("Su_Account", Length = 64, CanBeNull = false)]
    public string Account { get; set; } = string.Empty;

    [SooColumn("Su_Password", Length = 256, CanBeNull = false)]
    public string Password { get; set; } = string.Empty;

    [SooColumn("Su_RealName", Length = 64, CanBeNull = true)]
    public string? RealName { get; set; }

    [SooColumn("Su_Status", CanBeNull = false)]
    public int Status { get; set; } = 1;

    [SooColumn("Su_OrgId", CanBeNull = true)]
    public Guid? OrgId { get; set; }

    [SooColumn("Su_TenantId", CanBeNull = true)]
    public Guid? TenantId { get; set; }

    [SooColumn("Su_CreateTime", CanBeNull = false)]
    public DateTime CreateTime { get; set; }

    [SooColumn("Su_UpdateTime", CanBeNull = true)]
    public DateTime? UpdateTime { get; set; }
}

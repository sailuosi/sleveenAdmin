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

    [SooColumn("Su_NickName", Length = 64, CanBeNull = true)]
    public string? NickName { get; set; }

    [SooColumn("Su_Avatar", Length = 512, CanBeNull = true)]
    public string? Avatar { get; set; }

    [SooColumn("Su_Signature", Length = 512, CanBeNull = true)]
    public string? Signature { get; set; }

    [SooColumn("Su_Phone", Length = 32, CanBeNull = true)]
    public string? Phone { get; set; }

    [SooColumn("Su_Email", Length = 128, CanBeNull = true)]
    public string? Email { get; set; }

    [SooColumn("Su_Sex", CanBeNull = true)]
    public int? Sex { get; set; }

    [SooColumn("Su_Birthday", CanBeNull = true)]
    public DateTime? Birthday { get; set; }

    [SooColumn("Su_Address", Length = 512, CanBeNull = true)]
    public string? Address { get; set; }

    [SooColumn("Su_Remark", Length = 512, CanBeNull = true)]
    public string? Remark { get; set; }

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

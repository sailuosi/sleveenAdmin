namespace SleveenAdmin.System.User.Dtos;

/// <summary>个人中心基本信息（与前端 userCenter / 待办签名约定对齐）</summary>
public sealed class SysUserBaseInfoDto
{
    public Guid Id { get; set; }

    public string Account { get; set; } = string.Empty;

    public string? RealName { get; set; }

    public string? NickName { get; set; }

    public string? Avatar { get; set; }

    public string? Signature { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? Sex { get; set; }

    public DateTime? Birthday { get; set; }

    public string? Address { get; set; }

    public string? Remark { get; set; }
}

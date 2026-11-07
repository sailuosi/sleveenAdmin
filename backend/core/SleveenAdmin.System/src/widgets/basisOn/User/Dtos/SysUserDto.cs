namespace SleveenAdmin.System.User.Dtos;

public sealed class SysUserDto
{
    public Guid Id { get; set; }

    public string Account { get; set; } = string.Empty;

    public string? RealName { get; set; }

    public int Status { get; set; }

    public Guid? OrgId { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime? UpdateTime { get; set; }
}

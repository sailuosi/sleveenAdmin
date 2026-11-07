namespace SleveenAdmin.System.User.Dtos;

public sealed class SysUserInput
{
    public Guid? Id { get; set; }

    public string Account { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? RealName { get; set; }

    public int Status { get; set; } = 1;

    public Guid? OrgId { get; set; }

    public Guid? TenantId { get; set; }
}

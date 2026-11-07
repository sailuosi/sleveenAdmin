namespace SleveenAdmin.System.Tenant.Dtos;

public sealed class SysTenantDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int Status { get; set; }

    public string? Remark { get; set; }

    public DateTime CreateTime { get; set; }
}

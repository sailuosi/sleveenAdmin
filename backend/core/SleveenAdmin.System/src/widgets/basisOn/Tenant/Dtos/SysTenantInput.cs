namespace SleveenAdmin.System.Tenant.Dtos;

public sealed class SysTenantInput
{
    public Guid? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int Status { get; set; } = 1;

    public string? Remark { get; set; }
}

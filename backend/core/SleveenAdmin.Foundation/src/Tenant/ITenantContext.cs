namespace SleveenAdmin.Foundation.Tenant;

public interface ITenantContext
{
    Guid? TenantId { get; }

    Guid? UserId { get; }

    bool IsSuperAdmin { get; }
}

namespace SleveenAdmin.Foundation.Auth;

public interface IAuthUserContext
{
    Guid UserId { get; }
    string Account { get; }
    string? RealName { get; }
    Guid? OrgId { get; }
    Guid? TenantId { get; }
    bool IsSuperAdmin { get; }
}

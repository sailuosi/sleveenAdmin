using Microsoft.Extensions.DependencyInjection;

namespace SleveenAdmin.Business.Portal;

public static class PortalSetup
{
    public static IServiceCollection AddSleveenAdminPortal(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }
}

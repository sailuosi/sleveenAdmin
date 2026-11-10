using Microsoft.Extensions.DependencyInjection;
using SleveenAdmin.Business;

namespace SleveenAdmin.Business.Portal;

public static class PortalSetup
{
    public static IServiceCollection AddSleveenAdminPortal(this IServiceCollection services)
    {
        services.AddMemoryCache();
        // Business 程序集源生成 DI（含 Portal + Ai 等）
        services.AddSleveenAdminBusinessServices();
        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SleveenAdmin.System;

public static class SystemSetupExtensions
{
    public static IServiceCollection AddSleveenAdminSystem(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        // FileStorage / Redis / Captcha 由宿主 AddMooProjectOptions 集中注册
        return services;
    }
}

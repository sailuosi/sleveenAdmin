using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SleveenAdmin.Foundation.Data;
using SleveenAdmin.Foundation.Options;

namespace SleveenAdmin.Foundation.Configuration;

/// <summary>
/// 集中注册宿主常用强类型 Options（节名约定：去掉 Options 后缀）。
/// </summary>
public static class MooProjectOptionsExtensions
{
    public static IServiceCollection AddMooProjectOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<DBCoonfigOptions>(configuration.GetSection(DBCoonfigOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<CaptchaOptions>(configuration.GetSection(CaptchaOptions.SectionName));
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));
        return services;
    }
}

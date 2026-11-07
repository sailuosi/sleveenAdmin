using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace SleveenAdmin.Foundation.DI;

public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// 自动扫描并注册生命周期标记类（ITransient / IScoped / ISingleton）。
    /// </summary>
    /// <param name="services">DI 容器</param>
    /// <param name="assemblyAnchorTypes">用于定位程序集的类型锚点</param>
    public static IServiceCollection AddSleveenAdminServiceDiscovery(
        this IServiceCollection services,
        params Type[] assemblyAnchorTypes)
    {
        if (assemblyAnchorTypes.Length == 0)
            throw new ArgumentException("至少需要一个程序集锚点类型。", nameof(assemblyAnchorTypes));

        RegisterMarked<ISingleton>(services, assemblyAnchorTypes, ServiceLifetime.Singleton);
        RegisterMarked<IScoped>(services, assemblyAnchorTypes, ServiceLifetime.Scoped);
        RegisterMarked<ITransient>(services, assemblyAnchorTypes, ServiceLifetime.Transient);

        return services;
    }

    private static void RegisterMarked<TMarker>(
        IServiceCollection services,
        Type[] assemblyAnchorTypes,
        ServiceLifetime lifetime)
    {
        services.Scan(scan => scan
            .FromAssembliesOf(assemblyAnchorTypes)
            .AddClasses(classes => classes.AssignableTo<TMarker>()
                .Where(t => t is { IsClass: true, IsAbstract: false }))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsSelf()
            .AsImplementedInterfaces(IsInfrastructureInterface)
            .WithLifetime(lifetime));
    }

    /// <summary>
    /// 注册基础设施抽象（如 IDbInitializer、IIdGenerator），排除 Java 式 IUserService 业务接口对。
    /// </summary>
    private static bool IsInfrastructureInterface(Type interfaceType)
    {
        if (!interfaceType.IsInterface)
            return false;

        if (interfaceType == typeof(ISingleton)
            || interfaceType == typeof(IScoped)
            || interfaceType == typeof(ITransient))
            return false;

        if (!interfaceType.Name.StartsWith('I'))
            return false;

        // IUserService / IRoleService 等业务接口不自动注册
        return !interfaceType.Name.EndsWith("Service", StringComparison.Ordinal);
    }
}

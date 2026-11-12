using Microsoft.Extensions.Configuration;
using sailflow.engine.Client;
using sailflow.engine.Hosting;

namespace SleveenAdmin.Sailflow.Hosting;

/// <summary>从配置绑定 <see cref="SailDataLifecycleOptions"/> 并挂到 Builder。</summary>
public static class SailDataLifecycleHostExtensions
{
    public const string SectionName = "Sail:DataLifecycle";

    public static SailClientBuilder UseDataLifecycleFromConfig(
        this SailClientBuilder builder,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(SectionName);
        if (!section.Exists())
            return builder;

        return builder.UseDataLifecycle(o => section.Bind(o));
    }
}

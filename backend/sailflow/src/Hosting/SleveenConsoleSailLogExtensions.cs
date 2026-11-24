using sailflow.engine.Client;
using sailflow.hosting;
using sailflow.log;
using sailflow.msg.Logging;

namespace SleveenAdmin.Sailflow.Hosting;

/// <summary>启用 <see cref="SleveenDevConsoleSailLog"/>（DevConsole 带标题块）。</summary>
public static class SleveenConsoleSailLogExtensions
{
    public const string FeatureName = "Sleveen.Sailflow.ConsoleSailLog";

    public static SailClientBuilder UseSleveenConsoleSailLog(
        this SailClientBuilder builder,
        Action<SailLogOptions>? configureLog = null,
        Action<ConsoleSailLogOptions>? configureConsole = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.UseSleveenConsoleSailLog(configureLog, configureConsole);
        return builder;
    }

    public static ISailCollection UseSleveenConsoleSailLog(
        this ISailCollection services,
        Action<SailLogOptions>? configureLog = null,
        Action<ConsoleSailLogOptions>? configureConsole = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var consoleOptions = new ConsoleSailLogOptions();
        configureConsole?.Invoke(consoleOptions);

        return services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.ConfigureClient,
            Order = 16, // 略晚于 msg.UseConsoleSailLog(15)，同开时覆盖为 Sleveen 版
            ConfigureClient = (client, _) =>
            {
                configureLog?.Invoke(client.LogOptions ??= new SailLogOptions());
                client.useLogger(new SleveenDevConsoleSailLog(
                    () => client.LogOptions ?? new SailLogOptions(),
                    consoleOptions));
            }
        });
    }

    public static SailClient useSleveenConsoleSailLog(
        this SailClient client,
        Action<SailLogOptions>? configureLog = null,
        Action<ConsoleSailLogOptions>? configureConsole = null)
    {
        ArgumentNullException.ThrowIfNull(client);
        configureLog?.Invoke(client.LogOptions ??= new SailLogOptions());
        var consoleOptions = new ConsoleSailLogOptions();
        configureConsole?.Invoke(consoleOptions);
        client.useLogger(new SleveenDevConsoleSailLog(
            () => client.LogOptions ?? new SailLogOptions(),
            consoleOptions));
        return client;
    }
}

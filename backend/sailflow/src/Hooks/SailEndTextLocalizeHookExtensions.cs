using sailflow.engine.Client;
using sailflow.engine.Event;
using sailflow.engine.Hosting;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.Hooks;

public static class SailEndTextLocalizeHookExtensions
{
    static readonly EventFireTypeEnum[] Events =
    [
        EventFireTypeEnum.OnTaskCompleted
    ];

    static void Configure(HookRegistrationOptions o)
    {
        o.Match = HookMatch.All;
        o.Mode = HookMode.Sync;
        o.Events = Events;
    }

    public static SailClientBuilder UseEndTextLocalize(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = "host.endtext.localize",
            Phase = SailFeaturePhase.ConfigureClient,
            Order = 520,
            ConfigureClient = (c, _) => c.addHook(new SailEndTextLocalizeHook(c), Configure),
        });
        return builder;
    }

    public static ISailCollection UseEndTextLocalize(this ISailCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddFeature(new SailFeatureDescriptor
        {
            Name = "host.endtext.localize",
            Phase = SailFeaturePhase.ConfigureClient,
            Order = 520,
            ConfigureClient = (c, _) => c.addHook(new SailEndTextLocalizeHook(c), Configure),
        });
        return services;
    }

    public static SailClient useEndTextLocalize(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.addHook(new SailEndTextLocalizeHook(client), Configure);
        return client;
    }
}

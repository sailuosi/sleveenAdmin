using Microsoft.Extensions.DependencyInjection;
using saiflow.engine.Client;
using saiflow.engine.Hosting;
using saiflow.engine.Event;
using saiflow.engine.Hooks;

namespace SleveenAdmin.Saiflow.Timeout;

public static class SailNodePolicyTimeoutHostingExtensions
{
    const string FeatureName = "sleveen.nodepolicy.timeout.stamp";

    /// <summary>DI：超时/策略跳过后台作业。</summary>
    public static IServiceCollection AddSailNodePolicyJobs(this IServiceCollection services)
    {
        services.AddHostedService<SailNodePolicyJobWorker>();
        return services;
    }

    /// <summary>可办时刻按 NodePolicy 写入待办 Activated/Overdue（Created / Ready / TaskCreated）。</summary>
    public static SailClientBuilder UseNodePolicyTimeoutStamp(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.PostBuild,
            Order = 195,
            PostBuild = (client, _) =>
            {
                client.useFlowHook(new SailNodePolicyTimeoutStampHook(client), o =>
                {
                    o.Match = HookMatch.All;
                    o.Mode = HookMode.Sync;
                    o.Events =
                    [
                        EventFireTypeEnum.OnActivityCreated,
                        EventFireTypeEnum.OnActivityReady,
                        EventFireTypeEnum.OnTaskCreated
                    ];
                });
            }
        });
        return builder;
    }
}

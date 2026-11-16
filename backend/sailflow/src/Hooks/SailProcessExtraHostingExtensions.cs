using Microsoft.Extensions.DependencyInjection;
using sailflow.engine.Client;
using sailflow.engine.Event;
using sailflow.engine.Hosting;
using sailflow.engine.Hooks;
using sailflow.msg.SideEffect;

namespace SleveenAdmin.Sailflow.Hooks;

public static class SailProcessExtraHostingExtensions
{
    const string FeatureCreate = "sleveen.process.extra.create";
    const string FeatureStarted = "sleveen.process.extra.started";
    const string FeatureDedup = "sleveen.process.extra.dedup";
    const string FeatureHttp = "sleveen.process.extra.http";

    /// <summary>Title / processIdRule（创建 Hook）+ 摘要变量 + 去重 AutoApprove + HTTP BestEffort。</summary>
    public static SailClientBuilder UseProcessExtraRuntime(
        this SailClientBuilder builder,
        IServiceProvider? services = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureCreate,
            Phase = SailFeaturePhase.PostBuild,
            Order = 190,
            PostBuild = (client, _) =>
            {
                client.addProcessInstanceCreateHook(new SailProcessExtraCreateHook(client));
            }
        });

        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureStarted,
            Phase = SailFeaturePhase.PostBuild,
            Order = 191,
            PostBuild = (client, _) =>
            {
                client.addHook(new SailProcessExtraStartedHook(client), o =>
                {
                    o.Match = HookMatch.All;
                    o.Mode = HookMode.Sync;
                    o.Events = [EventFireTypeEnum.OnProcessStarted];
                });
            }
        });

        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureDedup,
            Phase = SailFeaturePhase.PostBuild,
            Order = 197,
            PostBuild = (client, _) =>
            {
                client.addHook(new SailProcessExtraDedupHook(client), o =>
                {
                    o.Match = HookMatch.All;
                    o.Mode = HookMode.Sync;
                    o.Events =
                    [
                        EventFireTypeEnum.OnProcessStarted,
                        EventFireTypeEnum.OnProcessContinued,
                        EventFireTypeEnum.OnProcessResent
                    ];
                });
            }
        });

        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureHttp,
            Phase = SailFeaturePhase.PostBuild,
            Order = 198,
            PostBuild = (client, _) =>
            {
                IHttpClientFactory? factory = null;
                try
                {
                    factory = services?.GetService<IHttpClientFactory>();
                }
                catch
                {
                    /* optional */
                }

                client.addHook(
                    new SailProcessExtraHttpTriggerHook(client, factory),
                    BestEffortHookRegistration.Configure(
                        HookMatch.All,
                        order: 198,
                        EventFireTypeEnum.OnProcessStarted,
                        EventFireTypeEnum.OnProcessCompleted,
                        EventFireTypeEnum.OnProcessClosed,
                        EventFireTypeEnum.OnTaskCreated,
                        EventFireTypeEnum.OnTaskCompleted));
            }
        });

        return builder;
    }
}

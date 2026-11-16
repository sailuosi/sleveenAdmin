using Microsoft.Extensions.DependencyInjection;
using sailflow.engine.Client;
using sailflow.engine.Event;
using sailflow.engine.Hosting;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.Assignee;

public static class SailNodePolicyAutoApproveHookExtensions
{
    const string FeatureName = "sleveen.nodepolicy.autoapprove.hook";

    /// <summary>
    /// 发送成功（Started/Continued/Resent）后按 NodePolicy 入队 AutoApprove；不订阅 SendBack 等。
    /// </summary>
    public static SailClientBuilder UseNodePolicyAutoApproveHook(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.PostBuild,
            Order = 196,
            PostBuild = (client, _) =>
            {
                client.addHook(new SailNodePolicyAutoApproveHook(client), o =>
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
        return builder;
    }
}

using saiflow.engine.Client;
using saiflow.engine.Hosting;
using saiflow.engine.Event;
using saiflow.engine.Hooks;
using SleveenAdmin.Saiflow.Service;

namespace SleveenAdmin.Saiflow.Hooks;

/// <summary>请假样例 Status Hook：建表 + 完成/驳回/关闭回写。</summary>
public static class SailLeaveStatusHookExtensions
{
    const string FeatureName = "sleveen.demo.leave.status";

    /// <summary>
    /// 构建期注册：PostBuild 时 EnsureTable 并挂 <see cref="SailLeaveStatusHook"/>（Sync）。
    /// </summary>
    public static SailClientBuilder UseLeaveStatusHook(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.PostBuild,
            Order = 190,
            PostBuild = (client, _) => Register(client)
        });

        return builder;
    }

    /// <summary>已构建 Client 上补挂。</summary>
    public static SailClient useLeaveStatusHook(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        Register(client);
        return client;
    }

    static void Register(SailClient client)
    {
        var leave = new SailLeaveService(client);
        try { leave.EnsureTable(); } catch { /* Probe/init-db 可再补 */ }

        client.useFlowHook(new SailLeaveStatusHook(leave), o =>
        {
            o.Match = HookMatch.All;
            o.Mode = HookMode.Sync;
            o.Events =
            [
                EventFireTypeEnum.OnProcessCompleted,
                EventFireTypeEnum.OnProcessRejected,
                EventFireTypeEnum.OnProcessClosed
            ];
        });
    }
}

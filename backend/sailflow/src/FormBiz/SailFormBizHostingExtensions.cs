using Microsoft.Extensions.DependencyInjection;
using sailflow.engine.Client;
using sailflow.engine.Hosting;
using sailflow.engine.Event;
using sailflow.engine.Hooks;
using SleveenAdmin.Sailflow.FormBiz.Handlers;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.FormBiz;

public static class SailFormBizHostingExtensions
{
    const string FeatureName = "sleveen.formbiz.lifecycle";

    /// <summary>DI：Registry + Leave/Dynamic Handler + Host 模板。</summary>
    public static IServiceCollection AddSailFormBiz(this IServiceCollection services)
    {
        services.AddScoped<ISailFormBizHandler, LeaveFormBizHandler>();
        services.AddScoped<ISailFormBizHandler, DynamicFormBizHandler>();
        services.AddScoped<SailFormBizRegistry>();
        services.AddScoped<SailFormBizHost>();
        return services;
    }

    /// <summary>
    /// 构建期挂生命周期 Hook（完成/驳回/关闭 → OnFinish）。
    /// 替代专用 <c>UseLeaveStatusHook</c>：请假走 LeaveFormBizHandler。
    /// </summary>
    public static SailClientBuilder UseFormBizLifecycleHook(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.PostBuild,
            Order = 190,
            PostBuild = (client, _) => RegisterHook(client)
        });
        return builder;
    }

    static void RegisterHook(SailClient client)
    {
        var leaveSvc = new SailLeaveService(client);
        try { leaveSvc.EnsureTable(); } catch { /* Probe 可再补 */ }

        ISailFormBizHandler[] handlers =
        [
            new LeaveFormBizHandler(leaveSvc),
            new DynamicFormBizHandler()
        ];

        client.addHook(new SailFormBizLifecycleHook(handlers), o =>
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

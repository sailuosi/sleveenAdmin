using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sailflow.engine.Client;
using sailflow.engine.Hosting;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Saiflow.Integration.UnifiedReport;

/// <summary>
/// Reliable 待办推送能力包：按 <see cref="UnifiedReportOptions.Enabled"/> 启用 Outbox Drain + TaskNotifier。
/// </summary>
public static class PortalScheduleTaskNotifierExtensions
{
    const string FeatureName = "sleveen.portal.schedule.notifier";

    /// <summary>
    /// 构建期注册 Reliable 档。未启用时 no-op；启用时挂 Outbox Drain，PostBuild 建表 + <c>useTaskNotifier</c>。
    /// </summary>
    public static SailClientBuilder UsePortalScheduleTaskNotifier(
        this SailClientBuilder builder,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        var opts = services.GetRequiredService<IOptions<UnifiedReportOptions>>().Value;
        if (!opts.Enabled)
            return builder;

        builder.UseHookOutboxDrain(autoStart: true);

        builder.Services.AddFeature(new SailFeatureDescriptor
        {
            Name = FeatureName,
            Phase = SailFeaturePhase.PostBuild,
            Order = 200,
            PostBuild = (client, _) => RegisterNotifier(client, services)
        });

        return builder;
    }

    /// <summary>已构建 Client 上补挂（同样读 Enabled）。</summary>
    public static SailClient usePortalScheduleTaskNotifier(this SailClient client, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(services);

        var opts = services.GetRequiredService<IOptions<UnifiedReportOptions>>().Value;
        if (!opts.Enabled)
            return client;

        RegisterNotifier(client, services);
        return client;
    }

    static void RegisterNotifier(SailClient client, IServiceProvider services)
    {
        var records = new SailTodoPortalRecordService(client);
        try { records.EnsureTable(); } catch { /* Probe/init-db 可再补 */ }

        var notifier = new PortalScheduleTaskNotifier(
            client,
            services.GetRequiredService<UnifiedReportClient>(),
            services.GetRequiredService<IOptions<UnifiedReportOptions>>(),
            records,
            services.GetRequiredService<ILogger<PortalScheduleTaskNotifier>>());

        client.useTaskNotifier(notifier, o =>
        {
            o.Match = HookMatch.All;
            o.Mode = HookMode.AsyncOutbox;
            o.TaskKinds =
            [
                TaskNotifyKind.Created,
                TaskNotifyKind.Completed,
                TaskNotifyKind.Cancelled,
            ];
            // Drain 认领后进程内限流：有界 Channel 背压 + 双路并发 + 最小间隔削峰
            o.InProcessThrottle = new NotifyThrottleOptions
            {
                Capacity = 2000,
                MaxDegree = 2,
                MinInterval = TimeSpan.FromMilliseconds(50),
            };
        });
    }
}

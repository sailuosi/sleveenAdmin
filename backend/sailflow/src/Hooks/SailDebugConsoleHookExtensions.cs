using sailflow.engine.Client;
using sailflow.engine.Event;
using sailflow.engine.Hosting;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 开发用控制台调试：Hook + Notifier（Foundation TP 风格）。
/// </summary>
public static class SailDebugConsoleHookExtensions
{
    static readonly EventFireTypeEnum[] DebugHookEvents =
    [
        EventFireTypeEnum.OnProcessStarted,
        EventFireTypeEnum.OnProcessContinued,
        EventFireTypeEnum.OnProcessCompleted,
        EventFireTypeEnum.OnProcessRejected,
        EventFireTypeEnum.OnProcessSendBacked,
        EventFireTypeEnum.OnProcessWithdrawn,
        EventFireTypeEnum.OnProcessResent,
        EventFireTypeEnum.OnProcessRevised,
        EventFireTypeEnum.OnProcessReversed,
        EventFireTypeEnum.OnProcessClosed,
        EventFireTypeEnum.OnProcessJumped,
        EventFireTypeEnum.OnProcessSignForwarded,
        EventFireTypeEnum.OnTaskCreated,
        EventFireTypeEnum.OnTaskCompleted
    ];

    static void ConfigureDebugHook(HookRegistrationOptions o)
    {
        o.Match = HookMatch.All;
        o.Mode = HookMode.Sync;
        o.Events = DebugHookEvents;
    }

    static void ConfigureDebugNotifier(HookRegistrationOptions o)
    {
        o.Match = HookMatch.All;
        o.Mode = HookMode.AsyncOutbox;
        o.TaskKinds =
        [
            TaskNotifyKind.Created,
            TaskNotifyKind.Completed,
            TaskNotifyKind.Cancelled,
            TaskNotifyKind.Entrusted
        ];
    }

    /// <summary>
    /// 构建期注册 Debug Hook + Debug Notifier。
    /// Notifier 依赖 Outbox Drain（本扩展内启用，可与 Portal/BizMirror 共用）。
    /// </summary>
    public static SailClientBuilder UseDebugConsoleHook(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseHookOutboxDrain(autoStart: true);
        builder.AddHook(new SailDebugConsoleHook(), ConfigureDebugHook);
        builder.AddNotifier(new SailDebugConsoleNotifier(), ConfigureDebugNotifier);
        return builder;
    }

    /// <summary>Collection 管道注册（可放在 <c>ConfigureServices</c>）。</summary>
    public static ISailCollection UseDebugConsoleHook(this ISailCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHook(new SailDebugConsoleHook(), ConfigureDebugHook);
        services.AddNotifier(new SailDebugConsoleNotifier(), ConfigureDebugNotifier);
        return services;
    }

    /// <summary>已构建 Client 上补挂。</summary>
    public static SailClient useDebugConsoleHook(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.addHook(new SailDebugConsoleHook(), ConfigureDebugHook);
        client.addNotifier(new SailDebugConsoleNotifier(), ConfigureDebugNotifier);
        return client;
    }
}

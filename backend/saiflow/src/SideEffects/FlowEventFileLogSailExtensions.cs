using saiflow.engine.Client;
using saiflow.engine.Event;
using saiflow.engine.Hosting;
using saiflow.engine.Hooks;
using saiflow.msg.SideEffect;

namespace SleveenAdmin.Saiflow.SideEffects;

/// <summary>BestEffort 流程事件文件日志：能力包 UseXxx。</summary>
public static class FlowEventFileLogSailExtensions
{
    static readonly EventFireTypeEnum[] DefaultEvents =
    [
        EventFireTypeEnum.OnProcessStarted,
        EventFireTypeEnum.OnProcessContinued,
        EventFireTypeEnum.OnProcessCompleted,
        EventFireTypeEnum.OnProcessClosed,
        EventFireTypeEnum.OnTaskCreated,
        EventFireTypeEnum.OnTaskCompleted
    ];

    /// <summary>构建期注册（Sync + Channel 串行写文件）。</summary>
    public static SailClientBuilder UseFlowEventFileLog(
        this SailClientBuilder builder,
        Action<FlowEventFileLogOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var opts = new FlowEventFileLogOptions();
        configure?.Invoke(opts);
        if (!opts.Enabled)
            return builder;

        return builder.UseFlowHook(
            new FlowEventFileLogHook(opts.Directory, opts.FilePrefix, opts.Channel),
            BestEffortHookRegistration.Configure(HookMatch.All, opts.Order, opts.Events ?? DefaultEvents));
    }

    public static ISailCollection UseFlowEventFileLog(
        this ISailCollection services,
        Action<FlowEventFileLogOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        var opts = new FlowEventFileLogOptions();
        configure?.Invoke(opts);
        if (!opts.Enabled)
            return services;

        return services.UseFlowHook(
            new FlowEventFileLogHook(opts.Directory, opts.FilePrefix, opts.Channel),
            BestEffortHookRegistration.Configure(HookMatch.All, opts.Order, opts.Events ?? DefaultEvents));
    }
}

/// <summary>流程事件文件日志选项。</summary>
public sealed class FlowEventFileLogOptions
{
    public bool Enabled { get; set; } = true;
    public string Directory { get; set; }
    public string FilePrefix { get; set; } = "flow-events";
    public int Order { get; set; } = 900;
    public EventFireTypeEnum[] Events { get; set; }
    public BestEffortChannelOptions Channel { get; set; } = new()
    {
        Capacity = 8192,
        DropWhenFull = true
    };
}

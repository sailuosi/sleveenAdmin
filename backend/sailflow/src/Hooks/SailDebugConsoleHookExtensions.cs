using sailflow.engine.Client;
using sailflow.engine.Hosting;
using sailflow.engine.Event;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>开发用控制台调试 Hook（Builder / Collection / Client）。</summary>
public static class SailDebugConsoleHookExtensions
{
    static void ConfigureDebugOptions(HookRegistrationOptions o)
    {
        o.Match = HookMatch.All;
        o.Mode = HookMode.Sync;
        o.Events =
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
    }

    /// <summary>
    /// 构建期注册 <see cref="SailDebugConsoleHook"/>（pending → <c>Build</c> 时 Apply）。
    /// </summary>
    public static SailClientBuilder UseDebugConsoleHook(this SailClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.UseFlowHook(new SailDebugConsoleHook(), ConfigureDebugOptions);
    }

    /// <summary>Collection 管道注册（可放在 <c>ConfigureServices</c>）。</summary>
    public static ISailCollection UseDebugConsoleHook(this ISailCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.UseFlowHook(new SailDebugConsoleHook(), ConfigureDebugOptions);
    }

    /// <summary>已构建 Client 上补挂（与引擎 <c>useFlowHook</c> 对齐）。</summary>
    public static SailClient useDebugConsoleHook(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return client.useFlowHook(new SailDebugConsoleHook(), ConfigureDebugOptions);
    }
}

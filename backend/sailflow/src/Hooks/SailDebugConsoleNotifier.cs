using sailflow.engine.Hooks;
using sailflow.msg.Notify;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 开发核验：待办 Notifier 通道用 TP 风格控制台输出。
/// 走 AsyncOutbox（引擎强制）；Drain 消费时打印。勿用于生产噪声场景。
/// </summary>
public sealed class SailDebugConsoleNotifier : TaskNotifyChannelBase
{
    public override string Name => "sail.debug.console.notifier";

    /// <summary>打印失败不抛，避免拖死 Outbox。</summary>
    protected override bool RethrowFailures => false;

    protected override Task SendCoreAsync(TaskNotifyContext ctx, CancellationToken ct)
    {
        SailDevConsole.WriteNotify("调试", ctx);
        return Task.CompletedTask;
    }
}

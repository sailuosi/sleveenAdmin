using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.Hooks;

/// <summary>
/// 开发核验：流程 Hook 回调时用 TP 风格控制台输出。
/// 不序列化 <see cref="FlowHookContext.Kit"/>。仅 Sync；勿用于生产噪声场景。
/// </summary>
public sealed class SailDebugConsoleHook : IFlowHook
{
    public string Name => "sail.debug.console.hook";

    public void Handle(FlowHookContext ctx) =>
        SailDevConsole.WriteHook("调试", ctx);
}

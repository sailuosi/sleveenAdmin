using sailflow.engine.Hooks;
using SleveenAdmin.Sailflow.Hooks;

namespace SleveenAdmin.Sailflow.BizMirror;

/// <summary>
/// stub: console-only; real port later。
/// 伪实现：命中 Continued/终态时仅打控制台，不建表、不投影。
/// </summary>
public sealed class SailBizMirrorStubHook : IFlowHook
{
    public string Name => "host.sleveen.biz.mirror.stub";

    public void Handle(FlowHookContext ctx) =>
        SailDevConsole.WriteHook("业务回写", ctx);
}

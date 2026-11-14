using sailflow.engine.Event;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>§H：流程终态 → 按 BizChannel(=FormCode) 调 Handler.OnFinish。</summary>
public sealed class SailFormBizLifecycleHook : IFlowHook
{
    readonly IReadOnlyDictionary<string, ISailFormBizHandler> _map;

    public SailFormBizLifecycleHook(IEnumerable<ISailFormBizHandler> handlers)
    {
        _map = handlers
            .Where(h => h != null && !string.IsNullOrWhiteSpace(h.FormCode))
            .ToDictionary(h => h.FormCode.Trim(), h => h, StringComparer.OrdinalIgnoreCase);
    }

    public string Name => "sail.formbiz.lifecycle";

    public void Handle(FlowHookContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.BizChannel)) return;
        if (!_map.TryGetValue(ctx.BizChannel, out var handler)) return;
        if (string.IsNullOrWhiteSpace(ctx.BizPK)) return;

        handler.OnFinish(new SailFormBizContext
        {
            FormCode = handler.FormCode,
            BizChannel = ctx.BizChannel,
            BizPK = ctx.BizPK,
            ProcessInstanceId = string.IsNullOrWhiteSpace(ctx.ProcessInstanceId)
                ? ""
                : ctx.ProcessInstanceId.Trim(),
            FinishEvent = ctx.EventType
        });
    }
}
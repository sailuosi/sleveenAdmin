using sailflow.engine.Event;
using sailflow.engine.Hooks;

namespace SleveenAdmin.Sailflow.FormBiz;

/// <summary>§H：流程终态 → 按 AppName(=FormCode) 调 Handler.OnFinish。</summary>
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
        if (string.IsNullOrWhiteSpace(ctx.AppName)) return;
        if (!_map.TryGetValue(ctx.AppName, out var handler)) return;
        if (string.IsNullOrWhiteSpace(ctx.AppInstanceId)) return;

        handler.OnFinish(new SailFormBizContext
        {
            FormCode = handler.FormCode,
            AppName = ctx.AppName,
            AppInstanceId = ctx.AppInstanceId,
            ProcessInstanceId = string.IsNullOrWhiteSpace(ctx.ProcessInstanceId)
                ? ""
                : ctx.ProcessInstanceId.Trim(),
            FinishEvent = ctx.EventType
        });
    }
}

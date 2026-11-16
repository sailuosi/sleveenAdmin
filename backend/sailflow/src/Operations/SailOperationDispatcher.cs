using sailflow.engine.Result;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.FormBiz;
using SleveenAdmin.Sailflow.Service;

namespace SleveenAdmin.Sailflow.Operations;

/// <summary>按码表 ButtonId 路由到已注册 Handler；未注册禁止空壳执行。</summary>
public sealed class SailOperationDispatcher
{
    readonly Dictionary<string, ISailOperationHandler> _map;
    readonly SailFormBizHost? _formBiz;
    readonly IWorkflowService _wf;
    readonly SailProcessExtraService _extra;

    public SailOperationDispatcher(
        IEnumerable<ISailOperationHandler> handlers,
        IWorkflowService wf,
        SailProcessExtraService extra,
        SailFormBizHost? formBiz = null)
    {
        _formBiz = formBiz;
        _wf = wf ?? throw new ArgumentNullException(nameof(wf));
        _extra = extra ?? throw new ArgumentNullException(nameof(extra));
        _map = new Dictionary<string, ISailOperationHandler>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in handlers)
        {
            foreach (var id in h.SupportedButtonIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (_map.TryGetValue(id, out var prev))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[SailOperation] ButtonId '{id}' overridden: {prev.GetType().Name} → {h.GetType().Name}");
                }
                _map[id] = h;
            }
        }
    }

    public ISailOperationHandler? Resolve(string buttonId)
    {
        if (string.IsNullOrWhiteSpace(buttonId)) return null;
        return _map.TryGetValue(buttonId.Trim(), out var h) ? h : null;
    }

    public async Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
    {
        var gated = SailExtraOperationGate.TryReject(ctx.ButtonId, ctx.Runner, _wf, _extra);
        if (gated != null)
            return gated;

        var handler = Resolve(ctx.ButtonId);
        if (handler == null)
        {
            var fail = WfExecutedResult.Default();
            fail.Status = WfExecutedStatus.Exception;
            fail.Message = $"No operation handler registered for button '{ctx.ButtonId}'.";
            fail.ReasonCode = "OperationHandlerMissing";
            return fail;
        }
        var result = await handler.ExecuteAsync(ctx, ct);
        if (result.Status == WfExecutedStatus.Success && _formBiz != null)
        {
            try
            {
                _formBiz.NotifyAfterOperation(
                    ctx.Runner?.BizChannel,
                    ctx.ButtonId,
                    new SailFormBizContext
                    {
                        FormCode = ctx.Runner?.BizChannel ?? "",
                        BizChannel = ctx.Runner?.BizChannel ?? "",
                        BizPK = ctx.Runner?.BizPK ?? "",
                        UserId = ctx.Runner?.UserId ?? "",
                        UserName = ctx.Runner?.UserName ?? "",
                        ButtonId = ctx.ButtonId
                    });
            }
            catch
            {
                /* 业务回调失败不回滚引擎结果；可观测即可 */
            }
        }
        return result;
    }

    /// <summary>码表 Id × 是否已挂 Handler（探针/自检）。</summary>
    public IReadOnlyDictionary<string, bool> Probe(IEnumerable<string> buttonIds)
    {
        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in buttonIds)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            map[id] = Resolve(id) != null;
        }
        return map;
    }
}

using sailflow.engine.Result;
using SleveenAdmin.Saiflow.FormBiz;

namespace SleveenAdmin.Saiflow.Operations;

/// <summary>按码表 ButtonId 路由到已注册 Handler；未注册禁止空壳执行。</summary>
public sealed class SailOperationDispatcher
{
    readonly Dictionary<string, ISailOperationHandler> _map;
    readonly SailFormBizHost? _formBiz;

    public SailOperationDispatcher(
        IEnumerable<ISailOperationHandler> handlers,
        SailFormBizHost? formBiz = null)
    {
        _formBiz = formBiz;
        _map = new Dictionary<string, ISailOperationHandler>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in handlers)
        {
            foreach (var id in h.SupportedButtonIds)
            {
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (_map.TryGetValue(id, out var prev))
                {
                    // 后注册覆盖，便于宿主定制；诊断用
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
        // §H：办理成功后按 AppName 回调业务 Handler（同意/退回/取消）
        if (result.Status == WfExecutedStatus.Success && _formBiz != null)
        {
            try
            {
                _formBiz.NotifyAfterOperation(
                    ctx.Runner?.AppName,
                    ctx.ButtonId,
                    new SailFormBizContext
                    {
                        FormCode = ctx.Runner?.AppName ?? "",
                        AppName = ctx.Runner?.AppName ?? "",
                        AppInstanceId = ctx.Runner?.AppInstanceId ?? "",
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
        var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in buttonIds)
        {
            if (string.IsNullOrWhiteSpace(id)) continue;
            result[id] = _map.ContainsKey(id);
        }
        return result;
    }
}

using sailflow.engine.Common;
using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Sailflow.FormBiz;

public sealed class SubmitBizFormRequest
{
    public string FormCode { get; set; } = "";
    public string? ProcessId { get; set; }
    public string? Version { get; set; }
    public string? PayloadJson { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
}

/// <summary>§H Host 模板：Submit → OnSubmit → StartProcess → OnStarted。</summary>
public sealed class SailFormBizHost(
    SailFormBizRegistry registry,
    IWorkflowService wf)
{
    public SailFormBizRegistry Registry => registry;

    public async Task<(SailFormBizContext Ctx, WfExecutedResult? Exec)> SubmitAndStartAsync(
        SubmitBizFormRequest req,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.FormCode))
            throw new ArgumentException("FormCode required", nameof(req));
        if (string.IsNullOrWhiteSpace(req.ProcessId))
            throw new ArgumentException("ProcessId required", nameof(req));

        var handler = registry.GetRequired(req.FormCode);
        var ctx = new SailFormBizContext
        {
            FormCode = req.FormCode.Trim(),
            UserId = req.UserId ?? "",
            UserName = req.UserName ?? "",
            ProcessId = req.ProcessId!,
            Version = string.IsNullOrWhiteSpace(req.Version) ? "1" : req.Version!,
            BizChannel = req.FormCode.Trim(),
            PayloadJson = req.PayloadJson
        };

        handler.OnSubmit(ctx);
        if (!string.IsNullOrWhiteSpace(ctx.Error))
            return (ctx, null);

        EnsureDefaultFirstHopPerformers(ctx);

        var exec = await wf.StartProcessAsync(ctx.ToRunner(), ct).ConfigureAwait(false);
        if (exec.Status == WfExecutedStatus.Success
            && !string.IsNullOrWhiteSpace(exec.ProcessInstanceIdStarted))
        {
            ctx.ProcessInstanceId = exec.ProcessInstanceIdStarted.Trim();
            handler.OnStarted(ctx);
        }
        return (ctx, exec);
    }

    /// <summary>
    /// ApproverPick 等首跳需 NextActivityPerformers；未指定时默认当前用户到首个人工节点。
    /// </summary>
    void EnsureDefaultFirstHopPerformers(SailFormBizContext ctx)
    {
        if (ctx.NextActivityPerformers is { Count: > 0 })
            return;
        if (string.IsNullOrWhiteSpace(ctx.UserId) || string.IsNullOrWhiteSpace(ctx.ProcessId))
            return;

        try
        {
            var ver = string.IsNullOrWhiteSpace(ctx.Version) ? "1" : ctx.Version;
            var tasks = wf.GetTaskActivityList(ctx.ProcessId, ver);
            var first = tasks?.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a?.ActivityId));
            if (first == null)
            {
                var act = wf.GetFirstActivity(ctx.ProcessId, ver);
                if (act == null || string.IsNullOrWhiteSpace(act.ActivityId))
                    return;
                first = act;
            }

            var list = new PerformerList
            {
                new Performer(ctx.UserId.Trim(), string.IsNullOrWhiteSpace(ctx.UserName) ? ctx.UserId : ctx.UserName)
            };
            ctx.NextActivityPerformers = new Dictionary<string, PerformerList>
            {
                [first.ActivityId!] = list
            };
        }
        catch
        {
            /* 定义缺失时交给引擎报错，不在此吞业务异常语义 */
        }
    }

    public void NotifyAfterOperation(string? BizChannel, string buttonId, SailFormBizContext? seed = null)
    {
        var handler = registry.TryGet(BizChannel);
        if (handler == null) return;

        var ctx = seed ?? new SailFormBizContext
        {
            FormCode = handler.FormCode,
            BizChannel = BizChannel ?? handler.FormCode,
            ButtonId = buttonId
        };
        ctx.ButtonId = buttonId;

        switch (buttonId)
        {
            case "Approve":
                handler.OnConsent(ctx);
                break;
            case "SendBack":
                handler.OnBackToModify(ctx);
                break;
            case "Close":
            case "Terminate":
            case "Discard":
            case "Reject":
                handler.OnCancel(ctx);
                break;
        }
    }
}

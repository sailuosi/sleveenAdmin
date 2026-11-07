using saiflow.engine.Result;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.FormBiz;

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
            AppName = req.FormCode.Trim(),
            PayloadJson = req.PayloadJson
        };

        handler.OnSubmit(ctx);
        if (!string.IsNullOrWhiteSpace(ctx.Error))
            return (ctx, null);

        var exec = await wf.StartProcessAsync(ctx.ToRunner(), ct).ConfigureAwait(false);
        if (exec.Status == WfExecutedStatus.Success && exec.ProcessInstanceIdStarted > 0)
        {
            ctx.ProcessInstanceId = exec.ProcessInstanceIdStarted.ToString();
            handler.OnStarted(ctx);
        }
        return (ctx, exec);
    }

    public void NotifyAfterOperation(string? appName, string buttonId, SailFormBizContext? seed = null)
    {
        var handler = registry.TryGet(appName);
        if (handler == null) return;

        var ctx = seed ?? new SailFormBizContext
        {
            FormCode = handler.FormCode,
            AppName = appName ?? handler.FormCode,
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

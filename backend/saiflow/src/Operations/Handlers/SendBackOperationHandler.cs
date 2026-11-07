using saiflow.engine.Result;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

/// <summary>§J：退回前强校验目标 ActivityId（Workbench 已按 SendBackTarget 收窄，此处防直调 API）。</summary>
public sealed class SendBackOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["SendBack"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
    {
        var next = ctx.Runner?.NextActivityPerformers;
        if (next == null || next.Count == 0)
            return Task.FromResult(Fail("SendBackTargetMissing", "SendBack requires NextActivityPerformers target."));

        var targetId = next.Keys.FirstOrDefault(k => !string.IsNullOrWhiteSpace(k));
        if (string.IsNullOrWhiteSpace(targetId))
            return Task.FromResult(Fail("SendBackTargetMissing", "SendBack target ActivityId is empty."));

        return wf.SendBackProcessAsync(ctx.Runner, ct);
    }

    static WfExecutedResult Fail(string reason, string message)
    {
        var r = WfExecutedResult.Default();
        r.Status = WfExecutedStatus.Failed;
        r.ReasonCode = reason;
        r.Message = message;
        return r;
    }
}

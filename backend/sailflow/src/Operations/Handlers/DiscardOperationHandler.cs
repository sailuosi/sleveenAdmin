using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class DiscardOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Discard"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
    {
        var ok = wf.DiscardProcess(ctx.Runner);
        var r = WfExecutedResult.Default();
        if (ok)
        {
            r.Status = WfExecutedStatus.Success;
            r.Message = "DiscardProcess ok";
        }
        else
        {
            r.Status = WfExecutedStatus.Failed;
            r.Message = "DiscardProcess failed";
            r.ReasonCode = "DiscardFailed";
        }
        return Task.FromResult(r);
    }
}

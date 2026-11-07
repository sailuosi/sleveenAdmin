using saiflow.engine.Result;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class TerminateOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Terminate"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
    {
        var ok = wf.TerminateProcess(ctx.Runner);
        var r = WfExecutedResult.Default();
        if (ok)
        {
            r.Status = WfExecutedStatus.Success;
            r.Message = "TerminateProcess ok";
        }
        else
        {
            r.Status = WfExecutedStatus.Failed;
            r.Message = "TerminateProcess failed";
            r.ReasonCode = "TerminateFailed";
        }
        return Task.FromResult(r);
    }
}

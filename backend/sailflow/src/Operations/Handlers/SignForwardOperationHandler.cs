using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class SignForwardOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["SignForward"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.SignForwardProcessAsync(ctx.Runner, ct);
}

using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class AcknowledgeOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Acknowledge"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.AcknowledgeProcessAsync(ctx.Runner, ct);
}

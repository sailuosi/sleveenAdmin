using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class JumpOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Jump"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.JumpProcessAsync(ctx.Runner, cancellationToken: ct);
}

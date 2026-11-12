using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class TransferOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Transfer"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.TransferProcessAsync(ctx.Runner, ct);
}

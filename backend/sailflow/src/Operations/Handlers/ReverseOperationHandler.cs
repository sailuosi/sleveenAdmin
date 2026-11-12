using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class ReverseOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Reverse"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.ReverseProcessAsync(ctx.Runner, ct);
}

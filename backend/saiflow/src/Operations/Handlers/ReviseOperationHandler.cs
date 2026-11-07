using saiflow.engine.Result;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class ReviseOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Revise"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.ReviseProcessAsync(ctx.Runner, ct);
}

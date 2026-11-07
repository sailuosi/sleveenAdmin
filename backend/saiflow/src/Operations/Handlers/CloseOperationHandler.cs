using saiflow.engine.Result;
using saiflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class CloseOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Close"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.CloseProcessAsync(ctx.Runner, ct);
}

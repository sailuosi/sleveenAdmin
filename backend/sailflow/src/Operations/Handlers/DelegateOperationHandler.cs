using sailflow.result;
using sailflow.service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class DelegateOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Delegate"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.DelegateProcessAsync(ctx.Runner, ct);
}

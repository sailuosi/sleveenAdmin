using sailflow.result;
using sailflow.service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class ApproveOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Approve"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.RunProcessAsync(ctx.Runner, ct);
}

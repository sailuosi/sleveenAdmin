using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class AddAssigneeOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["AddAssignee"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.AddAssigneeProcessAsync(ctx.Runner, ct);
}

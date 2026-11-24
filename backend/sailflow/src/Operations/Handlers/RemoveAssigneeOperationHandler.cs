using sailflow.result;
using sailflow.service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class RemoveAssigneeOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["RemoveAssignee"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.RemoveAssigneeProcessAsync(ctx.Runner, ct);
}

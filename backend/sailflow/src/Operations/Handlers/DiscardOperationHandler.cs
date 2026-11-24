using sailflow.result;
using sailflow.service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class DiscardOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Discard"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default) =>
        Task.FromResult(wf.DiscardProcess(ctx.Runner));
}

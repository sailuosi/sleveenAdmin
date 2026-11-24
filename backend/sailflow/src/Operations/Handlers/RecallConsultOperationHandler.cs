using sailflow.result;
using sailflow.service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class RecallConsultOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["RecallConsult"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.RecallConsultProcessAsync(ctx.Runner, ct);
}

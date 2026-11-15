using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

public sealed class ConsultOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Consult"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.ConsultProcessAsync(ctx.Runner, ct);
}

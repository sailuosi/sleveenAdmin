using sailflow.engine.Result;
using sailflow.engine.Service;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

public sealed class WithdrawOperationHandler(IWorkflowService wf) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Withdraw"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
        => wf.WithdrawProcessAsync(ctx.Runner, ct);
}

using sailflow.engine.Client;
using sailflow.engine.Result;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Operations.Handlers;

/// <summary>
/// 驳回：读 NodePolicy → 注入 ControlParameterSheet.RejectMode → 只调 RejectProcess。
/// 不再转调 SendBack（Specified 亦由引擎 Reject 回跳）。
/// </summary>
public sealed class RejectOperationHandler(IWorkflowService wf, SailClient sail) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Reject"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
    {
        var policy = SailNodePolicyLoader.LoadForRunner(sail, ctx.Runner);
        SailRejectModeMapper.ApplyToRunner(ctx.Runner, policy);
        return wf.RejectProcessAsync(ctx.Runner, ct);
    }
}

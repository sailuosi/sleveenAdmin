using saiflow.engine.Client;
using saiflow.engine.Common;
using saiflow.engine.Result;
using saiflow.engine.Service;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Operations.Handlers;

/// <summary>
/// 驳回：读 NodePolicy.rejectHandlerType —— 1 结束流程（RejectProcess）；
/// 2 驳回到 returnNodeId（SendBack）。
/// </summary>
public sealed class RejectOperationHandler(IWorkflowService wf, SailClient sail) : ISailOperationHandler
{
    public IReadOnlyList<string> SupportedButtonIds { get; } = ["Reject"];

    public Task<WfExecutedResult> ExecuteAsync(SailOperationContext ctx, CancellationToken ct = default)
    {
        var policy = SailNodePolicyLoader.LoadForRunner(sail, ctx.Runner);
        if (policy.RejectHandlerType == 2 && !string.IsNullOrWhiteSpace(policy.ReturnNodeId))
        {
            var target = policy.ReturnNodeId.Trim();
            var runner = ctx.Runner;
            runner.NextActivityPerformers ??= new Dictionary<string, PerformerList>();
            if (!runner.NextActivityPerformers.ContainsKey(target) ||
                runner.NextActivityPerformers[target] == null ||
                runner.NextActivityPerformers[target].Count == 0)
            {
                var list = new PerformerList();
                if (!string.IsNullOrWhiteSpace(runner.UserId))
                    list.Add(new Performer(runner.UserId, runner.UserName ?? runner.UserId));
                else
                    list.Add(new Performer("system", "system"));
                runner.NextActivityPerformers[target] = list;
            }

            // 清掉无关目标，只留 returnNodeId
            var keep = runner.NextActivityPerformers[target];
            runner.NextActivityPerformers = new Dictionary<string, PerformerList> { [target] = keep };
            return wf.SendBackProcessAsync(runner, ct);
        }

        return wf.RejectProcessAsync(ctx.Runner, ct);
    }
}

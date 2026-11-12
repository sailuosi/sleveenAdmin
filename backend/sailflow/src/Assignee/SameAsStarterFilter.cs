using System.Diagnostics;
using sailflow.assignee;
using sailflow.engine.Assignee;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Runtime;
using sailflow.engine.Service;
using SleveenAdmin.Saiflow.Utility;

namespace SleveenAdmin.Saiflow.Assignee;

/// <summary>
/// assignStartUserHandlerType：2=同人策略跳过；3=按 NodePolicy.assignStartResolveType 走主管道 Resolver 换人。
/// </summary>
public sealed class SameAsStarterFilter : IAssigneeResolveFilter
{
    readonly SailClient _sail;

    public SameAsStarterFilter(SailClient sail)
    {
        _sail = sail ?? throw new ArgumentNullException(nameof(sail));
    }

    public void Apply(AssigneeFilterContext ctx)
    {
        var type = NodePolicyAssigneeSupport.GetInt(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagAssignStart, 1);
        if (type is 1 or 0)
            return;
        if (ctx.Performers == null || ctx.Performers.Count == 0)
            return;
        if (string.IsNullOrWhiteSpace(ctx.Request?.ProcessInstanceId))
            return;

        if (ctx.Performers.Any(p =>
                SailPolicyConstants.IsSkipUser(p.UserId)
                || SailPolicyConstants.IsAutoRejectUser(p.UserId)))
            return;

        try
        {
            var starter = new WorkflowService(_sail).GetProcessInitiator(ctx.Request.ProcessInstanceId);
            if (starter == null || string.IsNullOrWhiteSpace(starter.UserId))
            {
                // type 2/3 须能判定同人；读不到发起人 → fail-closed
                throw new WfAssigneeException(
                    AssigneeReasonCodes.SameAsStarterResolveEmpty,
                    "SameAsStarter=" + type + " cannot load process initiator. ReasonCode="
                    + AssigneeReasonCodes.SameAsStarterResolveEmpty);
            }

            var allSame = ctx.Performers.All(p =>
                string.Equals(p.UserId, starter.UserId, StringComparison.OrdinalIgnoreCase));
            if (!allSame)
                return;

            if (string.Equals(ctx.Source, PerformerSources.Specific, StringComparison.OrdinalIgnoreCase))
                ctx.RewroteSpecific = true;

            if (type == 2)
            {
                NodePolicySkipIntent.Apply(ctx, "SameAsStarterSkip", "同人策略：自动通过");
                Trace.TraceInformation(
                    "[Assignee.Host] SameAsStarter=2 → policy skip + anchor; Activity={0}",
                    ctx.Request.ActivityId);
                return;
            }

            if (type == 3)
                ApplyReResolve(ctx);
        }
        catch (WfAssigneeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // type 2/3：禁止吞异常导致同人策略静默失效
            throw new WfAssigneeException(
                AssigneeReasonCodes.SameAsStarterResolveEmpty,
                "SameAsStarter=" + type + " failed: " + ex.Message
                + ". ReasonCode=" + AssigneeReasonCodes.SameAsStarterResolveEmpty,
                ex);
        }
    }

    void ApplyReResolve(AssigneeFilterContext ctx)
    {
        var resolveType = NodePolicyAssigneeSupport.GetString(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagAssignStartResolveType);
        // 旧图仅有 type=3、未写 Type 时默认 deptBoss（与产品「转部门负责人」同口径）
        if (string.IsNullOrWhiteSpace(resolveType))
        {
            resolveType = DeptBossAssigneeResolver.TypeCode;
            Trace.TraceInformation(
                "[Assignee.Host] SameAsStarter=3 assignStartResolveType empty → default {0}",
                resolveType);
        }

        if (_sail.AssigneePipeline is not AssigneePipeline pipeline)
        {
            throw new WfAssigneeException(
                AssigneeReasonCodes.AssigneeTypeUnregistered,
                "SameAsStarter=3 needs AssigneePipeline to resolve Type '"
                + resolveType
                + "'. ReasonCode="
                + AssigneeReasonCodes.AssigneeTypeUnregistered);
        }

        if (!pipeline.Registry.TryGetResolver(resolveType, out var resolver) || resolver == null)
        {
            throw new WfAssigneeException(
                AssigneeReasonCodes.AssigneeTypeUnregistered,
                "SameAsStarter Resolve Type '"
                + resolveType
                + "' is not registered. ReasonCode="
                + AssigneeReasonCodes.AssigneeTypeUnregistered);
        }

        var paramsJson = NodePolicyAssigneeSupport.GetString(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagAssignStartResolveParams);
        var config = ConfigPairJson.FromTypeAndParamsJson(resolveType, paramsJson)
                     ?? ConfigPair.Create(resolveType);

        pipeline.WireResolver(resolver);
        if (resolver is DeptBossAssigneeResolver deptBoss)
            deptBoss.Sail ??= _sail;

        var list = resolver.Resolve(new AssigneeResolveContext
        {
            Request = ctx.Request,
            Config = config
        }) ?? new PerformerList();

        if (list.Count == 0)
        {
            throw new WfAssigneeException(
                AssigneeReasonCodes.SameAsStarterResolveEmpty,
                "SameAsStarter=3 Resolve Type '"
                + resolveType
                + "' yielded empty performers. ReasonCode="
                + AssigneeReasonCodes.SameAsStarterResolveEmpty);
        }

        ctx.Performers = list;
        ctx.Source = PerformerSources.Resolver;
        Trace.TraceInformation(
            "[Assignee.Host] SameAsStarter=3 → Type={0} performers={1}; Activity={2}",
            resolveType, list.Count, ctx.Request?.ActivityId);
    }
}

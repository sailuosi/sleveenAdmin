using System.Diagnostics;
using sailflow.assignee;
using sailflow.engine.Assignee;
using sailflow.engine.Client;
using sailflow.engine.Common;
using sailflow.engine.Runtime;
using sailflow.engine.Service;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Assignee;

/// <summary>
/// assignStartUserHandlerType：3=按 assignStartResolveType 换人。
/// type=2 自动通过改由 <see cref="SailNodePolicyAutoApproveHook"/> 在发送成功后入队，本 Filter 不再清人/打锚点。
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
        if (ctx.Request != null && !ctx.Request.AllowSameAsStarterPolicy)
            return;

        var type = NodePolicyAssigneeSupport.GetInt(
            ctx.PolicyBag, NodePolicyAssigneeSupport.BagAssignStart, 1);
        // type=2：保留办理人，由发送成功 Hook 入队 AutoApprove
        if (type is 1 or 0 or 2)
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
            // 同事务读未提交实例；禁止无 kit 的 GetProcessInitiator（会另开连接锁死）
            var kit = ctx.Request?.Kit;
            var starter = kit != null
                ? new WorkflowService(_sail).GetProcessInitiator(kit.useSQL(), ctx.Request.ProcessInstanceId)
                : new WorkflowService(_sail).GetProcessInitiator(ctx.Request.ProcessInstanceId);
            if (starter == null || string.IsNullOrWhiteSpace(starter.UserId))
            {
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

            if (type == 3)
                ApplyReResolve(ctx);
        }
        catch (WfAssigneeException)
        {
            throw;
        }
        catch (Exception ex)
        {
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

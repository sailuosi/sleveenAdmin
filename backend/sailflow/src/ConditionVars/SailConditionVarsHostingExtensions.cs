using sailflow.engine.Client;
using sailflow.engine.Hosting;
using sailflow.engine.NextStep;

namespace SleveenAdmin.Sailflow.ConditionVars;

public static class SailConditionVarsHostingExtensions
{
    /// <summary>挂业务 NextStep Prepare（请假 + 联调种子）。新业务：再 <c>IfCondition&lt;YourPrepare&gt;</c>。</summary>
    public static SailClient UseSailBizConditionPrepares(this SailClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        client.UseNextStepRoute(r =>
        {
            r.IfCondition(new LeaveConditionPrepare(), o =>
            {
                o.Match = NextStepMatch.All;
                o.Order = 40;
            });
            r.IfCondition(new DemoComplexCondPrepare(), o =>
            {
                o.Match = NextStepMatch.All;
                o.Order = 41;
            });
        });
        return client;
    }
}

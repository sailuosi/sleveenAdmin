using sailflow.engine.NextStep;
using SleveenAdmin.Sailflow.Hosting;

namespace SleveenAdmin.Sailflow.ConditionVars;

/// <summary>联调种子缺省变量（SetIfAbsent）。正式业务请另写 <see cref="NextStepPrepare"/>。</summary>
public sealed class DemoComplexCondPrepare : NextStepPrepare
{
    static readonly HashSet<string> Codes = new(StringComparer.OrdinalIgnoreCase)
    {
        SailProductSeed.ComplexCondProcessCode,
        SailProductSeed.ComplexRouteProcessCode
    };

    public override bool IsMatch(NextStepRouteContext ctx)
    {
        var code = ctx?.ProcessCode;
        if (string.IsNullOrWhiteSpace(code))
            return false;
        if (Codes.Contains(code))
            return true;
        return code.StartsWith("complexcond", StringComparison.OrdinalIgnoreCase)
               || code.StartsWith("complexroute", StringComparison.OrdinalIgnoreCase)
               || code.StartsWith("SEED_COMPLEX", StringComparison.OrdinalIgnoreCase);
    }

    protected override void Contribute(NextStepRouteContext ctx)
    {
        SetIfAbsent(ctx, "amount", "500");
        SetIfAbsent(ctx, "urgency", "low");
        SetIfAbsent(ctx, "needLegal", "Y");
        SetIfAbsent(ctx, "needFinance", "N");
        SetIfAbsent(ctx, "days", "2");
        SetIfAbsent(ctx, "skipAudit", "N");
    }
}

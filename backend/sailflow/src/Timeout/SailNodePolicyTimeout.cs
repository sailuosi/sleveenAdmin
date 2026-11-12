using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Sailflow.Timeout;

/// <summary>§A：NodePolicy 超时时长换算与 Overdue 对齐。</summary>
public static class SailNodePolicyTimeout
{
    /// <summary>timeUnit：1 分 · 2 时 · 3 天。</summary>
    public static TimeSpan? ToTimeSpan(int timeDuration, int timeUnit)
    {
        if (timeDuration <= 0) return null;
        return timeUnit switch
        {
            1 => TimeSpan.FromMinutes(timeDuration),
            3 => TimeSpan.FromDays(timeDuration),
            _ => TimeSpan.FromHours(timeDuration) // 2 或未知 → 小时
        };
    }

    public static DateTime? ComputeOverdue(DateTime createdUtc, SailNodePolicyDto policy)
    {
        if (policy == null || !policy.TimeoutHandlerEnable) return null;
        var span = ToTimeSpan(policy.TimeDuration, policy.TimeUnit);
        if (span == null) return null;
        return createdUtc.Add(span.Value);
    }

    /// <summary>与 sf:Boundaries ISO8601 双写时可用的时长串（如 PT6H）。</summary>
    public static string? ToIso8601Duration(SailNodePolicyDto policy)
    {
        if (policy == null || !policy.TimeoutHandlerEnable || policy.TimeDuration <= 0)
            return null;
        return policy.TimeUnit switch
        {
            1 => $"PT{policy.TimeDuration}M",
            3 => $"P{policy.TimeDuration}D",
            _ => $"PT{policy.TimeDuration}H"
        };
    }
}

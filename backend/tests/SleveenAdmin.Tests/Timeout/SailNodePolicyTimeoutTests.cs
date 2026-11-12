using SleveenAdmin.Sailflow.Timeout;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Tests.Timeout;

/// <summary>财务 SLA：应完成时间算式（Activated + Duration）。</summary>
public class SailNodePolicyTimeoutTests
{
    [Fact]
    public void ComputeOverdue_TenMinutes_FromActivated()
    {
        var activated = new DateTime(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);
        var policy = new SailNodePolicyDto
        {
            TimeoutHandlerEnable = true,
            TimeDuration = 10,
            TimeUnit = 1 // 分
        };

        var due = SailNodePolicyTimeout.ComputeOverdue(activated, policy);

        Assert.Equal(activated.AddMinutes(10), due);
    }

    [Fact]
    public void ComputeOverdue_Disabled_ReturnsNull()
    {
        var policy = new SailNodePolicyDto
        {
            TimeoutHandlerEnable = false,
            TimeDuration = 10,
            TimeUnit = 1
        };

        Assert.Null(SailNodePolicyTimeout.ComputeOverdue(DateTime.UtcNow, policy));
    }

    [Fact]
    public void ToTimeSpan_HoursAndDays()
    {
        Assert.Equal(TimeSpan.FromHours(6), SailNodePolicyTimeout.ToTimeSpan(6, 2));
        Assert.Equal(TimeSpan.FromDays(1), SailNodePolicyTimeout.ToTimeSpan(1, 3));
    }
}

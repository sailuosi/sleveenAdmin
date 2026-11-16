using SleveenAdmin.Sailflow.Assignee;
using SleveenAdmin.Sailflow.Entity;

namespace SleveenAdmin.Tests;

public class SurrogateMatchRulesTests
{
    static SailSurrogateEntity Row(
        string id,
        string? processId,
        int enabled = 1,
        DateTime? start = null,
        DateTime? end = null,
        DateTime? updated = null,
        string agent = "agent1") =>
        new()
        {
            Id = id,
            GrantorId = "grantor",
            AgentId = agent,
            ProcessId = processId,
            Enabled = enabled,
            StartAt = start,
            EndAt = end,
            UpdatedAt = updated
        };

    [Fact]
    public void Disabled_Not_In_Window()
    {
        var now = DateTime.UtcNow;
        Assert.False(SurrogateMatchRules.InWindow(Row("1", null, enabled: 0), now));
    }

    [Fact]
    public void Outside_Window_Rejected()
    {
        var now = DateTime.UtcNow;
        Assert.False(SurrogateMatchRules.InWindow(
            Row("1", null, start: now.AddDays(1)), now));
        Assert.False(SurrogateMatchRules.InWindow(
            Row("2", null, end: now.AddDays(-1)), now));
    }

    [Fact]
    public void Exact_ProcessId_Beats_Global()
    {
        var now = DateTime.UtcNow;
        var rows = new[]
        {
            Row("g", null, updated: now.AddHours(-1), agent: "global-agent"),
            Row("e", "P1", updated: now.AddHours(-2), agent: "exact-agent")
        };
        var hit = SurrogateMatchRules.PickActive(rows, "P1", now);
        Assert.NotNull(hit);
        Assert.Equal("exact-agent", hit!.AgentId);
    }

    [Fact]
    public void Falls_Back_To_Global_When_No_Exact()
    {
        var now = DateTime.UtcNow;
        var rows = new[]
        {
            Row("g", null, updated: now, agent: "global-agent"),
            Row("o", "OTHER", updated: now.AddHours(1), agent: "other-agent")
        };
        var hit = SurrogateMatchRules.PickActive(rows, "P1", now);
        Assert.NotNull(hit);
        Assert.Equal("global-agent", hit!.AgentId);
    }

    [Fact]
    public void Same_Scope_Takes_Latest()
    {
        var now = DateTime.UtcNow;
        var rows = new[]
        {
            Row("old", "P1", updated: now.AddDays(-2), agent: "old-agent"),
            Row("new", "P1", updated: now.AddDays(-1), agent: "new-agent")
        };
        var hit = SurrogateMatchRules.PickActive(rows, "P1", now);
        Assert.Equal("new-agent", hit!.AgentId);
    }

    [Fact]
    public void Self_Delegate_Detected()
    {
        Assert.True(SurrogateMatchRules.IsSelfDelegate("u1", "U1"));
        Assert.False(SurrogateMatchRules.IsSelfDelegate("u1", "u2"));
    }
}

using sailflow.utils;
using SleveenAdmin.Sailflow.Utility;

namespace SleveenAdmin.Tests.SendBack;

/// <summary>§J 两维：Normalize + Extra 覆盖 + Previous 收窄矩阵。</summary>
public class SailSendBackPolicyTests
{
    [Theory]
    [InlineData(null, "Starter")]
    [InlineData("", "Starter")]
    [InlineData("prev", "Prev")]
    [InlineData("Prev", "Prev")]
    [InlineData("HISTORY", "History")]
    [InlineData("Starter", "Starter")]
    [InlineData("bogus", "Starter")]
    public void NormalizeTarget_Matrix(string? raw, string expected)
        => Assert.Equal(expected, SailNodePolicyCatalog.NormalizeTarget(raw));

    [Theory]
    [InlineData(null, "ReturnToSender")]
    [InlineData("", "ReturnToSender")]
    [InlineData("forwardnormal", "ForwardNormal")]
    [InlineData("ForwardNormal", "ForwardNormal")]
    [InlineData("ReturnToSender", "ReturnToSender")]
    [InlineData("other", "ReturnToSender")]
    public void NormalizeResubmit_Matrix(string? raw, string expected)
        => Assert.Equal(expected, SailNodePolicyCatalog.NormalizeResubmit(raw));

    [Fact]
    public void Resolve_NodeWins_WhenExtraEmpty()
    {
        var r = SailSendBackPolicy.Resolve("History", "ForwardNormal", null, "  ");
        Assert.Equal("History", r.SendBackTarget);
        Assert.Equal("ForwardNormal", r.ResubmitMode);
    }

    [Fact]
    public void Resolve_ExtraOverrides_PerDimension()
    {
        var r = SailSendBackPolicy.Resolve("History", "ForwardNormal", "Prev", "ReturnToSender");
        Assert.Equal("Prev", r.SendBackTarget);
        Assert.Equal("ReturnToSender", r.ResubmitMode);
    }

    [Fact]
    public void Resolve_ExtraPartial_OnlyTarget()
    {
        var r = SailSendBackPolicy.Resolve("History", "ForwardNormal", "Starter", null);
        Assert.Equal("Starter", r.SendBackTarget);
        Assert.Equal("ForwardNormal", r.ResubmitMode);
    }

    [Fact]
    public void FilterPrevious_Prev_TakesFirst()
    {
        var nodes = SampleNodes();
        var filtered = SailNodePolicyCatalog.FilterPreviousBySendBackTarget(nodes, "Prev");
        Assert.Single(filtered);
        Assert.Equal("A1", filtered[0].ActivityId);
    }

    [Fact]
    public void FilterPrevious_Starter_PrefersStartNode()
    {
        var nodes = SampleNodes();
        var filtered = SailNodePolicyCatalog.FilterPreviousBySendBackTarget(nodes, "Starter");
        Assert.Single(filtered);
        Assert.Equal("Start", filtered[0].ActivityId);
        Assert.Equal(ActivityTypeEnum.StartNode, filtered[0].ActivityType);
    }

    [Fact]
    public void FilterPrevious_History_KeepsAll()
    {
        var nodes = SampleNodes();
        var filtered = SailNodePolicyCatalog.FilterPreviousBySendBackTarget(nodes, "History");
        Assert.Equal(3, filtered.Count);
    }

    [Fact]
    public void FilterPrevious_Empty_Safe()
    {
        Assert.Empty(SailNodePolicyCatalog.FilterPreviousBySendBackTarget(null, "Prev"));
        Assert.Empty(SailNodePolicyCatalog.FilterPreviousBySendBackTarget(new List<NodeView>(), "Starter"));
    }

    static List<NodeView> SampleNodes() =>
    [
        new NodeView { ActivityId = "A1", ActivityName = "一审", ActivityType = ActivityTypeEnum.TaskNode },
        new NodeView { ActivityId = "A2", ActivityName = "二审", ActivityType = ActivityTypeEnum.TaskNode },
        new NodeView { ActivityId = "Start", ActivityName = "开始", ActivityType = ActivityTypeEnum.StartNode }
    ];
}

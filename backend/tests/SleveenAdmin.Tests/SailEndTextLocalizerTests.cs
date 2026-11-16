using SleveenAdmin.Sailflow.Hooks;
using Xunit;

namespace SleveenAdmin.Tests;

public class SailEndTextLocalizerTests
{
    [Theory]
    [InlineData("SelfComplete", "张三", "自行办结")]
    [InlineData("ProxyTaken", "张三", "张三代办结")]
    [InlineData("RaceTaken", "李四", "李四抢办结")]
    [InlineData("TransferredOut", null, "已转办")]
    public void Format_Known_Codes(string endType, string? actor, string expected)
    {
        Assert.Equal(expected, SailEndTextLocalizer.Format(endType, actor));
    }

    [Fact]
    public void Format_Unknown_Returns_Null()
    {
        Assert.Null(SailEndTextLocalizer.Format("Biz.Custom", "x"));
    }
}

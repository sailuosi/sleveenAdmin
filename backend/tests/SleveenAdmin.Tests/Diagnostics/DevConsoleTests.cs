using SleveenAdmin.Foundation.Diagnostics;

namespace SleveenAdmin.Tests.Diagnostics;

public class DevConsoleTests
{
    [Fact]
    public void Format_includes_title_description_and_fields()
    {
        var text = DevConsole.Format(
            "流程钩子 · 调试",
            "事件：OnTaskCreated ／ 操作：Create",
            DevConsole.Field("应用", "SailLeave"),
            DevConsole.Field("业务主键", "abc"));

        Assert.Contains("流程钩子 · 调试", text);
        Assert.Contains("事件：OnTaskCreated", text);
        Assert.Contains("##应用## SailLeave", text);
        Assert.Contains("##业务主键## abc", text);
        Assert.StartsWith("╭─", text);
        Assert.Contains("╰", text);
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SleveenAdmin.Foundation.Diagnostics;

namespace SleveenAdmin.Tests.Diagnostics;

public class DevConsoleFormatterTests
{
    [Fact]
    public void Write_renders_dev_console_block_with_short_category()
    {
        var formatter = new DevConsoleFormatter();
        using var writer = new StringWriter();

        var state = "Executing ObjectResult, writing value of type 'sailflow.utils.ResponseResult'.";
        var entry = new LogEntry<string>(
            LogLevel.Information,
            "Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor",
            new EventId(1),
            state,
            exception: null,
            formatter: static (s, _) => s);

        formatter.Write(in entry, scopeProvider: null, writer);

        var text = writer.ToString();
        Assert.Contains("╭─", text);
        Assert.Contains("宿主 · ObjectResultExecutor", text);
        Assert.Contains("Information", text);
        Assert.Contains("##内容## Executing ObjectResult", text);
        Assert.Contains("╰", text);
    }

    [Fact]
    public void ShortCategory_takes_last_segment()
    {
        Assert.Equal(
            "ObjectResultExecutor",
            DevConsoleFormatter.ShortCategory(
                "Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor"));
        Assert.Equal("host", DevConsoleFormatter.ShortCategory(null));
        Assert.Equal("Simple", DevConsoleFormatter.ShortCategory("Simple"));
    }

    [Fact]
    public void TitleFor_maps_levels()
    {
        Assert.Equal("宿主 错误 · X", DevConsoleFormatter.TitleFor(LogLevel.Error, "X"));
        Assert.Equal("宿主 警告 · X", DevConsoleFormatter.TitleFor(LogLevel.Warning, "X"));
        Assert.Equal("宿主 调试 · X", DevConsoleFormatter.TitleFor(LogLevel.Debug, "X"));
        Assert.Equal("宿主 · X", DevConsoleFormatter.TitleFor(LogLevel.Information, "X"));
    }

    [Fact]
    public void Write_includes_exception_fields()
    {
        var formatter = new DevConsoleFormatter();
        using var writer = new StringWriter();
        var ex = new InvalidOperationException("boom");

        var entry = new LogEntry<string>(
            LogLevel.Error,
            "SleveenAdmin.Web.Api",
            new EventId(0),
            "failed",
            ex,
            static (s, _) => s);

        formatter.Write(in entry, scopeProvider: null, writer);

        var text = writer.ToString();
        Assert.Contains("宿主 错误 · Api", text);
        Assert.Contains("##异常## InvalidOperationException", text);
        Assert.Contains("##详情## boom", text);
    }
}
